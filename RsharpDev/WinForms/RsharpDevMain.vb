Imports Config
Imports Microsoft.VisualBasic.My
Imports My
Imports RibbonLib.Interop
Imports WeifenLuo.WinFormsUI.Docking

Partial Public Class RsharpDevMain : Inherits Form
    Implements IApplyVsTheme

    Friend ReadOnly ribbon As RibbonItems

    Public Sub New()
        InitializeComponent()

        ribbon = New RibbonItems(_ribbon)

        AddHandler ribbon.ButtonNew.ExecuteEvent, Sub() Call VisualStudio.AddDocument(New RsharpDevVscode)
        AddHandler ribbon.License.ExecuteEvent, Sub() Call New RsharpDevAbout().ShowDialog()
        AddHandler ribbon.About.ExecuteEvent, Sub() Call showAboutSplash()
        AddHandler ribbon.ButtonOpen.ExecuteEvent, Sub() Call VisualStudio.OpenFile()
        AddHandler ribbon.ConfigServer.ExecuteEvent, Sub() VisualStudio.LinuxServerList.DockState = DockState.DockLeft
        AddHandler ribbon.Console.ExecuteEvent, Sub() VisualStudio.AddDocument(SingletonHolder(Of RsharpDevConsole).Instance)
        AddHandler ribbon.ViewProperty.ExecuteEvent, Sub() Call VisualStudio.OpenSolution()
        AddHandler ribbon.Config.ExecuteEvent, Sub() Call New ConfigApp().ShowDialog()
        AddHandler ribbon.StartPage.ExecuteEvent, Sub() Call VisualStudio.AddDocument(SingletonHolder(Of StartPage).Instance)
        AddHandler ribbon.Close.ExecuteEvent, Sub() Call Me.Close()

        ribbon.SoluationTabGroup.ContextAvailable = ContextAvailability.NotAvailable
        ribbon.SoluationTabGroup.Label = "Solution [RsharpDev]"
        ribbon.SolutionTab.Label = "Solution [RsharpDev]"

        MyApplication.Register(Me)
    End Sub

    Private Sub showAboutSplash()
        Call New SplashScreen() With {.AutoClose = True}.ShowDialog()
    End Sub

    Private Sub RsharpDevMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        AutoScaleMode = AutoScaleMode.Dpi

        Call Program.Initialize()
        Call InitializeVsUI()
        Call VisualStudio.InitializeUI()
        Call VisualStudio.vsWindow.Add(Me)
        Call VisualStudio.AddDocument(SingletonHolder(Of StartPage).Instance)
    End Sub

    Public Sub ShowStatusMsg(message As String, Optional icon As Image = Nothing)
        If icon Is Nothing Then
            icon = My.Resources.preferences_system_notifications
        End If

        Call Me.Invoke(
            Sub()
                ToolStripStatusLabel1.Image = icon
                ToolStripStatusLabel1.Text = message
            End Sub)
    End Sub

    ReadOnly _toolStripProfessionalRenderer As New ToolStripProfessionalRenderer()

    Private Sub InitializeVsUI()
        VisualStudioToolStripExtender1.DefaultRenderer = _toolStripProfessionalRenderer

        DockPanel1.Theme = VS2015LightTheme1
        DockPanel1.ShowDocumentIcon = True

        EnableVSRenderer()
    End Sub

    Private Sub EnableVSRenderer() Implements IApplyVsTheme.ApplyVsTheme
        ' vsToolStripExtender1.SetStyle(mainMenu, version, theme)
        ' vsToolStripExtender1.SetStyle(toolBar, version, theme)
        VisualStudioToolStripExtender1.ApplyVsTheme(StatusStrip1)
    End Sub
End Class
