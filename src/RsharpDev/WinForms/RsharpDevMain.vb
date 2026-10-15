Imports System.ComponentModel
Imports Galaxy.Workbench
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.My
Imports RsharpDev.My
Imports RsharpDev.My.Resources

Public Class RsharpDevMain : Implements AppHost

    ReadOnly _toolStripProfessionalRenderer As New ToolStripProfessionalRenderer()


    Public Event ResizeForm As AppHost.ResizeFormEventHandler Implements AppHost.ResizeForm
    Public Event CloseWorkbench As AppHost.CloseWorkbenchEventHandler Implements AppHost.CloseWorkbench

    Private ReadOnly Property AppHost_ClientRectangle As Rectangle Implements AppHost.ClientRectangle
        Get
            Return New Rectangle(Location, Size)
        End Get
    End Property

    Public ReadOnly Property ActiveDocument As Form Implements AppHost.ActiveDocument
        Get
            Return DockPanel1.ActiveDocument
        End Get
    End Property

    Private Async Sub RsharpDevMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        AutoScaleMode = AutoScaleMode.Dpi

        Call ImageDriver.Register()
        Call RibbonMenu.Hook(RibbonUI)
        Call CommonRuntime.Hook(Me)
        Call MyApplication.Register(Me)
        Call Program.Initialize()
        Call InitializeVsUI()
        Call VisualStudio.InitializeUI()
        Call VisualStudio.AddDocument(SingletonHolder(Of StartPage).Instance)

        Await MyApplication.LaunchLanguageServer
    End Sub

    Public Sub ShowStatusMsg(message As String, Optional icon As Image = Nothing) Implements AppHost.StatusMessage
        Call Me.Invoke(
            Sub()
                ToolStripStatusLabel1.Image = If(icon, IconResource.preferences_system_notifications)
                ToolStripStatusLabel1.Text = message
            End Sub)
    End Sub

    Private Sub InitializeVsUI()
        VisualStudioToolStripExtender1.DefaultRenderer = _toolStripProfessionalRenderer

        DockPanel1.Theme = VS2015LightTheme1
        DockPanel1.ShowDocumentIcon = True

        EnableVSRenderer()
    End Sub

    Private Sub EnableVSRenderer()
        ' vsToolStripExtender1.SetStyle(mainMenu, version, theme)
        ' vsToolStripExtender1.SetStyle(toolBar, version, theme)
        VisualStudioToolStripExtender1.ApplyVsTheme(StatusStrip1)
    End Sub

    Private Sub RsharpDevMain_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Call Program.Save()
        Call CommonRuntime.SaveUISettings()
    End Sub

    Public Sub SetWorkbenchVisible(visible As Boolean) Implements AppHost.SetWorkbenchVisible
        Me.Visible = visible
    End Sub

    Public Sub SetWindowState(stat As FormWindowState) Implements AppHost.SetWindowState
        Me.WindowState = stat
    End Sub

    Public Function GetDesktopLocation() As Point Implements AppHost.GetDesktopLocation
        Return Location
    End Function

    Public Function GetClientSize() As Size Implements AppHost.GetClientSize
        Return Size
    End Function

    Public Function GetDocuments() As IEnumerable(Of Form) Implements AppHost.GetDocuments
        Return DockPanel1.Documents.OfType(Of Form)
    End Function

    Public Function GetDockPanel() As Control Implements AppHost.GetDockPanel
        Return DockPanel1
    End Function

    Public Function GetWindowState() As FormWindowState Implements AppHost.GetWindowState
        Return WindowState
    End Function

    Public Sub SetTitle(title As String) Implements AppHost.SetTitle
        Text = title
    End Sub

    Public Sub Warning(msg As String) Implements AppHost.Warning
        Call StatusMessage(msg, Icons8.Warning)
    End Sub

    Public Sub LogText(text As String) Implements AppHost.LogText
        Call CommonRuntime.GetOutputWindow.AppendLine(text)
    End Sub

    Public Sub ShowProperties(obj As Object) Implements AppHost.ShowProperties
        Call CommonRuntime.GetPropertyWindow.SetObject(obj)
    End Sub
End Class
