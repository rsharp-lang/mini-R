Imports System.IO
Imports System.Collections.Generic
Imports System.Text.Json
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Public Class VsCodeEditor : Implements IFileReference

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Return {}
        End Get
    End Property

    Shared ReadOnly cdn As ZipStream

    Shared Sub New()
        Dim asm As System.Reflection.Assembly = GetType(VsCodeEditor).Assembly
        ' VB embedded resource manifest name = <RootNamespace>.<path-with-dots>
        Dim resName As String = asm.GetName().Name & ".monaco-editor.zip"
        Dim res As Stream = asm.GetManifestResourceStream(resName)

        If res Is Nothing Then
            ' fallback: locate by suffix in case the manifest name differs
            For Each name As String In asm.GetManifestResourceNames()
                If name.EndsWith("RDev.monaco-editor.zip", StringComparison.OrdinalIgnoreCase) Then
                    resName = name
                    res = asm.GetManifestResourceStream(name)
                    Exit For
                End If
            Next
        End If

        If res Is Nothing Then
            Throw New System.Resources.MissingManifestResourceException("Embedded resource 'vscode\monaco-editor.zip' was not found in assembly " & asm.FullName & ".")
        End If

        ' copy into a standalone memory stream so the manifest resource stream
        ' can be released; ZipStream keeps a reference to the stream it is given
        ' and reads from it lazily, so it must remain alive.
        Dim buffer As New MemoryStream
        res.CopyTo(buffer)
        res.Dispose()
        buffer.Seek(0, SeekOrigin.Begin)

        cdn = New ZipStream(buffer, is_readonly:=True)
    End Sub

    Private Async Sub VsCodeEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)
    End Sub


End Class
