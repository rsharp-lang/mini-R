Imports Galaxy.Data.JSON

Public Class FormViewer

    Dim WithEvents packTree As JsonViewer

    Private Sub FormViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        packTree = New JsonViewer
        packTree.Dock = DockStyle.Fill

        Call SplitContainer1.Panel1.Controls.Add(packTree)
    End Sub
End Class
