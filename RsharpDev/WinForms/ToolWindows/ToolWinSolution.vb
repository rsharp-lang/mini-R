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
End Class