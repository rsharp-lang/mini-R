Imports System.IO
Imports System.Text
Imports Galaxy.Workbench
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualStudio.WinForms.Docking
Imports RDev

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

    Private Sub RsharpDevVscode_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim console = CommonRuntime.GetOutputWindow

        TabText = "New script"
        Text = TabText

        vscode.Dock = DockStyle.Fill
        vscode.lspLog = Sub(msg) console.AppendTo("lsp", msg)
        vscode.SetMinimapEnabled(True)
        vscode.SetLineNumbers(LineNumbersMode.On)
    End Sub

    Public Function View(file As String) As DockContent Implements Viewer.View
        If file.FileExists Then
            FilePath = file
            TabText = FilePath.FileName
            Text = TabText
        ElseIf file.StringEmpty(, True) Then
            TabText = "New script"
            Text = TabText
        End If

        vscode.FilePath = FilePath
        vscode.SetContent(FilePath.ReadAllText, "r")

        Return Me
    End Function

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save

    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save

    End Function

    Public Function Save(file As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save

    End Function
End Class