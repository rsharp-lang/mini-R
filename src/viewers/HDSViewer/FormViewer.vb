Imports Galaxy.Data.JSON
Imports Galaxy.Data.JSON.Models
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem

Public Class FormViewer

    Dim WithEvents packTree As JsonViewer
    Dim pack As StreamPack
    Dim filepath As String

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

                filepath = file.FileName
                pack = StreamPack.OpenReadOnly(file.FileName)
                Text = $"HDS Pack Viewer [{filepath}]"

                LoadTree()
            End If
        End Using
    End Sub

    Private Sub LoadTree()
        Dim tree As New JsonObject With {
            .Id = filepath.FileName,
            .JsonType = JsonType.Object,
            .Value = pack.superBlock
        }

        Call LoadTree(tree, pack.superBlock)
        Call packTree.Render(New JsonObjectTree(tree))
    End Sub

    Private Sub LoadTree(tree As JsonObject, group As StreamGroup)

    End Sub
End Class
