Imports Galaxy.Workbench
Imports Microsoft.Web.WebView2.Core

Public Class FormHtmlViewer

    Dim html_str As String
    Dim initialized As Boolean = False

    Private Async Sub FormHtmlViewer_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)
    End Sub

    Private Sub WebView21_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles WebView21.CoreWebView2InitializationCompleted
        initialized = True

        If Not html_str Is Nothing Then
            Call WebViewLoader.NavigateToLargeString(WebView21, html_str)
        End If
    End Sub

    Public Function ViewHtml(html As String) As FormHtmlViewer
        html_str = html

        If initialized Then
            Call WebViewLoader.NavigateToLargeString(WebView21, html_str)
        End If

        Return Me
    End Function
End Class