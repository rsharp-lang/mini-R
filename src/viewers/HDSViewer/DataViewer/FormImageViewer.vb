Imports System.IO

Public Class FormImageViewer

    Public Function ShowAsImage(s As Stream) As FormImageViewer
        PictureBox1.BackgroundImage = System.Drawing.Image.FromStream(s)
        Return Me
    End Function

End Class