Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ApplicationServices.Development

Public Class SplashScreen

    Public Property AutoClose As Boolean = False

    Private Sub SplashScreen_Load(sender As Object, e As EventArgs) Handles Me.Load
        Label4.Text = Label4.Text _
            .Replace("%user", My.User.Name) _
            .Replace("%year", Now.Year) _
            .Replace("%copyright", GetType(RsharpDevAbout).Assembly.FromAssembly.AssemblyCompany)
    End Sub

    Private Sub SplashScreen_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim w As Single = 2

        e.Graphics.DrawRectangle(New Pen(Brushes.Gray, w) With {.DashStyle = DashStyle.Dot}, New Rectangle(New Point, New Size(Width - w / 2, Height - w / 2)))
    End Sub

    Private Sub SplashScreen_Deactivate(sender As Object, e As EventArgs) Handles Me.Deactivate
        If AutoClose Then
            Call Close()
        End If
    End Sub
End Class