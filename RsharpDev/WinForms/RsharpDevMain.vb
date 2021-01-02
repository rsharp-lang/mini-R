Imports My
Imports RibbonLib.Interop
Imports WeifenLuo.WinFormsUI.Docking

Partial Public Class RsharpDevMain
    Inherits Form

    ReadOnly ribbon As RibbonItems

    Public Sub New()
        InitializeComponent()

        ribbon = New RibbonItems(_ribbon)

        AddHandler ribbon.ButtonNew.ExecuteEvent, Sub() Call VisualStudio.AddDocument(New RsharpDevEditor)
        AddHandler ribbon.About.ExecuteEvent, Sub() Call New RsharpDevAbout().ShowDialog()
        AddHandler ribbon.ButtonOpen.ExecuteEvent, Sub() Call VisualStudio.OpenScript()

        ribbon.SoluationTabGroup.ContextAvailable = ContextAvailability.Active
        ribbon.SoluationTabGroup.Label = "Solution [RsharpDev]"

        MyApplication.Register(Me)
    End Sub

    Private Sub RsharpDevMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        AutoScaleMode = AutoScaleMode.Dpi

        Call InitializeVsUI()
        Call VisualStudio.InitializeUI()
        Call Program.Initialize()

        ' test
        Call VisualStudio.AddDocument(New RsharpDevEditor)
    End Sub

    ReadOnly _toolStripProfessionalRenderer As New ToolStripProfessionalRenderer()

    Private Sub InitializeVsUI()
        VisualStudioToolStripExtender1.DefaultRenderer = _toolStripProfessionalRenderer

        DockPanel1.Theme = VS2015LightTheme1
        DockPanel1.ShowDocumentIcon = True

        EnableVSRenderer(VisualStudioToolStripExtender.VsVersion.Vs2015, VS2015LightTheme1)
    End Sub

    Private Sub EnableVSRenderer(version As VisualStudioToolStripExtender.VsVersion, theme As ThemeBase)
        ' vsToolStripExtender1.SetStyle(mainMenu, version, theme)
        ' vsToolStripExtender1.SetStyle(toolBar, version, theme)
        VisualStudioToolStripExtender1.SetStyle(StatusStrip1, version, theme)
    End Sub
End Class
