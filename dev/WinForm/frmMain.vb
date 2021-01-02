Imports WeifenLuo.WinFormsUI.Docking

Public Class frmMain

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        AutoScaleMode = AutoScaleMode.Dpi

        Call InitializeVsUI()
    End Sub

    ReadOnly _toolStripProfessionalRenderer As New ToolStripProfessionalRenderer()

    Private Sub InitializeVsUI()
        vsToolStripExtender1.DefaultRenderer = _toolStripProfessionalRenderer
        DockPanel1.Theme = VS2015LightTheme1
        EnableVSRenderer(VisualStudioToolStripExtender.VsVersion.Vs2015, VS2015LightTheme1)
    End Sub

    Private Sub EnableVSRenderer(version As VisualStudioToolStripExtender.VsVersion, theme As ThemeBase)
        ' vsToolStripExtender1.SetStyle(mainMenu, version, theme)
        ' vsToolStripExtender1.SetStyle(toolBar, version, theme)
        vsToolStripExtender1.SetStyle(StatusStrip1, version, theme)
    End Sub
End Class
