Imports Galaxy.Workbench.DockDocument

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RsharpCVS
    Inherits DocumentWindow

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
        ScriptManagerView1 = New RDev.ScriptManagerView()
        SuspendLayout()
        ' 
        ' ScriptManagerView1
        ' 
        ScriptManagerView1.DatabasePath = "C:\Users\Administrator\AppData\Local\RDev\scripts.rscvs"
        ScriptManagerView1.Dock = DockStyle.Fill
        ScriptManagerView1.Location = New Point(0, 0)
        ScriptManagerView1.Name = "ScriptManagerView1"
        ScriptManagerView1.RscriptPath = Nothing
        ScriptManagerView1.Size = New Size(1078, 681)
        ScriptManagerView1.TabIndex = 1
        ' 
        ' RsharpCVS
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1078, 681)
        Controls.Add(ScriptManagerView1)
        DockAreas = Microsoft.VisualStudio.WinForms.Docking.DockAreas.Float Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockLeft Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockRight Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockTop Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.DockBottom Or Microsoft.VisualStudio.WinForms.Docking.DockAreas.Document
        DoubleBuffered = True
        Name = "RsharpCVS"
        ShowHint = Microsoft.VisualStudio.WinForms.Docking.DockState.Unknown
        TabPageContextMenuStrip = DockContextMenuStrip1
        Text = "R# CSV"
        ResumeLayout(False)
    End Sub

    Friend WithEvents ScriptManagerView1 As RDev.ScriptManagerView
End Class
