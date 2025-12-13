Imports System.IO
Imports Galaxy.Data

Public Class FormXMLViewer

    Public Function RenderXml(xml As Stream) As FormXMLViewer
        Call New XmlSyntaxRender(RichTextBox1, xml).HighlightXML()
        Return Me
    End Function
End Class