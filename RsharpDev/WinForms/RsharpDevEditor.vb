Imports System.Text
Imports FastColoredTextBoxNS
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports RDev
Imports WeifenLuo.WinFormsUI.Docking

Public Class RsharpDevEditor : Inherits DockContent
    Implements ISaveHandle
    Implements IFileReference
    Implements Viewer

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Return Editor1.MimeType
        End Get
    End Property

    Private Sub RsharpDevEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.ShowIcon = True
    End Sub

    Public Function View(script As String) As DockContent Implements Viewer.View
        If script.FileExists Then
            FilePath = script
            Editor1.LoadScript(script)
            TabText = FilePath.FileName
        End If

        Return Me
    End Function

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Return Editor1.Save(path, encoding)
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Return Save(path, encoding.CodePage)
    End Function
End Class