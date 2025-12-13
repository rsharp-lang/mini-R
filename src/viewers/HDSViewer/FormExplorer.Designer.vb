Imports Galaxy.Workbench.DockDocument

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormExplorer
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
        components = New ComponentModel.Container()
        ToolStrip1 = New ToolStrip()
        SuspendLayout()
        ' 
        ' ToolStrip1
        ' 
        ToolStrip1.Location = New Point(0, 0)
        ToolStrip1.Name = "ToolStrip1"
        ToolStrip1.Size = New Size(363, 25)
        ToolStrip1.TabIndex = 0
        ToolStrip1.Text = "ToolStrip1"
        ' 
        ' FormExplorer
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(363, 649)
        Controls.Add(ToolStrip1)
        DockAreas = Microsoft.VisualStudio.WinForms.Docking.DockAreas.Float Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockLeft Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockRight Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockTop Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockBottom Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.Document
        DoubleBuffered = True
        Name = "FormExplorer"
        ShowHint = Microsoft.VisualStudio.WinForms.Docking.DockState.Unknown
        Text = "Explorer"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents ToolStrip1 As ToolStrip
End Class
