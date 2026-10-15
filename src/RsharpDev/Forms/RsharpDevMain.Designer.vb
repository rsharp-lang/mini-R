
Imports Microsoft.VisualStudio.WinForms.Docking
Imports RibbonLib
Imports RsharpDev.My.Resources
Imports ThemeVS2015

Partial Public Class RsharpDevMain : Inherits Form
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

    Sub New()
        Call InitializeComponent()
    End Sub

#Region "Windows Form Designer generated code"

    ''' <summary>
    ''' Required method for Designer support - do not modify
    ''' the contents of this method with the code editor.
    ''' </summary>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RsharpDevMain))
        RibbonUI = New Ribbon()
        VS2015LightTheme1 = New VS2015LightTheme()
        VS2015DarkTheme1 = New VS2015DarkTheme()
        VS2015BlueTheme1 = New VS2015BlueTheme()
        VisualStudioToolStripExtender1 = New VisualStudioToolStripExtender(components)
        DockPanel1 = New DockPanel()
        StatusStrip1 = New StatusStrip()
        ToolStripStatusLabel1 = New ToolStripStatusLabel()
        ToolStripStatusLabel2 = New ToolStripStatusLabel()
        ToolStripStatusLabel3 = New ToolStripStatusLabel()
        ContextMenuStrip1 = New ContextMenuStrip(components)
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' RibbonUI
        ' 
        RibbonUI.Location = New Point(0, 0)
        RibbonUI.Name = "RibbonUI"
        RibbonUI.ResourceIdentifier = Nothing
        RibbonUI.ResourceName = "RsharpDev.RibbonMarkup.ribbon"
        RibbonUI.ShortcutTableResourceName = Nothing
        RibbonUI.Size = New Size(1035, 116)
        RibbonUI.TabIndex = 5
        ' 
        ' VisualStudioToolStripExtender1
        ' 
        VisualStudioToolStripExtender1.DefaultRenderer = Nothing
        ' 
        ' DockPanel1
        ' 
        DockPanel1.Dock = DockStyle.Fill
        DockPanel1.Location = New Point(0, 116)
        DockPanel1.Name = "DockPanel1"
        DockPanel1.Size = New Size(1035, 513)
        DockPanel1.TabIndex = 6
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.Items.AddRange(New ToolStripItem() {ToolStripStatusLabel1, ToolStripStatusLabel2, ToolStripStatusLabel3})
        StatusStrip1.Location = New Point(0, 629)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Size = New Size(1035, 22)
        StatusStrip1.TabIndex = 7
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Image = CType(resources.GetObject("ToolStripStatusLabel1.Image"), Image)
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New Size(55, 17)
        ToolStripStatusLabel1.Text = "Ready"
        ' 
        ' ToolStripStatusLabel2
        ' 
        ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        ToolStripStatusLabel2.Size = New Size(965, 17)
        ToolStripStatusLabel2.Spring = True
        ' 
        ' ToolStripStatusLabel3
        ' 
        ToolStripStatusLabel3.Name = "ToolStripStatusLabel3"
        ToolStripStatusLabel3.Size = New Size(0, 17)
        ' 
        ' ContextMenuStrip1
        ' 
        ContextMenuStrip1.Name = "ContextMenuStrip1"
        ContextMenuStrip1.Size = New Size(61, 4)
        ' 
        ' RsharpDevMain
        ' 
        ClientSize = New Size(1035, 651)
        Controls.Add(DockPanel1)
        Controls.Add(StatusStrip1)
        Controls.Add(RibbonUI)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "RsharpDevMain"
        StartPosition = FormStartPosition.CenterScreen
        Text = "R# Develop Studio"
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub

#End Region

    Friend WithEvents RibbonUI As Ribbon
    Friend WithEvents VS2015LightTheme1 As VS2015LightTheme
    Friend WithEvents VS2015DarkTheme1 As VS2015DarkTheme
    Friend WithEvents VS2015BlueTheme1 As VS2015BlueTheme
    Friend WithEvents VisualStudioToolStripExtender1 As VisualStudioToolStripExtender
    Friend WithEvents DockPanel1 As DockPanel
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel2 As ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel3 As ToolStripStatusLabel
End Class

