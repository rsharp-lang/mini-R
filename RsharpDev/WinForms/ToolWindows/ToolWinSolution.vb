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

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Call VisualStudio.AddDocument(SingletonHolder(Of PackageConfiguration).Instance)
        Call SingletonHolder(Of PackageConfiguration).Instance.LoadSolution()
    End Sub

    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        If e.Node.Text = "DESCRIPTION" Then
            Call ToolStripButton1_Click(Nothing, Nothing)
        End If
    End Sub
End Class