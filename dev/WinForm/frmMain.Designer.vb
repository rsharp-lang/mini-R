<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.Ribbon1 = New RibbonLib.Ribbon()
        Me.DockPanel1 = New WeifenLuo.WinFormsUI.Docking.DockPanel()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.vsToolStripExtender1 = New WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(Me.components)
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
        Me.SuspendLayout()
        '
        'Ribbon1
        '
        Me.Ribbon1.Location = New System.Drawing.Point(0, 0)
        Me.Ribbon1.Name = "Ribbon1"
        Me.Ribbon1.ResourceIdentifier = "RibbonMarkup.ribbon"
        Me.Ribbon1.ResourceName = Nothing
        Me.Ribbon1.ShortcutTableResourceName = Nothing
        Me.Ribbon1.Size = New System.Drawing.Size(1174, 109)
        Me.Ribbon1.TabIndex = 0
        '
        'DockPanel1
        '
        Me.DockPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DockPanel1.Location = New System.Drawing.Point(0, 109)
        Me.DockPanel1.Name = "DockPanel1"
        Me.DockPanel1.Size = New System.Drawing.Size(1174, 539)
        Me.DockPanel1.TabIndex = 1
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 648)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1174, 22)
        Me.StatusStrip1.TabIndex = 2
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'vsToolStripExtender1
        '
        Me.vsToolStripExtender1.DefaultRenderer = Nothing
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1174, 670)
        Me.Controls.Add(Me.DockPanel1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Ribbon1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmMain"
        Me.Text = "R# Studio"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Ribbon1 As RibbonLib.Ribbon
    Friend WithEvents DockPanel1 As WeifenLuo.WinFormsUI.Docking.DockPanel
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents vsToolStripExtender1 As WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender
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
End Class
