Imports Config

Public Class ToolOutput : Implements IApplyVsTheme

    Dim active As TextBox

    Private Sub ToolOutput_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = "Output"

        Call ApplyVsTheme()
        Call VisualStudio.vsWindow.Add(Me)
        Call ToolStripComboBox1_SelectedIndexChanged()
    End Sub

    Public Sub AppendLine(line As String)
        Call Me.Invoke(Sub() Call txtBuild.AppendText(line & vbCrLf))
    End Sub

    Public Sub LogLanguageServer(line As String)
        Call Me.Invoke(Sub() Call TextBox1.AppendText(line & vbCrLf))
    End Sub

    Public Overrides Sub ApplyVsTheme() Implements IApplyVsTheme.ApplyVsTheme
        VisualStudioToolStripExtender1.ApplyVsTheme(ToolStrip1)
    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        txtBuild.Text = ""
        TextBox1.Text = ""
    End Sub

    Private Sub ToolStripComboBox1_SelectedIndexChanged() Handles ToolStripComboBox1.SelectedIndexChanged
        Select Case ToolStripComboBox1.SelectedIndex
            Case 0
                TextBox1.Visible = False
                TextBox1.Dock = DockStyle.None
                txtBuild.Visible = True
                txtBuild.Dock = DockStyle.Fill
            Case 1
                txtBuild.Visible = False
                txtBuild.Dock = DockStyle.None
                TextBox1.Visible = True
                TextBox1.Dock = DockStyle.Fill
        End Select
    End Sub
End Class