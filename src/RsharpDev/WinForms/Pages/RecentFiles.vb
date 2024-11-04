Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class RecentFiles

    Private Sub RecentFiles_Load(sender As Object, e As EventArgs) Handles Me.Load
        VisualStudio.RefreshRecentList = AddressOf LoadList
    End Sub

    Public Sub LoadList()
        Call FlowLayoutPanel1.Controls.Clear()

        For Each file As NamedValue In Program.Config.recentFiles.SafeQuery
            Dim item As New FileItem

            If file.name.ExtensionSuffix("Rproj") Then
                item.PictureBox1.BackgroundImage = My.Resources.Rproj
            Else
                item.PictureBox1.BackgroundImage = My.Resources.R_sharp
            End If

            item.Label1.Text = file.name.FileName
            item.LinkLabel1.Text = file.name.ParentPath

            With Date.Parse(file.text)
                item.Label2.Text = $"{ .Year}/{ .Month}/{ .Day}"
            End With

            FlowLayoutPanel1.Controls.Add(item)
        Next
    End Sub

    Private Sub FlowLayoutPanel1_Resize(sender As Object, e As EventArgs) Handles FlowLayoutPanel1.Resize
        Dim w = FlowLayoutPanel1.Width * 0.9

        For Each item As Control In FlowLayoutPanel1.Controls
            item.Width = w
        Next
    End Sub
End Class
