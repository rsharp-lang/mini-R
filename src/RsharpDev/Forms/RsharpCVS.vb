Imports Galaxy.Workbench
Imports RDev

Public Class RsharpCVS

    Dim editor As RsharpDevVscode

    Private Sub RsharpCVS_Load(sender As Object, e As EventArgs) Handles Me.Load
        ScriptManagerView1.OpenEditor = AddressOf OpenEditor
    End Sub

    Private Function OpenEditor() As IEditor
        If editor Is Nothing OrElse editor.editorClosed Then
            editor = CommonRuntime.ShowDocument(Of RsharpDevVscode)
        End If

        Return editor
    End Function
End Class