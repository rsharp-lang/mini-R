Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ApplicationServices.Development

Public Class SplashScreen

    Private Sub SplashScreen_Load(sender As Object, e As EventArgs) Handles Me.Load
        Label4.Text = Label4.Text _
            .Replace("%user", My.User.Name) _
            .Replace("%year", Now.Year) _
            .Replace("%copyright", GetType(RsharpDevAbout).Assembly.FromAssembly.AssemblyCompany)
    End Sub

    Private Sub SplashScreen_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim w As Single = 3

        e.Graphics.DrawRectangle(New Pen(Brushes.Gray, w) With {.DashStyle = DashStyle.Dot}, New Rectangle(New Point, New Size(Width - w, Height - w)))
    End Sub
End Class