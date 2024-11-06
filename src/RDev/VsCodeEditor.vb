Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Http
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

    Public ReadOnly Property ScriptText As String
        Get
            Dim getter_js As String = "rstudio.getCodeText();"
            Dim fetch = WebView21.ExecuteScriptAsync(getter_js)
            Call fetch.Wait()
            Return fetch.Result
        End Get
    End Property

    Public Event OnFocus()
    Public Event EditCode()

    Private Async Sub VsCodeEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebKit.Init(WebView21)
    End Sub

    Private Sub WebView21_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles WebView21.CoreWebView2InitializationCompleted
        WebView21.CoreWebView2.Navigate(vscode_url)
    End Sub

    Dim ready As Boolean = False

    Private Sub WebView21_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles WebView21.NavigationCompleted
        If FilePath Is Nothing Then
            WebView21.ExecuteScriptAsync($"run_vscode('','r');")
        Else
            WebView21.ExecuteScriptAsync($"run_vscode('{FilePath.Replace("\", "/")}','r');")
        End If

        ready = True
    End Sub

    Public Sub LoadScript(str As String)
        Call Task.Run(
            Sub()
                Do While App.Running AndAlso Not ready
                    Call Thread.Sleep(1)
                Loop

                Call WebView21.ExecuteScriptAsync($"run_vscode('base64://{str.Base64String}','r');")
            End Sub)
    End Sub

    Private Sub WebView21_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs) Handles WebView21.NavigationStarting
        ready = False
    End Sub

    Private Sub WebView21_GotFocus(sender As Object, e As EventArgs) Handles WebView21.GotFocus
        RaiseEvent OnFocus()
    End Sub

    Private Sub webView2_WebMessageReceived(sender As Object, args As CoreWebView2WebMessageReceivedEventArgs) Handles WebView21.WebMessageReceived
        Dim message = args.TryGetWebMessageAsString()

        If message = "input" OrElse message = "change" Then
            RaiseEvent EditCode()
        End If
    End Sub

End Class
