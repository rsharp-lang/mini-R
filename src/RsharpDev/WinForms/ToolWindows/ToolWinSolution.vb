Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.My
Imports My
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

        Dim filepath As String = TreeView1.SelectedNode.Tag

        If TreeView1.SelectedNode.Text = "DESCRIPTION" Then
            Call OpenSolutionInfo(Nothing, Nothing)
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("png", "jpg", "bitmap", "gif") Then
            Call VisualStudio.AddDocument(New ViewImage, filepath, Sub(c) DirectCast(c, ViewImage).View(filepath))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("txt", "log", "1") Then
            Call VisualStudio.AddDocument(New ViewText, filepath, Sub(c) DirectCast(c, ViewText).View(filepath))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("csv") Then
            Call VisualStudio.AddDocument(New ViewDataFrame, filepath, Sub(c) DirectCast(c, ViewDataFrame).View(filepath))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("json") Then
            Call VisualStudio.AddDocument(New ViewJSON, filepath, Sub(c) DirectCast(c, ViewJSON).View(filepath))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("xml", "html", "htm") Then
            Call VisualStudio.AddDocument(New ViewHtml, filepath, Sub(c) DirectCast(c, ViewHtml).View(filepath))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("R") Then
            Call VisualStudio.AddDocument(New RsharpDevVscode, filepath, Sub(c) DirectCast(c, RsharpDevVscode).View(filepath))
        ElseIf TreeView1.SelectedNode.Text.ExtensionSuffix("dll") Then
            Call VisualStudio.AddDocument(New ViewPkgModule, filepath, Sub(c) DirectCast(c, ViewPkgModule).View(filepath))
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

    ''' <summary>
    ''' build package
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        If Program.Solution Is Nothing Then
            MyApplication.RStudio.ShowStatusMsg("No solution for build package...", My.Resources.StatusAnnotations_Warning_32xLG)
            Return
        Else
            MyApplication.RStudio.ShowStatusMsg($"Build package '{Program.Solution.LoadInformation.Package}'...", My.Resources.build_Selection_32xLG)
        End If

        Dim Rscript = CLI.Rscript.FromEnvironment(App.HOME)
        Dim commandlineArguments As String = Rscript.GetCompileCommandLine(src:=Program.Solution.ProjectFolder)

        VisualStudio.Output.DockState = DockState.DockBottom

        Call New Thread(
            Sub()
                Call PipelineProcess.ExecSub(
                    app:=Rscript.Path,
                    args:=commandlineArguments,
                    onReadLine:=AddressOf VisualStudio.Output.AppendLine
                )
                Call MyApplication.RStudio.ShowStatusMsg("Ready")
            End Sub).Start()
    End Sub
End Class