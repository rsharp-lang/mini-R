Public Class FileItem

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click, Label1.Click, LinkLabel1.LinkClicked, Label2.Click, Me.Click
        Call VisualStudio.OpenFile($"{LinkLabel1.Text}/{Label1.Text}")
    End Sub
End Class
