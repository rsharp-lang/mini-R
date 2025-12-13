Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualStudio.WinForms.Docking

Public Class FormViewer

    Friend pack As StreamPack
    Friend filepath As String
    Friend explorer As FormExplorer

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
                explorer.LoadTree()
                explorer.DockState = DockState.DockLeft
            End If
        End Using
    End Sub

    Private Sub DockPanel1_ActiveContentChanged(sender As Object, e As EventArgs) Handles DockPanel1.ActiveContentChanged

    End Sub

    Private Sub FormViewer_Load(sender As Object, e As EventArgs) Handles Me.Load
        explorer = New FormExplorer
        explorer.viewer = Me
        explorer.Show(DockPanel1)
        explorer.DockState = DockState.DockLeftAutoHide
    End Sub
End Class
