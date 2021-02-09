Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports FastColoredTextBoxNS
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports any = Microsoft.VisualBasic.Scripting

Public Class Editor
    Implements ISaveHandle
    Implements IFileReference

    Public Event OnFocus()

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Return {
                New ContentType With {.Details = "http://r_lang.dev.smrucc.org/", .FileExt = ".R", .MIMEType = "text/r_sharp", .Name = "R# script"}
            }
        End Get
    End Property

    Public ReadOnly Property ScriptText As String
        Get
            Return FastColoredTextBox1.Text
        End Get
    End Property

    Public Event EditCode()

    Public ReadOnly Property IsEdited As Boolean

    Private Sub Editor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim syntaxHighlighter As New SyntaxHighlighter(FastColoredTextBox1)

        FastColoredTextBox1.Text = "#!/usr/local/bin/R#"
        FastColoredTextBox1.SyntaxHighlighter = syntaxHighlighter
    End Sub

    Public Sub LoadScript(script As String)
        If script.FileExists Then
            FilePath = script
            script = script.ReadAllText
        End If

        FastColoredTextBox1.Text = script

        Call RefreshSymbolList()
    End Sub

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Return FastColoredTextBox1.Text.SaveTo(path, encoding)
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Return FastColoredTextBox1.Text.SaveTo(path, encoding.CodePage)
    End Function

    ''' <summary>
    ''' 关键词
    ''' </summary>
    Dim blue As New TextStyle(Brushes.Blue, Nothing, FontStyle.Regular)
    ''' <summary>
    ''' 代码注释
    ''' </summary>
    Dim green As New TextStyle(Brushes.Green, Nothing, FontStyle.Italic)
    ''' <summary>
    ''' 字符串
    ''' </summary>
    Dim red As New TextStyle(Brushes.Brown, Nothing, FontStyle.Bold)
    ''' <summary>
    ''' 结束符号
    ''' </summary>
    Dim endSymbol As New TextStyle(Brushes.Black, Nothing, FontStyle.Bold)
    ''' <summary>
    ''' 内部函数
    ''' </summary>
    Dim purple As New TextStyle(Brushes.Purple, Nothing, FontStyle.Regular)
    ''' <summary>
    ''' 数字
    ''' </summary>
    Dim orange As New TextStyle(Brushes.OrangeRed, Nothing, FontStyle.Bold)
    Dim link As New TextStyle(Brushes.Blue, Brushes.AliceBlue, FontStyle.Underline Or FontStyle.Bold)
    Dim colorCode As New ColorStyle
    Dim pipeLine As New TextStyle(Brushes.Red, Brushes.LightGray, FontStyle.Underline)
    Dim interpolate As New TextStyle(Brushes.Black, Brushes.AliceBlue, FontStyle.Italic)
    Dim funcCall As New TextStyle(Brushes.Black, Brushes.AliceBlue, FontStyle.Regular)

    Dim callFunc As String = "[^\s]+\s*\("
    Dim stringInterpolate As String = "[$]\{.+?\}"

    Dim buildInfunction As String = "\s?(" & {
        "list", "stop", "print"
    }.Select(Function(a) $"({a})") _
     .JoinBy("|") & ")\s*\("

    Dim startKeyword As String = "(let|const|imports|using)\s"
    Dim keywords As String = "\s(" & {
        "as", "integer",
        "from",
        "in"
    }.Select(Function(a) $"({a})").JoinBy("|") & ")\s"

    Dim numbers As String = "[-]?\d*(\.\d+)?([Ee][-]?\d+)?"
    Dim functionKeyword As String = "\s(function|if|for|require)\s*\("
    Dim keyword2 As String = "\s(" & {
        "double", "boolean", "string", "integer",
        "else"
    }.Select(Function(a) $"({a})") _
     .JoinBy("|") & ")(\s|\)|,)"

    Private Sub FastColoredTextBox1_ToolTipNeeded(sender As Object, e As ToolTipNeededEventArgs) Handles FastColoredTextBox1.ToolTipNeeded
        If Not String.IsNullOrEmpty(e.HoveredWord) Then
            e.ToolTipTitle = e.HoveredWord
            e.ToolTipText = vbCrLf & GetTooltipContent(Strings.Trim(e.HoveredWord)).DoCall(AddressOf Strings.Trim)
        End If

        Dim range As New Range(sender, e.Place, e.Place)
        Dim hoveredWord = range.GetFragment("[^\n]").Text

        e.ToolTipTitle = hoveredWord
        e.ToolTipText = vbCrLf & GetTooltipContent(Strings.Trim(hoveredWord)).DoCall(AddressOf Strings.Trim)
    End Sub

    Private Function GetTooltipContent(hoveredWord As String) As String
        hoveredWord = hoveredWord.Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)

        If hoveredWord.StringEmpty Then
            Return Nothing
        End If

        If hoveredWord.First = "}"c AndAlso hoveredWord.Last = "{"c Then
            hoveredWord = hoveredWord.GetStackValue("}", "{").Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
        End If

        Select Case hoveredWord
            Case "function" : Return "A keyword for identify current symbol is a function closure data object."
            Case "as" : Return "A keyword for add type constraint decorating to the target symbol."
            Case "let" : Return "A keyword for declare new symbol."
            Case "const" : Return "A keyword for declare a new symbol that not mutable."
            Case "string" : Return "R# character string type"
            Case "integer" : Return "R# int64 type"
            Case "double" : Return "R# float64 type"
            Case "boolean" : Return "R# logical type"
            Case "if" : Return "Execute the code closure based on the test condition is true or not."
            Case "else" : Return "Execute the code in next closure if the given test condition is FALSE."
            Case "using" : Return "A closure that will auto dispose the target symbol after finished the closure executation."
            Case "for" : Return $"Loop through each elements in the given sequence."
        End Select

        Return DescriptionTooltip.GetDescription(hoveredWord)
    End Function

    ''' <summary>
    ''' 顺序处于后面的优先级会很低
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FastColoredTextBox1_TextChanged(sender As Object, e As TextChangedEventArgs) Handles FastColoredTextBox1.TextChanged
        ' clear folding markers of changed range
        e.ChangedRange.ClearFoldingMarkers()
        ' set folding markers
        e.ChangedRange.SetFoldingMarkers("{", "}")
        e.ChangedRange.SetFoldingMarkers("\[", "\]")
        e.ChangedRange.SetFoldingMarkers("\(", "\)")
        e.ChangedRange.SetFoldingMarkers("""", """")
        e.ChangedRange.SetFoldingMarkers("#region", "#endregion")

        e.ChangedRange.ClearStyle(blue, green, red, endSymbol, link, colorCode, purple, pipeLine, interpolate, funcCall, orange)
        e.ChangedRange.SetStyle(link, "(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?")
        e.ChangedRange.SetStyle(colorCode, ColorStyle.htmlcolor)
        e.ChangedRange.SetStyle(green, "#.*")
        e.ChangedRange.SetStyle(interpolate, stringInterpolate)
        e.ChangedRange.SetStyle(red, "([""].*[""])|(['].*['])|([`].*[`])")

        e.ChangedRange.SetStyle(blue, keywords)
        e.ChangedRange.SetStyle(blue, keyword2)
        e.ChangedRange.SetStyle(blue, startKeyword)
        e.ChangedRange.SetStyle(blue, "\sfrom\s")
        e.ChangedRange.SetStyle(blue, functionKeyword)
        e.ChangedRange.SetStyle(purple, buildInfunction)
        e.ChangedRange.SetStyle(endSymbol, ";")
        e.ChangedRange.SetStyle(pipeLine, "[:]>")
        e.ChangedRange.SetStyle(funcCall, callFunc)
        e.ChangedRange.SetStyle(orange, numbers)

        _IsEdited = True

        RaiseEvent EditCode()

        Dim text As String = Strings.Trim(e.ChangedRange.Text)

        If text.IndexOf(ASCII.CR) > -1 OrElse text.IndexOf(ASCII.LF) > -1 Then
            Call RefreshSymbolList()
        End If
    End Sub

    Dim symbolJump As New Dictionary(Of String, Integer)

    Private Function RefreshSymbolList() As Integer
        Call ToolStripComboBox1.Items.Clear()

        For Each item As NamedValue(Of String) In DescriptionTooltip.GetSymbols(FastColoredTextBox1.Text)
            Call ToolStripComboBox1.Items.Add(item.Name)

            If Not symbolJump.ContainsKey(item.Name) Then
                Call symbolJump.Add(item.Name, Integer.Parse(item.Description))
                Call FastColoredTextBox1.BookmarkLine(symbolJump(item.Name) - 1)
            End If
        Next

        Return 0
    End Function

    Private Function CharIsHyperlink(place As Place) As Boolean
        Dim mask = FastColoredTextBox1.GetStyleIndexMask(New Style() {link})

        If (place.iChar < FastColoredTextBox1.GetLineLength(place.iLine)) Then
            If ((FastColoredTextBox1(place).style And mask) <> 0) Then
                Return True
            End If
        End If

        Return False
    End Function

    Private Sub FastColoredTextBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles FastColoredTextBox1.MouseMove
        Dim p = FastColoredTextBox1.PointToPlace(e.Location)

        If (CharIsHyperlink(p)) Then
            FastColoredTextBox1.Cursor = Cursors.Hand
        Else
            FastColoredTextBox1.Cursor = Cursors.IBeam
        End If
    End Sub

    Private Sub FastColoredTextBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles FastColoredTextBox1.MouseDown
        Dim p = FastColoredTextBox1.PointToPlace(e.Location)

        If (CharIsHyperlink(p)) Then
            Dim url = FastColoredTextBox1.GetRange(p, p).GetFragment("[\S]").Text

            ' bugs fixed of string interpolation like ${""}
            If url.StartsWith("${") Then
                url = url.GetStackValue("{", "}")
                url = url.Trim(""""c, "'"c, "`"c).Trim
            End If

            Process.Start(url)
        End If
    End Sub

    Private Sub DocumentMap1_GotFocus(sender As Object, e As EventArgs) Handles DocumentMap1.GotFocus
        RaiseEvent OnFocus()
    End Sub

    Private Sub FastColoredTextBox1_GotFocus(sender As Object, e As EventArgs) Handles FastColoredTextBox1.GotFocus
        RaiseEvent OnFocus()
    End Sub

    Private Sub Editor_GotFocus(sender As Object, e As EventArgs) Handles Me.GotFocus
        RaiseEvent OnFocus()
    End Sub

    Private Sub ToolStripComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ToolStripComboBox1.SelectedIndexChanged
        If Not symbolJump.IsNullOrEmpty Then
            Dim symbolLine As Integer = symbolJump(any.ToString(ToolStripComboBox1.SelectedItem))

            FastColoredTextBox1.YtoLineIndex(symbolLine)
        End If
    End Sub
End Class
