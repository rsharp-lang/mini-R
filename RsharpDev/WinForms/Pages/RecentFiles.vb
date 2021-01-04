Imports Microsoft.VisualBasic.Linq

Public Class RecentFiles

    Private Sub RecentFiles_Load(sender As Object, e As EventArgs) Handles Me.Load
        VisualStudio.RefreshRecentList = AddressOf LoadList
    End Sub

    Public Sub LoadList()
        For Each file As String In Program.Config.recentFiles.SafeQuery

        Next
    End Sub
End Class
