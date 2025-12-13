Imports Galaxy.Data.JSON

Public Class FormJSONViewer

    Dim WithEvents viewer As JsonViewer

    Private Sub FormJSONViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        viewer = New JsonViewer
        viewer.Dock = DockStyle.Fill

        Call Controls.Add(viewer)
        Call ApplyVsTheme(viewer.GetContextMenu)
    End Sub

    Public Function ShowJSON(tag As String, json_str As String) As FormJSONViewer
        viewer.RootTag = tag
        viewer.Json = json_str
        Return Me
    End Function
End Class