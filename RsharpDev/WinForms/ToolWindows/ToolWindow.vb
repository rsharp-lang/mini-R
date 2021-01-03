Imports System.Windows.Forms
Imports Config
Imports WeifenLuo.WinFormsUI.Docking


Partial Public Class ToolWindow
    Inherits DockContent
    Implements IApplyVsTheme

    Protected ReadOnly _toolStripProfessionalRenderer As New ToolStripProfessionalRenderer()

    Public Sub New()
        InitializeComponent()

        AutoScaleMode = AutoScaleMode.Dpi
        VisualStudioToolStripExtender1.DefaultRenderer = _toolStripProfessionalRenderer
    End Sub

    Public Overridable Sub ApplyVsTheme() Implements IApplyVsTheme.ApplyVsTheme
        Throw New NotImplementedException
    End Sub

    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        DockState = DockState.Hidden
    End Sub

    Private Sub FloatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FloatToolStripMenuItem.Click
        DockState = DockState.Float
    End Sub

    Private Sub HideToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HideToolStripMenuItem.Click
        DockState = DockState.Hidden
    End Sub
End Class

