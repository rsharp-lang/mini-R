Imports System.Drawing
Imports FastColoredTextBoxNS
Imports Microsoft.VisualBasic.Imaging

Public Class ColorStyle : Inherits Style

    Friend Shared ReadOnly htmlcolor As String = "[#][0-9a-fA-F]{6}"

    Public Overrides Sub Draw(gr As Graphics, position As Point, range As Range)
        If range.Text.IsPattern(htmlcolor) Then
            ' get size of rectangle
            Dim size As Size = GetSizeOfRange(range)
            'create rectangle
            Dim rect As New Rectangle(position, size)
            ' inflate it
            ' rect.Inflate(2, 2);
            ' get rounded rectangle
            Dim path = GetRoundedRectangle(rect, 7)
            Dim color As Brush = range.Text.GetBrush

            gr.FillPath(color, path)
            ' draw rounded rectangle
            gr.DrawPath(Pens.LightGray, path)
        End If
    End Sub
End Class
