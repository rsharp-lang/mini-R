Imports Microsoft.VisualBasic.ApplicationServices.Development

Public Class RsharpDevAbout

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub RsharpDevAbout_Load(sender As Object, e As EventArgs) Handles Me.Load
        Label4.Text = Label4.Text _
            .Replace("%user", My.User.Name) _
            .Replace("%year", Now.Year) _
            .Replace("%copyright", GetType(RsharpDevAbout).Assembly.FromAssembly.AssemblyCompany)
    End Sub
End Class