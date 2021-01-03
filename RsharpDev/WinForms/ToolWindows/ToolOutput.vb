Imports WeifenLuo.WinFormsUI.Docking

Public Class ToolOutput

    Private Sub ToolOutput_Load(sender As Object, e As EventArgs) Handles Me.Load
        VisualStudioToolStripExtender1.ApplyVsTheme(VisualStudioToolStripExtender.VsVersion.Vs2015, VS2015LightTheme1, ToolStrip1)
    End Sub
End Class