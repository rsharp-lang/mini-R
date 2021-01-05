Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Package.File

Public Class Solution
    Implements ISaveHandle
    Implements IFileReference

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public ReadOnly Property ProjectFolder As String
        Get
            Return FilePath.ParentPath
        End Get
    End Property

    Public Function LoadInformation() As DESCRIPTION
        Return DESCRIPTION.Parse($"{FilePath.ParentPath}/DESCRIPTION")
    End Function

    Public Shared Function LoadRproj(file As String) As Solution
        Return New Solution With {.FilePath = file}
    End Function

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function
End Class
