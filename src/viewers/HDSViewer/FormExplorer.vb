Imports Galaxy.Data.JSON
Imports Galaxy.Data.JSON.Models
Imports Galaxy.Workbench
Imports Microsoft.VisualBasic.DataStorage.HDSPack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem

Public Class FormExplorer

    Dim WithEvents packTree As JsonViewer

    Friend viewer As FormViewer

    Private Sub FormExplorer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        packTree = New JsonViewer
        packTree.Dock = DockStyle.Fill

        Call Controls.Add(packTree)
        Call ApplyVsTheme(ToolStrip1, packTree.GetContextMenu)
        Call packTree.BringToFront()
        Call packTree.AddContextMenuItem("View As PlainText", "view_text")
    End Sub

    Public Sub LoadTree()
        Dim tree As New JsonObject With {
            .Id = viewer.filepath.FileName,
            .JsonType = JsonType.Object,
            .Value = viewer.pack.superBlock
        }

        Call LoadTree(tree, viewer.pack.superBlock)
        Call packTree.Render(New JsonObjectTree(tree))
    End Sub

    Private Sub LoadTree(tree As JsonObject, group As StreamGroup)
        For Each dir As StreamGroup In group.dirs
            Dim node As New JsonObject With {
                .Id = dir.fileName,
                .JsonType = JsonType.Object,
                .Value = dir,
                .Parent = tree
            }

            Call tree.Fields.Add(node)
            Call LoadTree(node, dir)
        Next

        For Each file As StreamBlock In group.files.OfType(Of StreamBlock)
            Dim node As New JsonObject With {
                .Id = file.fileName,
                .JsonType = JsonType.Value,
                .Parent = tree,
                .Value = file
            }

            Call tree.Fields.Add(node)
        Next
    End Sub

    Private Sub packTree_ViewAction(node As JsonViewerTreeNode) Handles packTree.ViewAction
        If node.JsonObject.Value Is Nothing OrElse TypeOf node.JsonObject.Value Is StreamGroup Then
            Return
        End If

        Dim file As StreamBlock = DirectCast(node.JsonObject.Value, StreamBlock)
        Dim pack As StreamPack = viewer.pack

        Select Case file.fileName.ExtensionSuffix
            Case "json"
            Case "txt"
                Call CommonRuntime.ShowDocument(Of FormTextViewer)(, file.fileName).ShowTextData(pack.ReadText(file))
            Case "jpg", "png", "jpeg", "bmp", "tiff"
            Case "xml"
            Case "html"
            Case "csv"
            Case "rtf"
            Case Else
                ' view in binary mode

        End Select
    End Sub

    Private Sub packTree_MenuAction(sender As ToolStripMenuItem, node As JsonObject) Handles packTree.MenuAction
        If node.Value Is Nothing OrElse TypeOf node.Value Is StreamGroup Then
            Return
        End If

        Dim file As StreamBlock = DirectCast(node.Value, StreamBlock)
        Dim pack As StreamPack = viewer.pack

        Select Case sender.Name
            Case "view_text" : Call CommonRuntime.ShowDocument(Of FormTextViewer)(, file.fileName).ShowTextData(Pack.ReadText(file))
        End Select
    End Sub

    Private Sub packTree_Visit(node As JsonViewerTreeNode) Handles packTree.Visit
        If node.JsonObject Is Nothing OrElse node.JsonObject.Value Is Nothing Then
            Return
        End If

        Call CommonRuntime.StatusMessage(node.JsonObject.Value.ToString)
    End Sub
End Class