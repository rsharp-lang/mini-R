Imports System.IO
Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.Web.WebView2.Core

Public Class VsCodeEditor : Implements IFileReference

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Return {}
        End Get
    End Property

    ' virtual host used to serve the monaco assets entirely from the embedded zip
    Shared ReadOnly host As String = "https://monaco.local/"
    Shared ReadOnly resourceFilter As String = "https://monaco.local/*"

    Shared ReadOnly cdn As ZipStream
    Shared ReadOnly editor_js As String

    Dim code As String
    Dim lang As String

    ' single shared command channel (from galaxy/WebViewHelper). WebView2CommandChannel.Send
    ' only really posts when IsReady = True, so we must set it after Init and keep the instance.
    Dim _channel As WebView2CommandChannel
    ' becomes True once the frontend signals EditorReady via postMessage.
    Dim _editorReady As Boolean

    ' commands queued until the frontend reports EditorReady (avoids racing the page load).
    Dim _pending As New List(Of KeyValuePair(Of String, Object))

    ' mirrored editor state reported by the frontend through Outgoing messages.
    Dim _isDirty As Boolean
    Dim _languages As String()
    ' pending tcs used by GetLanguagesAsync to await the Languages reply.
    Dim _languagesTcs As TaskCompletionSource(Of String())

    ''' <summary>
    ''' raised when the frontend editor content changes (debounced on the JS side).
    ''' distinct from the base UserControl.TextChanged event.
    ''' </summary>
    Public Event EditorTextChanged As EventHandler(Of String)

    ''' <summary>
    ''' raised when the frontend editor dirty/modified state changes.
    ''' </summary>
    Public Event IsDirtyChanged As EventHandler(Of Boolean)

    ''' <summary>
    ''' raised when the frontend returns the list of available language ids
    ''' (in response to GetLanguagesAsync / GetLanguages).
    ''' </summary>
    Public Event LanguagesReceived As EventHandler(Of String())

    ''' <summary>
    ''' true when the editor content differs from the last baseline (after SetContent / MarkAsSaved).
    ''' </summary>
    Public ReadOnly Property IsModified As Boolean
        Get
            Return _isDirty
        End Get
    End Property

    ''' <summary>
    ''' the most recent list of language ids reported by the frontend.
    ''' </summary>
    Public ReadOnly Property Languages As String()
        Get
            Return _languages
        End Get
    End Property

    ''' <summary>
    ''' locate an embedded manifest resource by case-insensitive suffix match.
    ''' the embedded monaco resources are named "RDev.monaco-editor.zip" /
    ''' "RDev.monaco-editor.html" (no folder prefix), so match by suffix.
    ''' </summary>
    Private Shared Function GetManifestResource(suffix As String) As Stream
        Dim asm As Assembly = GetType(VsCodeEditor).Assembly
        For Each name As String In asm.GetManifestResourceNames()
            If name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then
                Return asm.GetManifestResourceStream(name)
            End If
        Next
        Throw New System.Resources.MissingManifestResourceException("Embedded resource ending with '" & suffix & "' was not found in assembly " & asm.FullName & ".")
    End Function

    Public Property lspLog As Action(Of String) = AddressOf System.Diagnostics.Debug.WriteLine

    Shared Sub New()
        ' load the embedded monaco-editor.zip into the in-memory zip virtual fs.
        ' the manifest resource stream is released after copying into a standalone
        ' memory stream: ZipStream keeps a reference to the stream it is given and
        ' reads from it lazily, so the buffer must remain alive (it is owned by cdn).
        Using res As Stream = GetManifestResource("monaco-editor.zip")
            Dim buffer As New MemoryStream
            res.CopyTo(buffer)
            buffer.Seek(0, SeekOrigin.Begin)
            cdn = New ZipStream(buffer, is_readonly:=True)
        End Using

        Using res As Stream = GetManifestResource("editor.js")
            Dim buffer As New MemoryStream
            res.CopyTo(buffer)
            buffer.Seek(0, SeekOrigin.Begin)
            editor_js = Encoding.UTF8.GetString(buffer.ToArray)
        End Using
    End Sub

    Dim WithEvents _core As CoreWebView2

    Shared _lsp As lspclient

    Private Async Sub VsCodeEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)

        ' create the shared command channel and mark it ready: WebView2CommandChannel.Send
        ' only posts a message when IsReady = True (it is False by default).
        _channel = New WebView2CommandChannel(WebView21) With {.IsReady = True}
        _editorReady = False
        _core = WebView21.CoreWebView2

        If _lsp Is Nothing Then
            _lsp = Await New lspclient(document.R, lspLog).Init()
        End If

        ' intercept every request to our virtual host and serve it from the zip
        WebView21.CoreWebView2.AddWebResourceRequestedFilter(resourceFilter, CoreWebView2WebResourceContext.All)

        ' navigate to the virtual host; the document itself comes from the embedded html
        _lspLog("open monaco-editor.")

        WebView21.CoreWebView2.Navigate(host)
    End Sub

    Private Sub OnWebResourceRequested(sender As Object, e As CoreWebView2WebResourceRequestedEventArgs) Handles _core.WebResourceRequested
        Dim def = e.GetDeferral()
        Try
            Dim localPath = New Uri(e.Request.Uri).LocalPath

            If localPath = "/" OrElse localPath = "" Then
                ' the monaco editor page itself, served from the embedded html resource
                Using html As Stream = GetManifestResource("monaco-editor.html")
                    Dim ms As New MemoryStream
                    html.CopyTo(ms)
                    ms.Seek(0, SeekOrigin.Begin)
                    e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(ms, 200, "OK", "Content-Type: text/html; charset=utf-8")
                End Using
                Return
            ElseIf localPath.Trim("/"c) = "editor.js" Then
                e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(New MemoryStream(Encoding.UTF8.GetBytes(editor_js)), 200, "OK", "Content-Type: text/javascript")
            Else
                Dim zipPath = ResolveZipPath(localPath)
                If zipPath Is Nothing Then
                    e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(Nothing, 404, "Not Found", "Content-Type: text/plain")
                    Return
                End If

                Dim stream As Stream = cdn.OpenFile(zipPath)
                e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(stream, 200, "OK", "Content-Type: " & MimeFromPath(zipPath) & vbCrLf)
            End If
        Catch ex As Exception
            If e.Response Is Nothing Then
                e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(Nothing, 500, "Error", "Content-Type: text/plain")
            End If
        Finally
            def.Complete()
        End Try
    End Sub

    Private Sub OnWebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs) Handles _core.WebMessageReceived
        Dim message As String = e.WebMessageAsJson

        ' parse with System.Text.Json for robust, typed payload extraction.
        Using doc As System.Text.Json.JsonDocument = System.Text.Json.JsonDocument.Parse(message)
            Dim root = doc.RootElement

            Dim typeEl As System.Text.Json.JsonElement
            If Not root.TryGetProperty("type", typeEl) Then
                Return
            End If

            Dim type As String = typeEl.GetString()

            Select Case type
                Case "EditorReady"
                    _editorReady = True

                    ' flush any commands queued before the frontend became ready
                    Call FlushPending()

                Case "TextChanged"
                    ' payload: the current editor text
                    Dim text As String = ""
                    Dim payloadEl As System.Text.Json.JsonElement

                    If root.TryGetProperty("payload", payloadEl) Then
                        text = payloadEl.GetString()
                    End If

                    RaiseEvent EditorTextChanged(Me, text)

                Case "IsDirtyChanged"
                    ' payload: boolean dirty flag
                    Dim isDirty As Boolean = False
                    Dim payloadEl As System.Text.Json.JsonElement

                    If root.TryGetProperty("payload", payloadEl) Then
                        isDirty = payloadEl.GetBoolean()
                    End If

                    _isDirty = isDirty
                    RaiseEvent IsDirtyChanged(Me, isDirty)

                Case "Languages"
                    ' payload: array of language ids
                    Dim langs As String() = {}
                    Dim payloadEl As System.Text.Json.JsonElement

                    If root.TryGetProperty("payload", payloadEl) AndAlso
                       payloadEl.ValueKind = System.Text.Json.JsonValueKind.Array Then

                        Dim list As New List(Of String)

                        For Each item In payloadEl.EnumerateArray()
                            list.Add(item.GetString())
                        Next

                        langs = list.ToArray()
                    End If

                    _languages = langs
                    RaiseEvent LanguagesReceived(Me, langs)

                    If _languagesTcs IsNot Nothing Then
                        Call _languagesTcs.TrySetResult(langs)
                        _languagesTcs = Nothing
                    End If

                Case "lsp/request"
                    ' payload: { id, method, params } coming from a Monaco provider
                    Dim payloadEl As System.Text.Json.JsonElement

                    If root.TryGetProperty("payload", payloadEl) Then
                        Call HandleLspRequest(payloadEl)
                    End If

                Case "lsp/notify"
                    ' payload: { method, params } -- document synchronization
                    Dim payloadEl As System.Text.Json.JsonElement

                    If root.TryGetProperty("payload", payloadEl) Then
                        Call HandleLspNotify(payloadEl)
                    End If
            End Select
        End Using
    End Sub

    ' ===================== in-process R# language server bridge =====================

    ''' <summary>
    ''' forward a provider request coming from the Monaco page to the in-process R#
    ''' language server and post the result back as an lsp/response message. the
    ''' request runs on a background thread (the call may parse the whole document)
    ''' and is serialized with the notification path.
    ''' </summary>
    Private Async Sub HandleLspRequest(payload As System.Text.Json.JsonElement)
        If _channel Is Nothing OrElse _lsp Is Nothing Then Return

        Dim id As Long = payload.GetProperty("id").GetInt64()
        Dim method As String = payload.GetProperty("method").GetString()
        Dim paramsJson As String = ExtractParams(payload)
        Dim reply = Await _lsp.handleLspRequest(id, method, paramsJson)

        ' WebView2CommandChannel.Send calls PostWebMessageAsJson,
        ' which must be invoked on the UI thread
        Call _channel.Send("lsp/response", reply)
        Call _lspLog($"lsp/response: {WebView2CommandChannel.ToJson(any:=reply)}")
    End Sub

    ''' <summary>
    ''' forward a document synchronization notification (didOpen / didChange /
    ''' didClose) to the language server. notifications never produce a reply.
    ''' </summary>
    Private Sub HandleLspNotify(payload As System.Text.Json.JsonElement)
        If _lsp Is Nothing Then Return

        Dim method As String = payload.GetProperty("method").GetString()
        Dim paramsJson As String = ExtractParams(payload)

        Call _lsp.handleLspNotification(method, paramsJson)
    End Sub

    ''' <summary>
    ''' read the raw json text of the "params" member of an lsp payload, falling
    ''' back to the json null literal when the member is missing or null.
    ''' </summary>
    Private Shared Function ExtractParams(payload As System.Text.Json.JsonElement) As String
        Dim paramsEl As System.Text.Json.JsonElement

        If payload.TryGetProperty("params", paramsEl) AndAlso
           paramsEl.ValueKind <> System.Text.Json.JsonValueKind.Null Then
            Return paramsEl.GetRawText()
        End If

        Return "null"
    End Function

    ''' <summary>
    ''' map a request local path to a zip entry. the embedded zip uses the standard
    ''' min/vs/ layout; fall back to the "min" prefix too for robustness.
    ''' </summary>
    Private Function ResolveZipPath(localPath As String) As String
        Dim prefix As String = "monaco-editor/"
        Dim candidate = prefix & localPath

        If cdn.FileExists(candidate) Then
            Return candidate
        Else
            Return Nothing
        End If
    End Function

    Private Function MimeFromPath(path As String) As String
        Select Case IO.Path.GetExtension(path).ToLowerInvariant()
            Case ".js", ".mjs" : Return "application/javascript"
            Case ".css" : Return "text/css"
            Case ".json" : Return "application/json"
            Case ".html", ".htm" : Return "text/html; charset=utf-8"
            Case ".ttf" : Return "font/ttf"
            Case ".woff" : Return "application/font-woff"
            Case ".woff2" : Return "font/woff2"
            Case ".svg" : Return "image/svg+xml"
            Case ".png" : Return "image/png"
            Case ".gif" : Return "image/gif"
            Case ".ico" : Return "image/x-icon"
            Case ".map" : Return "application/json"
            Case Else : Return "application/octet-stream"
        End Select
    End Function

    ''' <summary>
    ''' send a command to the frontend editor. if the WebView2 channel or the
    ''' frontend is not ready yet, the command is queued and flushed on EditorReady.
    ''' </summary>
    Private Sub SendCommand(type As String, payload As Object)
        If _channel IsNot Nothing AndAlso _channel.IsReady AndAlso _editorReady Then
            Call _channel.Send(type, payload)
        Else
            _pending.Add(New KeyValuePair(Of String, Object)(type, payload))
        End If
    End Sub

    ''' <summary>
    ''' flush all commands queued before the frontend became ready.
    ''' </summary>
    Private Sub FlushPending()
        For Each cmd In _pending
            Call _channel.Send(cmd.Key, cmd.Value)
        Next

        _pending.Clear()
    End Sub

    ''' <summary>
    ''' set the editor content and optionally its language from the host.
    ''' the text is pushed through the WebView2 command channel; if the frontend
    ''' is not ready yet the command is queued and flushed on EditorReady.
    ''' </summary>
    Public Sub SetContent(content As String, Optional language As String = Nothing)
        code = content
        lang = language

        Call SendCommand(EditorCommandType.SetText.ToString, code)

        If Not lang.StringEmpty Then
            Call SendCommand(EditorCommandType.SetLanguage.ToString, lang)
        End If
    End Sub

    ''' <summary>
    ''' get the current editor content (returns a JSON-unescaped string).
    ''' </summary>
    Public Async Function GetContent() As Task(Of String)
        If WebView21.CoreWebView2 Is Nothing Then
            Return ""
        Else
            Dim result = Await WebView21.CoreWebView2.ExecuteScriptAsync("window.getEditorContent()")

            If String.IsNullOrEmpty(result) Then
                Return ""
            Else
                Return System.Text.Json.JsonSerializer.Deserialize(Of String)(result)
            End If
        End If
    End Function

    ' ===================== editor options (Incoming contract, editor.js:9-19) =====================

    ''' <summary>
    ''' toggle the read-only mode of the editor.
    ''' </summary>
    Public Sub SetReadOnly(value As Boolean)
        Call SendCommand(EditorCommandType.SetReadOnly.ToString, value)
    End Sub

    ''' <summary>
    ''' mark the current content as saved: clears the dirty flag and resets the
    ''' baseline version on the frontend.
    ''' </summary>
    Public Sub MarkAsSaved()
        Call SendCommand(EditorCommandType.MarkAsSaved.ToString, Nothing)
    End Sub

    ''' <summary>
    ''' request the list of available language ids from the frontend. completes when
    ''' the frontend replies via the Languages message (2s timeout, returns empty on
    ''' not-ready or timeout). the <see cref="LanguagesReceived"/> event also fires.
    ''' </summary>
    Public Async Function GetLanguagesAsync() As Task(Of String())
        If _channel Is Nothing OrElse Not _channel.IsReady OrElse Not _editorReady Then
            Return {}
        End If

        _languagesTcs = New TaskCompletionSource(Of String())()
        Call SendCommand(EditorCommandType.GetLanguages.ToString, Nothing)

        Dim completed = Await Task.WhenAny(_languagesTcs.Task, Task.Delay(2000))

        If completed Is _languagesTcs.Task Then
            Return Await _languagesTcs.Task
        End If

        ' timeout: discard the pending tcs so a late reply just raises the event.
        _languagesTcs = Nothing
        Return {}
    End Function

    ''' <summary>
    ''' set the editor language (e.g. "r", "vb", "json").
    ''' </summary>
    Public Sub SetLanguage(id As String)
        Call SendCommand(EditorCommandType.SetLanguage.ToString, id)
    End Sub

    ''' <summary>
    ''' update a single Monaco editor option. pass the option key from the
    ''' <see cref="MonacoEditorOptions"/> constants module (e.g.
    ''' UpdateOption(MonacoEditorOptions.TabSize, 4) or
    ''' UpdateOption(MonacoEditorOptions.Minimap, New Dictionary(...) From {{"enabled", False}})).
    ''' the option name is forwarded verbatim to monaco.editor.updateOptions({ [option]: value }).
    ''' </summary>
    Public Sub UpdateOption(optionName As String, optionValue As Object)
        Dim payload = New Dictionary(Of String, Object) From {
            {"option", optionName},
            {"value", optionValue}
        }

        Call SendCommand(EditorCommandType.UpdateOption.ToString, payload)
    End Sub

    ' ===================== strongly-typed convenience wrappers for common options =====================

    ''' <summary>
    ''' set the number of spaces per tab (monaco tabSize).
    ''' </summary>
    Public Sub SetTabSize(value As Integer)
        Call UpdateOption(MonacoEditorOptions.TabSize, value)
    End Sub

    ''' <summary>
    ''' set the editor font size in pixels (monaco fontSize).
    ''' </summary>
    Public Sub SetFontSize(value As Integer)
        Call UpdateOption(MonacoEditorOptions.FontSize, value)
    End Sub

    ''' <summary>
    ''' set the editor font family (monaco fontFamily).
    ''' </summary>
    Public Sub SetFontFamily(value As String)
        Call UpdateOption(MonacoEditorOptions.FontFamily, value)
    End Sub

    ''' <summary>
    ''' set the word-wrap behavior (monaco wordWrap).
    ''' </summary>
    Public Sub SetWordWrap(mode As WordWrapMode)
        Call UpdateOption(MonacoEditorOptions.WordWrap, WordWrapToString(mode))
    End Sub

    ''' <summary>
    ''' set the line-number rendering mode (monaco lineNumbers).
    ''' </summary>
    Public Sub SetLineNumbers(mode As LineNumbersMode)
        Call UpdateOption(MonacoEditorOptions.LineNumbers, LineNumbersToString(mode))
    End Sub

    ''' <summary>
    ''' enable or disable the minimap (monaco minimap.enabled).
    ''' </summary>
    Public Sub SetMinimapEnabled(enabled As Boolean)
        Call UpdateOption(MonacoEditorOptions.Minimap, New Dictionary(Of String, Object) From {{"enabled", enabled}})
    End Sub

    Private Shared Function WordWrapToString(mode As WordWrapMode) As String
        Select Case mode
            Case WordWrapMode.Off : Return "off"
            Case WordWrapMode.[On] : Return "on"
            Case WordWrapMode.WordWrapColumn : Return "wordWrapColumn"
            Case WordWrapMode.Bounded : Return "bounded"
        End Select
        Return "off"
    End Function

    Private Shared Function LineNumbersToString(mode As LineNumbersMode) As String
        Select Case mode
            Case LineNumbersMode.[On] : Return "on"
            Case LineNumbersMode.Off : Return "off"
            Case LineNumbersMode.Relative : Return "relative"
            Case LineNumbersMode.Interval : Return "interval"
        End Select
        Return "on"
    End Function

    ''' <summary>
    ''' register a custom Monaco theme. rules/colors are passed as already-built JSON
    ''' objects (e.g. Dictionary(Of String, Object) or JsonElement) matching the
    ''' monaco.editor.defineTheme signature.
    ''' </summary>
    Public Sub RegisterTheme(name As String, base As String, inherit As Boolean, rules As Object, colors As Object)
        Dim payload = New Dictionary(Of String, Object) From {
            {"name", name},
            {"base", base},
            {"inherit", inherit},
            {"rules", rules},
            {"colors", colors}
        }

        Call SendCommand(EditorCommandType.RegisterTheme.ToString, payload)
    End Sub

    ''' <summary>
    ''' apply a previously registered (or built-in) theme by name.
    ''' </summary>
    Public Sub SetTheme(themeName As String)
        Call SendCommand(EditorCommandType.SetTheme.ToString, themeName)
    End Sub

    ''' <summary>
    ''' register a custom language. <paramref name="monarch"/> and
    ''' <paramref name="configuration"/> are JS source literals (object expressions)
    ''' evaluated on the frontend via new Function(...), matching editor.js
    ''' handleRegisterLanguage.
    ''' </summary>
    Public Sub RegisterLanguage(id As String, Optional monarch As String = Nothing, Optional configuration As String = Nothing)
        Dim payload = New Dictionary(Of String, Object) From {
            {"id", id}
        }

        If monarch IsNot Nothing Then
            payload("monarch") = monarch
        End If

        If configuration IsNot Nothing Then
            payload("configuration") = configuration
        End If

        Call SendCommand(EditorCommandType.RegisterLanguage.ToString, payload)
    End Sub
End Class
