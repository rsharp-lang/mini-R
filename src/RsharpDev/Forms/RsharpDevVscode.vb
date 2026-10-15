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

    Private Sub vscode_EditorTextChanged(sender As Object, e As String) Handles vscode.EditorTextChanged

    End Sub

    Private Sub vscode_IsDirtyChanged(sender As Object, e As Boolean) Handles vscode.IsDirtyChanged
        TabText = $"{FilePath.FileName} *"
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
        Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Return Save(s, encoding)
        End Using
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Return Save(s, encoding.CodePage)
        End Using
    End Function

    Public Function Save(file As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Dim sw As New StreamWriter(file, encoding, leaveOpen:=True)
        Call sw.WriteLine(vscode.GetContent.GetAwaiter.GetResult)
        Call sw.Flush()

        Return True
    End Function
End Class