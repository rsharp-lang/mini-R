Public Class FormRtfViewer

    Public Function ShowRtf(rtf As String) As FormRtfViewer
        RichTextBox1.Rtf = rtf
        Return Me
    End Function

End Class