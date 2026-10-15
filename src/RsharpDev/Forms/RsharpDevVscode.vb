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
    Implements Editor

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

    ' 20261015 the file save function can not use the Using for make auto dispose of the file stream
    ' due to the reason of save function in used async method
    ' Using closure will close the stream before get vscode editor text content
    ' case the error of can not access the closed stream.

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        FilePath = path
        TabText = FilePath.FileName

        Dim s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
        Return Save(s, encoding)
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        FilePath = path
        TabText = FilePath.FileName

        Dim s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
        Return Save(s, encoding.CodePage)
    End Function

    Public Function Save(file As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Call AsyncSave(file, encoding)
        Return True
    End Function

    Private Async Sub AsyncSave(file As Stream, encoding As Encoding) Implements Editor.Save
        Dim sw As New StreamWriter(file, encoding, leaveOpen:=True)
        Call sw.WriteLine(Await vscode.GetContent)
        Call sw.Flush()
        Call file.Dispose()
    End Sub

    Protected Overrides Sub SaveDocument()
        If FilePath.StringEmpty(, True) Then
            Using file As New SaveFileDialog With {.Filter = "R# script(*.r)|*.R"}
                If file.ShowDialog = DialogResult.OK Then
                    Call Save(file.FileName, Encodings.UTF8WithoutBOM)
                End If
            End Using
        Else
            Call Save(FilePath, Encodings.UTF8WithoutBOM)
        End If
    End Sub

    Public Sub SetContent(scriptText As String, name As String) Implements Editor.SetContent
        vscode.SetContent(scriptText, "r")
        TabText = name
        FilePath = name
    End Sub
End Class