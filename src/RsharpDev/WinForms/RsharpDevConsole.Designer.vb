Imports WeifenLuo.WinFormsUI.Docking

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RsharpDevConsole
    Inherits DockContent

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RsharpDevConsole))
        Me.ConsoleControl1 = New Microsoft.VisualBasic.Windows.Forms.ConsoleControl()
        Me.SuspendLayout()
        '
        'ConsoleControl1
        '
        Me.ConsoleControl1.BackColor = System.Drawing.Color.Black
        Me.ConsoleControl1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ConsoleControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ConsoleControl1.Font = New System.Drawing.Font("Consolas", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ConsoleControl1.ForeColor = System.Drawing.Color.Lime
        Me.ConsoleControl1.Location = New System.Drawing.Point(0, 0)
        Me.ConsoleControl1.Name = "ConsoleControl1"
        Me.ConsoleControl1.Ps1Pattern = ""
        Me.ConsoleControl1.ReadOnly = True
        Me.ConsoleControl1.Size = New System.Drawing.Size(800, 450)
        Me.ConsoleControl1.TabIndex = 0
        Me.ConsoleControl1.Text = ""
        '
        'RsharpDevConsole
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.ConsoleControl1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "RsharpDevConsole"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ConsoleControl1 As Microsoft.VisualBasic.Windows.Forms.ConsoleControl
End Class
