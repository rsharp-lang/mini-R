<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ToolWinServers
    Inherits ToolWindow

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
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButtonAddServer = New System.Windows.Forms.ToolStripButton()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TreeView1
        '
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.Location = New System.Drawing.Point(0, 25)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.Size = New System.Drawing.Size(499, 503)
        Me.TreeView1.TabIndex = 1
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButtonAddServer})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(499, 25)
        Me.ToolStrip1.TabIndex = 2
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'ToolStripButtonAddServer
        '
        Me.ToolStripButtonAddServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonAddServer.Image = Global.My.Resources.Resources.AddMark_10580
        Me.ToolStripButtonAddServer.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonAddServer.Name = "ToolStripButtonAddServer"
        Me.ToolStripButtonAddServer.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonAddServer.Text = "Add Server"
        '
        'ToolWinServers
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(499, 528)
        Me.Controls.Add(Me.TreeView1)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Name = "ToolWinServers"
        Me.Text = "Server Explorer"
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TreeView1 As TreeView
    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents ToolStripButtonAddServer As ToolStripButton
End Class
