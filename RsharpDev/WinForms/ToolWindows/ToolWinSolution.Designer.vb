<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ToolWinSolution
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
        Me.components = New System.ComponentModel.Container()
        Dim TreeNode1 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("assembly")
        Dim TreeNode2 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("data")
        Dim TreeNode3 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("man")
        Dim TreeNode4 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("R")
        Dim TreeNode5 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("Solution", New System.Windows.Forms.TreeNode() {TreeNode1, TreeNode2, TreeNode3, TreeNode4})
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ToolWinSolution))
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.NewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ScriptToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.BuildToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.ContextMenuStrip1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewToolStripMenuItem, Me.BuildToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(102, 48)
        '
        'NewToolStripMenuItem
        '
        Me.NewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ScriptToolStripMenuItem})
        Me.NewToolStripMenuItem.Name = "NewToolStripMenuItem"
        Me.NewToolStripMenuItem.Size = New System.Drawing.Size(101, 22)
        Me.NewToolStripMenuItem.Text = "Add"
        '
        'ScriptToolStripMenuItem
        '
        Me.ScriptToolStripMenuItem.Name = "ScriptToolStripMenuItem"
        Me.ScriptToolStripMenuItem.Size = New System.Drawing.Size(104, 22)
        Me.ScriptToolStripMenuItem.Text = "Script"
        '
        'BuildToolStripMenuItem
        '
        Me.BuildToolStripMenuItem.Name = "BuildToolStripMenuItem"
        Me.BuildToolStripMenuItem.Size = New System.Drawing.Size(101, 22)
        Me.BuildToolStripMenuItem.Text = "Build"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.Button1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(471, 32)
        Me.Panel1.TabIndex = 1
        '
        'Button1
        '
        Me.Button1.BackgroundImage = Global.My.Resources.Resources.package_32xLG
        Me.Button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Button1.Location = New System.Drawing.Point(12, 4)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(24, 24)
        Me.Button1.TabIndex = 0
        Me.Button1.UseVisualStyleBackColor = True
        '
        'TreeView1
        '
        Me.TreeView1.ContextMenuStrip = Me.ContextMenuStrip1
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.FullRowSelect = True
        Me.TreeView1.HotTracking = True
        Me.TreeView1.ImageIndex = 0
        Me.TreeView1.ImageList = Me.ImageList1
        Me.TreeView1.LabelEdit = True
        Me.TreeView1.Location = New System.Drawing.Point(0, 32)
        Me.TreeView1.Name = "TreeView1"
        TreeNode1.ImageIndex = 2
        TreeNode1.Name = "Node3"
        TreeNode1.Text = "assembly"
        TreeNode2.ImageIndex = 2
        TreeNode2.Name = "Node2"
        TreeNode2.Text = "data"
        TreeNode3.ImageIndex = 2
        TreeNode3.Name = "Node1"
        TreeNode3.Text = "man"
        TreeNode4.ImageIndex = 2
        TreeNode4.Name = "Node0"
        TreeNode4.Text = "R"
        TreeNode5.ImageIndex = 5
        TreeNode5.Name = "Node4"
        TreeNode5.Text = "Solution"
        Me.TreeView1.Nodes.AddRange(New System.Windows.Forms.TreeNode() {TreeNode5})
        Me.TreeView1.SelectedImageIndex = 0
        Me.TreeView1.ShowNodeToolTips = True
        Me.TreeView1.Size = New System.Drawing.Size(471, 553)
        Me.TreeView1.StateImageList = Me.ImageList1
        Me.TreeView1.TabIndex = 2
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "class_32xLG.png")
        Me.ImageList1.Images.SetKeyName(1, "document_32xLG.png")
        Me.ImageList1.Images.SetKeyName(2, "folder_Closed_32xLG.png")
        Me.ImageList1.Images.SetKeyName(3, "folder_Open_32xLG.png")
        Me.ImageList1.Images.SetKeyName(4, "script_32xLG.png")
        Me.ImageList1.Images.SetKeyName(5, "Solution_8308.png")
        '
        'ToolWinSolution
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(471, 585)
        Me.Controls.Add(Me.TreeView1)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "ToolWinSolution"
        Me.Text = "Form1"
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents NewToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ScriptToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents BuildToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Panel1 As Panel
    Friend WithEvents TreeView1 As TreeView
    Friend WithEvents Button1 As Button
    Friend WithEvents ImageList1 As ImageList
End Class
