Imports System.IO
Imports System.Text

Public Interface Editor

    Sub SetContent(scriptText As String, filename As String)
    Sub Save(file As Stream, encoding As Encoding)

End Interface
