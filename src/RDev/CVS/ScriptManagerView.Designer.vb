<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScriptManagerView
    Inherits System.Windows.Forms.UserControl

    'UserControl 重写释放以清理组件列表。
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

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.WebView21 = New Microsoft.Web.WebView2.WinForms.WebView2()
        Me.Editor = New VsCodeEditor()
        Me.tsTools = New System.Windows.Forms.ToolStrip()
        Me.btnNew = New System.Windows.Forms.ToolStripButton()
        Me.btnSave = New System.Windows.Forms.ToolStripButton()
        Me.btnRun = New System.Windows.Forms.ToolStripButton()
        Me.btnRefresh = New System.Windows.Forms.ToolStripButton()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.tsTools.SuspendLayout()
        CType(Me.WebView21, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tsTools
        '
        Me.tsTools.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnNew, Me.btnSave, Me.btnRun, Me.btnRefresh})
        Me.tsTools.Location = New System.Drawing.Point(0, 0)
        Me.tsTools.Name = "tsTools"
        Me.tsTools.Size = New System.Drawing.Size(1000, 27)
        Me.tsTools.TabIndex = 0
        Me.tsTools.Text = "tools"
        '
        'btnNew
        '
        Me.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnNew.Name = "btnNew"
        Me.btnNew.Size = New System.Drawing.Size(70, 22)
        Me.btnNew.Text = "新建脚本"
        '
        'btnSave
        '
        Me.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(82, 22)
        Me.btnSave.Text = "保存新版本"
        '
        'btnRun
        '
        Me.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnRun.Name = "btnRun"
        Me.btnRun.Size = New System.Drawing.Size(46, 22)
        Me.btnRun.Text = "运行"
        '
        'btnRefresh
        '
        Me.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(46, 22)
        Me.btnRefresh.Text = "刷新"
        '
        'splitMain
        '
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.Location = New System.Drawing.Point(0, 27)
        Me.splitMain.Name = "splitMain"
        Me.splitMain.Panel1.Controls.Add(Me.WebView21)
        Me.splitMain.Panel1MinSize = 320
        Me.splitMain.Panel2.Controls.Add(Me.Editor)
        Me.splitMain.Panel2MinSize = 360
        Me.splitMain.Size = New System.Drawing.Size(1000, 613)
        Me.splitMain.SplitterDistance = 420
        Me.splitMain.TabIndex = 1
        '
        'WebView21
        '
        Me.WebView21.AllowExternalDrop = True
        Me.WebView21.CreationProperties = Nothing
        Me.WebView21.DefaultBackgroundColor = System.Drawing.Color.White
        Me.WebView21.Dock = System.Windows.Forms.DockStyle.Fill
        Me.WebView21.Location = New System.Drawing.Point(0, 0)
        Me.WebView21.Name = "WebView21"
        Me.WebView21.Size = New System.Drawing.Size(420, 613)
        Me.WebView21.TabIndex = 0
        Me.WebView21.ZoomFactor = 1.0R
        '
        'Editor
        '
        Me.Editor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Editor.FilePath = Nothing
        Me.Editor.Location = New System.Drawing.Point(0, 0)
        Me.Editor.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Editor.Name = "Editor"
        Me.Editor.Size = New System.Drawing.Size(576, 613)
        Me.Editor.TabIndex = 0
        '
        'ScriptManagerView
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.tsTools)
        Me.Name = "ScriptManagerView"
        Me.Size = New System.Drawing.Size(1000, 640)
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.tsTools.ResumeLayout(False)
        CType(Me.WebView21, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents WebView21 As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents Editor As VsCodeEditor
    Friend WithEvents tsTools As System.Windows.Forms.ToolStrip
    Friend WithEvents btnNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnSave As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnRun As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnRefresh As System.Windows.Forms.ToolStripButton
End Class
