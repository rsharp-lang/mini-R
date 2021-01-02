Imports System.Windows.Forms
Imports WeifenLuo.WinFormsUI.Docking


Partial Public Class ToolWindow
    Inherits DockContent

    Public Sub New()
        InitializeComponent()

        AutoScaleMode = AutoScaleMode.Dpi
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

