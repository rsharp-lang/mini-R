Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.Web.WebView2.Core

Public Class VsCodeEditor : Implements IFileReference

    Public Shared ReadOnly Property vscode_url As String
        Get
            Return $"http://localhost:{lspclient.lsp_server}/index.html"
        End Get
    End Property

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Return {}
        End Get
    End Property

    Private Async Sub VsCodeEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebKit.Init(WebView21)
    End Sub

    Private Sub WebView21_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles WebView21.CoreWebView2InitializationCompleted
        WebView21.CoreWebView2.Navigate(vscode_url)
    End Sub

    Private Sub WebView21_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles WebView21.NavigationCompleted
        If FilePath Is Nothing Then
            WebView21.ExecuteScriptAsync($"run_vscode('','r');")
        Else
            WebView21.ExecuteScriptAsync($"run_vscode('{FilePath.Replace("\", "/")}','r');")
        End If
    End Sub
End Class
