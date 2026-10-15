Imports System.IO
Imports System.Collections.Generic
Imports System.Text.Json
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

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

    ''' <summary>
    ''' locate an embedded manifest resource by case-insensitive suffix match.
    ''' the embedded monaco resources are named "RDev.monaco-editor.zip" /
    ''' "RDev.monaco-editor.html" (no folder prefix), so match by suffix.
    ''' </summary>
    Private Shared Function GetManifestResource(suffix As String) As Stream
        Dim asm As System.Reflection.Assembly = GetType(VsCodeEditor).Assembly
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
    End Sub

    Private Async Sub VsCodeEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)

        ' intercept every request to our virtual host and serve it from the zip
        WebView21.CoreWebView2.AddWebResourceRequestedFilter(resourceFilter, CoreWebView2WebResourceContext.All)
        AddHandler WebView21.CoreWebView2.WebResourceRequested, AddressOf OnWebResourceRequested
        AddHandler WebView21.CoreWebView2.WebMessageReceived, AddressOf OnWebMessageReceived

        ' inject MonacoEnvironment.getWorkerUrl before any page script runs so the
        ' workers are served from the zip (which has no base/worker/workerMain.js).
        Await WebView21.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(BuildWorkerScript())

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
            End If

            Dim zipPath = ResolveZipPath(localPath)
            If zipPath Is Nothing Then
                e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(Nothing, 404, "Not Found", "Content-Type: text/plain")
                Return
            End If

            Dim stream As Stream = cdn.OpenFile(zipPath)
            e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(stream, 200, "OK", "Content-Type: " & MimeFromPath(zipPath) & vbCrLf)
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
        For Each prefix As String In {"min/vs", "min"}
            Dim candidate = prefix & localPath
            If cdn.FileExists(candidate) Then
                Return candidate
            End If
        Next
        Return Nothing
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
    ''' build the MonacoEnvironment.getWorkerUrl injector. the worker file names are
    ''' hashed in the packaged zip, so discover them from the assets folder.
    ''' </summary>
    Private Function BuildWorkerScript() As String
        Dim labelToFile As New Dictionary(Of String, String)
        Dim prefixToLabel As New Dictionary(Of String, String) From {
            {"editor.worker", "editor"},
            {"json.worker", "json"},
            {"css.worker", "css"},
            {"html.worker", "html"},
            {"ts.worker", "typescript"}
        }

        For Each entry As String In cdn.GetFiles()
            If entry.StartsWith("min/vs/assets/", StringComparison.OrdinalIgnoreCase) AndAlso entry.IndexOf(".worker", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Dim baseName = IO.Path.GetFileName(entry)
                For Each kvp In prefixToLabel
                    If baseName.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase) Then
                        If Not labelToFile.ContainsKey(kvp.Value) Then
                            labelToFile(kvp.Value) = baseName
                        End If
                        If kvp.Value = "typescript" AndAlso Not labelToFile.ContainsKey("javascript") Then
                            labelToFile("javascript") = baseName
                        End If
                    End If
                Next
            End If
        Next

        Dim sb As New System.Text.StringBuilder()
        sb.Append("window.MonacoEnvironment=window.MonacoEnvironment||{};")
        sb.Append("window.MonacoEnvironment.getWorkerUrl=function(workerId,label){")
        sb.Append("var w=")
        sb.Append("{")
        Dim first = True
        For Each kvp In labelToFile
            If Not first Then sb.Append(",")
            first = False
            sb.Append("""").Append(kvp.Key).Append(""":""").Append(kvp.Value).Append("""")
        Next
        sb.Append("};")
        sb.Append("var f=w[label]||w['editor'];")
        sb.Append("return '").Append(host).Append("assets/'+f;")
        sb.Append("};")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' set the editor content and optionally its language from the host.
    ''' </summary>
    Public Async Function SetContent(content As String, Optional language As String = Nothing) As Task
        If WebView21.CoreWebView2 Is Nothing Then Return
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

End Class
