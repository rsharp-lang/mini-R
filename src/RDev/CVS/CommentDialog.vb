Imports System.Drawing
Imports System.Threading.Tasks
Imports System.Windows.Forms

''' <summary>
''' a small dialog for entering the change comment of a new version. it replaces
''' the plain Interaction.InputBox so that an "AI 生成" button can be hosted next
''' to the text box. the comment generator is injected as a Func(Of Task(Of String))
''' (the shared GenerateCommentAsync of the host control).
''' </summary>
Public Class CommentDialog : Inherits Form

    Private ReadOnly txtComment As New TextBox
    Private ReadOnly btnAIGen As New Button
    Private ReadOnly btnOK As New Button
    Private ReadOnly btnCancel As New Button
    Private ReadOnly generate As Func(Of Task(Of String))

    Public Sub New(generate As Func(Of Task(Of String)))
        Me.generate = generate
        Call BuildLayout()
    End Sub

    ''' <summary>the comment text that the user entered / the AI generated</summary>
    Public ReadOnly Property Comment As String
        Get
            Return txtComment.Text
        End Get
    End Property

    Private Sub BuildLayout()
        Me.Text = "保存新版本"
        Me.Size = New Size(520, 320)
        Me.MinimumSize = New Size(360, 240)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ShowIcon = False
        Me.Font = New Font("Segoe UI", 9.75!)
        Me.Padding = New Padding(10)

        txtComment.Multiline = True
        txtComment.ScrollBars = ScrollBars.Vertical
        txtComment.Dock = DockStyle.Fill
        txtComment.Font = New Font("Consolas", 10!)
        txtComment.Text = ""

        btnAIGen.Text = "AI 生成"
        btnAIGen.Size = New Size(90, 28)
        btnAIGen.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        btnOK.Text = "确定"
        btnOK.Size = New Size(80, 28)
        btnOK.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnOK.DialogResult = DialogResult.OK

        btnCancel.Text = "取消"
        btnCancel.Size = New Size(80, 28)
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel

        Dim buttonRow As New FlowLayoutPanel
        buttonRow.FlowDirection = FlowDirection.RightToLeft
        buttonRow.Dock = DockStyle.Bottom
        buttonRow.Height = 40
        buttonRow.Padding = New Padding(0, 6, 0, 0)
        buttonRow.Controls.Add(btnCancel)
        buttonRow.Controls.Add(btnOK)
        buttonRow.Controls.Add(btnAIGen)

        Me.Controls.Add(buttonRow)
        Me.Controls.Add(txtComment)

        Me.AcceptButton = btnOK
        Me.CancelButton = btnCancel

        AddHandler btnAIGen.Click, AddressOf btnAIGen_Click
    End Sub

    Private Async Sub btnAIGen_Click(sender As Object, e As EventArgs)
        If generate Is Nothing Then
            Return
        End If

        btnAIGen.Enabled = False
        Dim oldText As String = btnAIGen.Text
        btnAIGen.Text = "生成中…"

        Try
            Dim text As String = Await generate()

            If Not String.IsNullOrEmpty(text) Then
                txtComment.Text = text
            End If
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "AI 生成失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            btnAIGen.Text = oldText
            btnAIGen.Enabled = True
        End Try
    End Sub
End Class
