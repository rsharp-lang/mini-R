Imports Config
Imports WeifenLuo.WinFormsUI.Docking

Public Class ToolOutput : Implements IApplyVsTheme

    Private Sub ToolOutput_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = "Output"

        Call ApplyVsTheme()
        Call VisualStudio.vsWindow.Add(Me)
    End Sub

    Public Overrides Sub ApplyVsTheme() Implements IApplyVsTheme.ApplyVsTheme
        VisualStudioToolStripExtender1.ApplyVsTheme(ToolStrip1)
    End Sub
End Class