Public Class FormTextViewer

    Public Function ShowTextData(text As String) As FormTextViewer
        Me.TextBox1.Text = text
        Return Me
    End Function

End Class