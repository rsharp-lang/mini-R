Imports System.Drawing
Imports System.Threading.Tasks
Imports System.Windows.Forms

''' <summary>
''' a small dialog for entering the change comment of a new version. it replaces
''' the plain Interaction.InputBox so that an "AI 生成" button can be hosted next
''' to the text box. the comment generator is injected as a Func(Of Task(Of String))
''' (the shared GenerateCommentAsync of the host control), and a diff provider is
''' injected as a Func(Of String) so that the second tab can show the MyersDiff
''' of the editor content against the previous version for manual review.
''' </summary>
Public Class CommentDialog : Inherits Form

    Private ReadOnly txtComment As New TextBox
    Private ReadOnly rtbDiff As New RichTextBox
    Private ReadOnly tabControl As New TabControl
    Private ReadOnly tabComment As New TabPage
    Private ReadOnly tabDiff As New TabPage
    Private WithEvents btnAIGen As New Button
    Private ReadOnly btnOK As New Button
    Private ReadOnly btnCancel As New Button
    Private WithEvents btnRefreshDiff As New Button
    Private ReadOnly generate As Func(Of Task(Of String))
    Private ReadOnly diffFunc As Func(Of String)
    Private _diffText As String = ""

    Public Sub New(generate As Func(Of Task(Of String)), Optional diffFunc As Func(Of String) = Nothing)
        Me.generate = generate
        Me.diffFunc = diffFunc
        Call BuildLayout()
    End Sub

    ''' <summary>the comment text that the user entered / the AI generated</summary>
    Public ReadOnly Property Comment As String
        Get
            Return txtComment.Text
        End Get
    End Property

    ''' <summary>the unified diff text, recomputed live from the injected provider</summary>
    Public Property DiffText As String
        Get
            If diffFunc IsNot Nothing Then
                Return diffFunc()
            End If
            Return _diffText
        End Get
        Set(value As String)
            _diffText = If(value, "")
        End Set
    End Property

    Private Sub BuildLayout()
        Me.Text = "保存新版本"
        Me.Size = New Size(580, 400)
        Me.MinimumSize = New Size(380, 280)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ShowIcon = False
        Me.Font = New Font("Segoe UI", 9.75!)
        Me.Padding = New Padding(10)

        ' ---- tab 1: the change comment ----
        txtComment.Multiline = True
        txtComment.ScrollBars = ScrollBars.Vertical
        txtComment.Dock = DockStyle.Fill
        txtComment.Font = New Font("Consolas", 10!)
        txtComment.Text = ""

        tabComment.Text = "变更说明"
        tabComment.Controls.Add(txtComment)

        ' ---- tab 2: the MyersDiff review ----
        rtbDiff.ReadOnly = True
        rtbDiff.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbDiff.Dock = DockStyle.Fill
        rtbDiff.BorderStyle = BorderStyle.None
        rtbDiff.WordWrap = False
        rtbDiff.Font = New Font("Consolas", 9.75!)

        btnRefreshDiff.Text = "刷新差异"
        btnRefreshDiff.Size = New Size(84, 26)


        Dim diffHead As New FlowLayoutPanel
        diffHead.FlowDirection = FlowDirection.RightToLeft
        diffHead.Dock = DockStyle.Top
        diffHead.Height = 30
        diffHead.Padding = New Padding(0, 2, 0, 2)
        diffHead.Controls.Add(btnRefreshDiff)

        tabDiff.Text = "差异审阅"
        tabDiff.Controls.Add(diffHead)
        tabDiff.Controls.Add(rtbDiff)

        tabControl.Dock = DockStyle.Fill
        tabControl.TabPages.Add(tabComment)
        tabControl.TabPages.Add(tabDiff)

        ' ---- bottom button row ----
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
        Me.Controls.Add(tabControl)

        Me.AcceptButton = btnOK
        Me.CancelButton = btnCancel

        ' show the diff (if available) in the review tab
        Call ColorizeDiff(rtbDiff, Me.DiffText)
    End Sub

    ''' <summary>render the unified diff with light green / red backgrounds by line</summary>
    Private Sub ColorizeDiff(rtb As RichTextBox, text As String)
        rtb.Clear()

        If text.StringEmpty Then
            Return
        End If

        Dim lines As String() = text.Replace(vbCrLf, vbLf).Split(vbLf)

        For i As Integer = 0 To lines.Length - 1
            Dim line As String = lines(i)
            Dim back As Color = Color.White

            If line.StartsWith("+") Then
                back = Color.FromArgb(230, 245, 233)
            ElseIf line.StartsWith("-") Then
                back = Color.FromArgb(250, 235, 235)
            ElseIf line.StartsWith("@@") OrElse line.StartsWith("diff ") OrElse line.StartsWith("===") Then
                back = Color.FromArgb(235, 242, 250)
            End If

            rtb.SelectionBackColor = back
            rtb.AppendText(line & vbLf)
        Next
    End Sub

    Private Sub btnRefreshDiff_Click(sender As Object, e As EventArgs) Handles btnRefreshDiff.Click
        Call ColorizeDiff(rtbDiff, Me.DiffText)
    End Sub

    Private Async Sub btnAIGen_Click(sender As Object, e As EventArgs) Handles btnAIGen.Click
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
