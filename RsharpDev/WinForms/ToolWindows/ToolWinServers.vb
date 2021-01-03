Public Class ToolWinServers

    Private Sub ToolWinServers_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = "Linux Server Resources"
    End Sub

    Private Sub ToolStripButtonAddServer_Click(sender As Object, e As EventArgs) Handles ToolStripButtonAddServer.Click
        Call New AddLinuxServer().ShowDialog()
    End Sub
End Class