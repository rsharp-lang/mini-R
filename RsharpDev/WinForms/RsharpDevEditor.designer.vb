<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RsharpDevEditor

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RsharpDevEditor))
        Me.FastColoredTextBox1 = New FastColoredTextBoxNS.FastColoredTextBox()
        Me.DocumentMap1 = New FastColoredTextBoxNS.DocumentMap()
        CType(Me.FastColoredTextBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'FastColoredTextBox1
        '
        Me.FastColoredTextBox1.AutoCompleteBracketsList = New Char() {Global.Microsoft.VisualBasic.ChrW(40), Global.Microsoft.VisualBasic.ChrW(41), Global.Microsoft.VisualBasic.ChrW(123), Global.Microsoft.VisualBasic.ChrW(125), Global.Microsoft.VisualBasic.ChrW(91), Global.Microsoft.VisualBasic.ChrW(93), Global.Microsoft.VisualBasic.ChrW(34), Global.Microsoft.VisualBasic.ChrW(34), Global.Microsoft.VisualBasic.ChrW(39), Global.Microsoft.VisualBasic.ChrW(39)}
        Me.FastColoredTextBox1.AutoIndentCharsPatterns = "^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;=]+);" & Global.Microsoft.VisualBasic.ChrW(10) & "^\s*(case|default)\s*[^:]*(" &
    "?<range>:)\s*(?<range>[^;]+);"
        Me.FastColoredTextBox1.AutoScrollMinSize = New System.Drawing.Size(109, 15)
        Me.FastColoredTextBox1.BackBrush = Nothing
        Me.FastColoredTextBox1.CharHeight = 15
        Me.FastColoredTextBox1.CharWidth = 7
        Me.FastColoredTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.FastColoredTextBox1.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(100, Byte), Integer), CType(CType(180, Byte), Integer), CType(CType(180, Byte), Integer), CType(CType(180, Byte), Integer))
        Me.FastColoredTextBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FastColoredTextBox1.Font = New System.Drawing.Font("Consolas", 9.75!)
        Me.FastColoredTextBox1.IsReplaceMode = False
        Me.FastColoredTextBox1.Location = New System.Drawing.Point(0, 0)
        Me.FastColoredTextBox1.Name = "FastColoredTextBox1"
        Me.FastColoredTextBox1.Paddings = New System.Windows.Forms.Padding(0)
        Me.FastColoredTextBox1.SelectionColor = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.FastColoredTextBox1.ServiceColors = CType(resources.GetObject("FastColoredTextBox1.ServiceColors"), FastColoredTextBoxNS.ServiceColors)
        Me.FastColoredTextBox1.Size = New System.Drawing.Size(787, 627)
        Me.FastColoredTextBox1.TabIndex = 0
        Me.FastColoredTextBox1.Text = "#/usr/bin/R#"
        Me.FastColoredTextBox1.Zoom = 100
        '
        'DocumentMap1
        '
        Me.DocumentMap1.Dock = System.Windows.Forms.DockStyle.Right
        Me.DocumentMap1.ForeColor = System.Drawing.Color.Maroon
        Me.DocumentMap1.Location = New System.Drawing.Point(787, 0)
        Me.DocumentMap1.Name = "DocumentMap1"
        Me.DocumentMap1.Size = New System.Drawing.Size(254, 627)
        Me.DocumentMap1.TabIndex = 1
        Me.DocumentMap1.Target = Me.FastColoredTextBox1
        Me.DocumentMap1.Text = "DocumentMap1"
        '
        'RsharpDevEditor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1041, 627)
        Me.Controls.Add(Me.FastColoredTextBox1)
        Me.Controls.Add(Me.DocumentMap1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "RsharpDevEditor"
        Me.Text = "New Script"
        CType(Me.FastColoredTextBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents FastColoredTextBox1 As FastColoredTextBoxNS.FastColoredTextBox
    Friend WithEvents DocumentMap1 As FastColoredTextBoxNS.DocumentMap
End Class
