
Partial Public Class RsharpDevMain
    ''' <summary>
    ''' Required designer variable.
    ''' </summary>
    Private components As System.ComponentModel.IContainer = Nothing

    ''' <summary>
    ''' Clean up any resources being used.
    ''' </summary>
    ''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso (components IsNot Nothing) Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

#Region "Windows Form Designer generated code"

    ''' <summary>
    ''' Required method for Designer support - do not modify
    ''' the contents of this method with the code editor.
    ''' </summary>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RsharpDevMain))
        Me._ribbon = New RibbonLib.Ribbon()
        Me.VS2015LightTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2015LightTheme()
        Me.VS2015DarkTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme()
        Me.VS2015BlueTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme()
        Me.VS2013LightTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2013LightTheme()
        Me.VS2013DarkTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2013DarkTheme()
        Me.VS2013BlueTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2013BlueTheme()
        Me.VS2012LightTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2012LightTheme()
        Me.VS2012DarkTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2012DarkTheme()
        Me.VS2012BlueTheme1 = New WeifenLuo.WinFormsUI.Docking.VS2012BlueTheme()
        Me.VS2005Theme1 = New WeifenLuo.WinFormsUI.Docking.VS2005Theme()
        Me.VS2003Theme1 = New WeifenLuo.WinFormsUI.Docking.VS2003Theme()
        Me.VisualStudioToolStripExtender1 = New WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(Me.components)
        Me.DockPanel1 = New WeifenLuo.WinFormsUI.Docking.DockPanel()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.SuspendLayout()
        '
        '_ribbon
        '
        Me._ribbon.Location = New System.Drawing.Point(0, 0)
        Me._ribbon.Name = "_ribbon"
        Me._ribbon.ResourceIdentifier = Nothing
        Me._ribbon.ResourceName = "RibbonMarkup.ribbon"
        Me._ribbon.ShortcutTableResourceName = Nothing
        Me._ribbon.Size = New System.Drawing.Size(889, 100)
        Me._ribbon.TabIndex = 5
        '
        'VisualStudioToolStripExtender1
        '
        Me.VisualStudioToolStripExtender1.DefaultRenderer = Nothing
        '
        'DockPanel1
        '
        Me.DockPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DockPanel1.Location = New System.Drawing.Point(0, 100)
        Me.DockPanel1.Name = "DockPanel1"
        Me.DockPanel1.Size = New System.Drawing.Size(889, 434)
        Me.DockPanel1.TabIndex = 6
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 534)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(889, 22)
        Me.StatusStrip1.TabIndex = 7
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(61, 4)
        '
        'RsharpDevMain
        '
        Me.ClientSize = New System.Drawing.Size(889, 556)
        Me.Controls.Add(Me.DockPanel1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me._ribbon)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "RsharpDevMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "R# Develop Studio"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private _ribbon As RibbonLib.Ribbon
    Friend WithEvents VS2015LightTheme1 As WeifenLuo.WinFormsUI.Docking.VS2015LightTheme
    Friend WithEvents VS2015DarkTheme1 As WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme
    Friend WithEvents VS2015BlueTheme1 As WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme
    Friend WithEvents VS2013LightTheme1 As WeifenLuo.WinFormsUI.Docking.VS2013LightTheme
    Friend WithEvents VS2013DarkTheme1 As WeifenLuo.WinFormsUI.Docking.VS2013DarkTheme
    Friend WithEvents VS2013BlueTheme1 As WeifenLuo.WinFormsUI.Docking.VS2013BlueTheme
    Friend WithEvents VS2012LightTheme1 As WeifenLuo.WinFormsUI.Docking.VS2012LightTheme
    Friend WithEvents VS2012DarkTheme1 As WeifenLuo.WinFormsUI.Docking.VS2012DarkTheme
    Friend WithEvents VS2012BlueTheme1 As WeifenLuo.WinFormsUI.Docking.VS2012BlueTheme
    Friend WithEvents VS2005Theme1 As WeifenLuo.WinFormsUI.Docking.VS2005Theme
    Friend WithEvents VS2003Theme1 As WeifenLuo.WinFormsUI.Docking.VS2003Theme
    Friend WithEvents VisualStudioToolStripExtender1 As WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender
    Friend WithEvents DockPanel1 As WeifenLuo.WinFormsUI.Docking.DockPanel
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
End Class

