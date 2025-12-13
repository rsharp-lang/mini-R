Imports Galaxy.Workbench

Public Class FormHtmlViewer

    Private Async Sub FormHtmlViewer_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)
    End Sub

    Public Function ViewHtml(html As String) As FormHtmlViewer
        Call WebViewLoader.NavigateToLargeString(WebView21, html)
        Return Me
    End Function
End Class