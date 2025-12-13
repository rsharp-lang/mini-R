<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormViewer
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        MenuStrip1 = New MenuStrip()
        FileToolStripMenuItem = New ToolStripMenuItem()
        OpenToolStripMenuItem = New ToolStripMenuItem()
        ToolStripMenuItem1 = New ToolStripSeparator()
        ExitToolStripMenuItem = New ToolStripMenuItem()
        DockPanel1 = New Microsoft.VisualStudio.WinForms.Docking.DockPanel()
        VisualStudioToolStripExtender1 = New Microsoft.VisualStudio.WinForms.Docking.VisualStudioToolStripExtender(components)
        StatusStrip1 = New StatusStrip()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Items.AddRange(New ToolStripItem() {FileToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(942, 24)
        MenuStrip1.TabIndex = 1
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' FileToolStripMenuItem
        ' 
        FileToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {OpenToolStripMenuItem, ToolStripMenuItem1, ExitToolStripMenuItem})
        FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        FileToolStripMenuItem.Size = New Size(37, 20)
        FileToolStripMenuItem.Text = "File"
        ' 
        ' OpenToolStripMenuItem
        ' 
        OpenToolStripMenuItem.Name = "OpenToolStripMenuItem"
        OpenToolStripMenuItem.Size = New Size(103, 22)
        OpenToolStripMenuItem.Text = "Open"
        ' 
        ' ToolStripMenuItem1
        ' 
        ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        ToolStripMenuItem1.Size = New Size(100, 6)
        ' 
        ' ExitToolStripMenuItem
        ' 
        ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        ExitToolStripMenuItem.Size = New Size(103, 22)
        ExitToolStripMenuItem.Text = "Exit"
        ' 
        ' DockPanel1
        ' 
        DockPanel1.Dock = DockStyle.Fill
        DockPanel1.Location = New Point(0, 24)
        DockPanel1.Name = "DockPanel1"
        DockPanel1.Size = New Size(942, 536)
        DockPanel1.TabIndex = 2
        ' 
        ' VisualStudioToolStripExtender1
        ' 
        VisualStudioToolStripExtender1.DefaultRenderer = Nothing
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.Location = New Point(0, 560)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Size = New Size(942, 22)
        StatusStrip1.TabIndex = 3
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' FormViewer
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(942, 582)
        Controls.Add(DockPanel1)
        Controls.Add(StatusStrip1)
        Controls.Add(MenuStrip1)
        Name = "FormViewer"
        StartPosition = FormStartPosition.CenterScreen
        Text = "HDS Pack Viewer"
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents OpenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As ToolStripSeparator
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DockPanel1 As Microsoft.VisualStudio.WinForms.Docking.DockPanel
    Friend WithEvents VisualStudioToolStripExtender1 As Microsoft.VisualStudio.WinForms.Docking.VisualStudioToolStripExtender
    Friend WithEvents StatusStrip1 As StatusStrip

End Class
