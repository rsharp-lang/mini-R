Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports WeifenLuo.WinFormsUI.Docking

Public Class ViewJSON : Implements Viewer

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Function View(file As String) As DockContent Implements Viewer.View
        Dim json As JsonElement = file.ReadAllText.ParseJson
        Dim root As New TreeNode With {.Text = file.FileName}

        TabText = file.FileName
        TreeView1.Nodes.Add(root)
        loadJson(json, root)
        FilePath = file

        Return Me
    End Function

    Private Sub loadJson(json As JsonElement, tree As TreeNode)
        If TypeOf json Is JsonArray Then
            Dim array As TreeNode = tree.Nodes.Add("[]")

            For Each item In DirectCast(json, JsonArray)
                Call loadJson(item, array)
            Next
        ElseIf TypeOf json Is JsonObject Then
            Dim obj As TreeNode = tree.Nodes.Add("{}")

            For Each item As NamedValue(Of JsonElement) In DirectCast(json, JsonObject)
                loadJson(item.Value, obj.Nodes.Add(item.Name))
            Next
        ElseIf TypeOf json Is JsonValue Then
            tree.Nodes.Add(DirectCast(json, JsonValue).GetStripString)
        End If
    End Sub

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function
End Class