Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging

Public Class ViewImage

    Public Function View(file As String) As ViewImage
        PictureBox1.BackgroundImage = file.LoadImage.CTypeGdiImage
        TabText = file.FileName

        Return Me
    End Function
End Class