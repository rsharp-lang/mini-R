Public Class Form1
    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {.Filter = "any file(*.*)|*.*"}
            If file.ShowDialog = DialogResult.OK Then
                Dim samples As sampledata()
            End If
        End Using
    End Sub
End Class
