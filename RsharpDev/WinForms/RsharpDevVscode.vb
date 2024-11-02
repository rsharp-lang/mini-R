Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports Microsoft.Web.WebView2.Core
Imports WeifenLuo.WinFormsUI.Docking
Imports My
Imports RDev
Imports System.Threading

Public Class RsharpDevVscode
    Implements ISaveHandle
    Implements IFileReference
    Implements Viewer

    ''' <summary>
    ''' the file path of the script file
    ''' </summary>
    ''' <returns></returns>
    Public Property FilePath As String Implements IFileReference.FilePath
    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType

    Dim ready As Boolean = False

    Public Shared ReadOnly Property vscode_url As String
        Get
            Return $"http://localhost:{MyApplication.lsp_server}/index.html"
        End Get
    End Property

    Private Async Sub RsharpDevVscode_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebKit.Init(WebView21)
    End Sub

    Private Sub WebView21_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles WebView21.CoreWebView2InitializationCompleted
        WebView21.CoreWebView2.Navigate(vscode_url)
        Thread.Sleep(100)
        ready = True
        View("G:\mzkit\Rscript\Library\MSI_app\test\ggplot_msi_filters.R")
    End Sub

    Public Function View(file As String) As DockContent Implements Viewer.View
        If file.FileExists Then
            FilePath = file
            TabText = FilePath.FileName

            Do While Not ready
                Thread.Sleep(10)
            Loop

            WebView21.ExecuteScriptAsync($"run_vscode('{file}','r');").Wait()
        End If

        Return Me
    End Function

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save

    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save

    End Function

    Public Function Save(file As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save

    End Function
End Class