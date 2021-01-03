Imports Microsoft.VisualBasic.My
Imports WeifenLuo.WinFormsUI.Docking

Public Class ToolWinSolution

    ReadOnly _toolStripProfessionalRenderer As New ToolStripProfessionalRenderer()

    Private Sub ToolWinSolution_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = "Solution Explorer"
        VisualStudioToolStripExtender1.DefaultRenderer = _toolStripProfessionalRenderer


        VisualStudioToolStripExtender1.ApplyVsTheme(VisualStudioToolStripExtender.VsVersion.Vs2015, VS2015LightTheme1, ContextMenuStrip1, ToolStrip1)
    End Sub

    Private Sub ScriptToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScriptToolStripMenuItem.Click

    End Sub

    Private Sub OpenSolutionInfo(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        If Not Program.Solution Is Nothing Then
            Call VisualStudio.AddDocument(SingletonHolder(Of PackageConfiguration).Instance)
            Call SingletonHolder(Of PackageConfiguration).Instance.LoadSolution()
        End If
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
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("xml", "json", "txt") Then
            Call VisualStudio.AddDocument(New ViewText, Sub(c) DirectCast(c, ViewText).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("html", "htm") Then
            Call VisualStudio.AddDocument(New ViewHtml, Sub(c) DirectCast(c, ViewHtml).View(TreeView1.SelectedNode.Tag))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("R") Then
            Call VisualStudio.AddDocument(New RsharpDevEditor, Sub(c) DirectCast(c, RsharpDevEditor).View(TreeView1.SelectedNode.Tag))
        End If
    End Sub
End Class