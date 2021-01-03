Public Class StartPage

    Private Sub StartPage_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = "Start"
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Using file As New OpenFileDialog With {
           .Filter = "R# Package Project(*.Rproj)|*.Rproj",
           .Title = "Open a R# package project"
        }
            If file.ShowDialog = DialogResult.OK Then
                Call Program.LoadSolution(Rproj:=file.FileName)
            End If
        End Using
    End Sub
End Class