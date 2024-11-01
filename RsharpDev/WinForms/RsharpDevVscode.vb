Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports WeifenLuo.WinFormsUI.Docking

Public Class RsharpDevVscode
    Implements ISaveHandle
    Implements IFileReference
    Implements Viewer

    Public Property FilePath As String Implements IFileReference.FilePath
    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType

    Public Function View(file As String) As DockContent Implements Viewer.View

    End Function

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save

    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save

    End Function

    Public Function Save(file As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save

    End Function
End Class