Imports Microsoft.VisualBasic.My
Imports WeifenLuo.WinFormsUI.Docking

Public Class ToolWinSolution

    Private Sub ToolWinSolution_Load(sender As Object, e As EventArgs) Handles Me.Load
        DoubleBuffered = True
        TabText = "Solution Explorer"
        TreeView1.Nodes(Scan0).Nodes.Clear()

        Call ApplyVsTheme()
        Call VisualStudio.vsWindow.Add(Me)
    End Sub

    Public Overrides Sub ApplyVsTheme()
        VisualStudioToolStripExtender1.ApplyVsTheme(ContextMenuStrip1, ToolStrip1)
    End Sub

    Private Sub ScriptToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScriptToolStripMenuItem.Click

    End Sub

    Private Sub OpenSolutionInfo(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Call VisualStudio.OpenSolution()
    End Sub

    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect

    End Sub

    Private Sub TreeView1_DoubleClick(sender As Object, e As EventArgs) Handles TreeView1.DoubleClick
        If TreeView1.SelectedNode Is Nothing Then
            Return
        End If

        If TreeView1.SelectedNode.Text = "DESCRIPTION" Then
            Call OpenSolutionInfo(Nothing, Nothing)
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("png", "jpg", "bitmap", "gif") Then
            Call VisualStudio.AddDocument(New ViewImage, Sub(c) DirectCast(c, ViewImage).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("txt", "log", "1") Then
            Call VisualStudio.AddDocument(New ViewText, Sub(c) DirectCast(c, ViewText).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("csv") Then
            Call VisualStudio.AddDocument(New ViewDataFrame, Sub(c) DirectCast(c, ViewDataFrame).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("json") Then
            Call VisualStudio.AddDocument(New ViewJSON, Sub(c) DirectCast(c, ViewJSON).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("xml", "html", "htm") Then
            Call VisualStudio.AddDocument(New ViewHtml, Sub(c) DirectCast(c, ViewHtml).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("R") Then
            Call VisualStudio.AddDocument(New RsharpDevEditor, Sub(c) DirectCast(c, RsharpDevEditor).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("dll") Then
            Call VisualStudio.AddDocument(New ViewPkgModule, Sub(c) DirectCast(c, ViewPkgModule).View(TreeView1.SelectedNode.Tag))
        End If
    End Sub

    Private Sub TreeView1_AfterCollapse(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterCollapse
        If e.Node.Tag Is Nothing Then
            e.Node.ImageIndex = 2
        End If
    End Sub

    Private Sub TreeView1_AfterExpand(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterExpand
        If e.Node.Tag Is Nothing Then
            e.Node.ImageIndex = 3
        End If
    End Sub
End Class