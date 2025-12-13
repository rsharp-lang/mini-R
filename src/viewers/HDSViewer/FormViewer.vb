Imports Galaxy.Data.JSON
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem

Public Class FormViewer

    Dim WithEvents packTree As JsonViewer
    Dim pack As StreamPack

    Private Sub FormViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        packTree = New JsonViewer
        packTree.Dock = DockStyle.Fill

        Call SplitContainer1.Panel1.Controls.Add(packTree)
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Call Me.Close()
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "HDS Pack File(*.hds)|*.hds|Any Data Pack File(*.*)|*.*"
        }
            If file.ShowDialog = DialogResult.OK Then
                If Not pack Is Nothing Then
                    Call pack.Dispose()
                End If

                pack = StreamPack.OpenReadOnly(file.FileName)
                LoadTree()
            End If
        End Using
    End Sub

    Private Sub LoadTree()

    End Sub
End Class
