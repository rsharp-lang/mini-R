Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class RecentFiles

    Private Sub RecentFiles_Load(sender As Object, e As EventArgs) Handles Me.Load
        VisualStudio.RefreshRecentList = AddressOf LoadList
    End Sub

    Public Sub LoadList()
        For Each file As NamedValue In Program.Config.recentFiles.SafeQuery
            Dim item As New FileItem

            item.PictureBox1.BackgroundImage = My.Resources.R_sharp
            item.Label1.Text = file.name.FileName
            item.LinkLabel1.Text = file.name.ParentPath
            item.Label2.Text = Date.Parse(file.text)
        Next
    End Sub
End Class
