Imports Galaxy.Workbench
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualStudio.WinForms.Docking

Public Class FormViewer : Implements AppHost

    Friend pack As StreamPack
    Friend filepath As String
    Friend explorer As FormExplorer

    Public ReadOnly Property ActiveDocument As Form Implements AppHost.ActiveDocument
        Get
            Return DockPanel1.ActiveDocument
        End Get
    End Property

    Private ReadOnly Property AppHost_ClientRectangle As Rectangle Implements AppHost.ClientRectangle
        Get
            Return New Rectangle(Location, Size)
        End Get
    End Property

    Public Event ResizeForm As AppHost.ResizeFormEventHandler Implements AppHost.ResizeForm
    Public Event CloseWorkbench As AppHost.CloseWorkbenchEventHandler Implements AppHost.CloseWorkbench

    Public Sub SetWorkbenchVisible(visible As Boolean) Implements AppHost.SetWorkbenchVisible
        Me.Visible = visible
    End Sub

    Public Sub SetWindowState(stat As FormWindowState) Implements AppHost.SetWindowState
        Me.WindowState = stat
    End Sub

    Public Sub SetTitle(title As String) Implements AppHost.SetTitle
        Me.Text = title
    End Sub

    Public Sub StatusMessage(msg As String, Optional icon As Image = Nothing) Implements AppHost.StatusMessage

    End Sub

    Public Sub Warning(msg As String) Implements AppHost.Warning

    End Sub

    Public Sub LogText(text As String) Implements AppHost.LogText

    End Sub

    Public Sub ShowProperties(obj As Object) Implements AppHost.ShowProperties
        Call CommonRuntime.GetPropertyWindow.SetObject(obj)
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Call Me.Close()
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "HDS Pack File(*.hds)|*.hds|Any Data Pack File(*.*)|*.*"
        }
            If file.ShowDialog = DialogResult.OK Then
                If Not pack Is Nothing Then
                    Call pack.Dispose()
                End If

                filepath = file.FileName
                pack = StreamPack.OpenReadOnly(file.FileName)
                Text = $"HDS Pack Viewer [{filepath}]"
                explorer.LoadTree()
                explorer.DockState = DockState.DockLeft
            End If
        End Using
    End Sub

    Private Sub FormViewer_Load(sender As Object, e As EventArgs) Handles Me.Load
        explorer = New FormExplorer
        explorer.viewer = Me
        explorer.Show(DockPanel1)
        explorer.DockState = DockState.DockLeftAutoHide

        Call CommonRuntime.Hook(Me)
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
End Class
