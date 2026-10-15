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

    Private Async Sub VsCodeEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)

        ' intercept every request to our virtual host and serve it from the zip
        WebView21.CoreWebView2.AddWebResourceRequestedFilter(resourceFilter, CoreWebView2WebResourceContext.All)
        AddHandler WebView21.CoreWebView2.WebResourceRequested, AddressOf OnWebResourceRequested
        AddHandler WebView21.CoreWebView2.WebMessageReceived, AddressOf OnWebMessageReceived

        ' navigate to the virtual host; the document itself comes from the embedded html
        WebView21.CoreWebView2.Navigate(host)
    End Sub

    Private Sub OnWebResourceRequested(sender As Object, e As CoreWebView2WebResourceRequestedEventArgs)
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

    Private Sub OnWebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        Dim message = e.TryGetWebMessageAsString()
        ' the page posts "initialized" once the editor is ready; hook point for callers
        If message = "initialized" Then
            ' editor ready
        End If
    End Sub

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
    ''' set the editor content and optionally its language from the host.
    ''' </summary>
    Public Async Function SetContent(content As String, Optional language As String = Nothing) As Task
        code = content
        lang = language

        If Not WebView21.CoreWebView2 Is Nothing Then
            Await SetContentInternal(content, language)
        End If
    End Function

    Private Async Function SetContentInternal(content As String, Optional language As String = Nothing) As Task
        Dim script As String

        If String.IsNullOrEmpty(language) Then
            script = "window.setEditorContent(" & System.Text.Json.JsonSerializer.Serialize(content) & ");"
        Else
            script = "window.setEditorContent(" & System.Text.Json.JsonSerializer.Serialize(content) & "," & System.Text.Json.JsonSerializer.Serialize(language) & ");"
        End If

        Await WebView21.CoreWebView2.ExecuteScriptAsync(script)
    End Function

    ''' <summary>
    ''' get the current editor content (returns a JSON-unescaped string).
    ''' </summary>
    Public Async Function GetContent() As Task(Of String)
        If WebView21.CoreWebView2 Is Nothing Then Return ""
        Dim result = Await WebView21.CoreWebView2.ExecuteScriptAsync("window.getEditorContent()")
        If String.IsNullOrEmpty(result) Then Return ""
        Return System.Text.Json.JsonSerializer.Deserialize(Of String)(result)
    End Function

    Private Sub WebView21_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles WebView21.CoreWebView2InitializationCompleted

    End Sub
End Class
