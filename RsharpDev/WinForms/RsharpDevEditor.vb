Imports System.Text
Imports FastColoredTextBoxNS
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports RDev
Imports WeifenLuo.WinFormsUI.Docking
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Public Class RsharpDevEditor : Inherits DockContent
    Implements ISaveHandle
    Implements IFileReference

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Return {
                New ContentType With {.Details = "http://r_lang.dev.smrucc.org/", .FileExt = ".R", .MIMEType = "text/r_sharp", .Name = "R# script"}
            }
        End Get
    End Property

    Private Sub RsharpDevEditor_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.ShowIcon = True

        Dim syntaxHighlighter As New SyntaxHighlighter(FastColoredTextBox1)

        FastColoredTextBox1.Text = "#!/usr/local/bin/R#"
        FastColoredTextBox1.SyntaxHighlighter = syntaxHighlighter

        Call LoadScript("E:\mini-R\test\SyntaxHighlightTest.R".ReadAllText)
    End Sub

    Public Sub LoadScript(script As String)
        FastColoredTextBox1.Text = script
    End Sub

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function

    Dim blue As New TextStyle(Brushes.Blue, Nothing, FontStyle.Regular)
    Dim green As New TextStyle(Brushes.Green, Nothing, FontStyle.Italic)
    Dim red As New TextStyle(Brushes.RosyBrown, Nothing, FontStyle.Bold)
    Dim endSymbol As New TextStyle(Brushes.Black, Nothing, FontStyle.Bold)

    Dim keywords As String = "(\s)?(" & {
        "let", "const", "as", "integer",
        "imports", "from",
        "in",
        "using"
    }.Select(Function(a) $"({a})").JoinBy("|") & ")\s"

    Dim keyword2 As String = "\s(" & {
        "function", "double", "boolean", "string", "integer",
        "list", "for", "if", "else"
    }.Select(Function(a) $"({a})") _
     .JoinBy("|") & ")(\s|\)|,)"

    Private Sub FastColoredTextBox1_ToolTipNeeded(sender As Object, e As ToolTipNeededEventArgs) Handles FastColoredTextBox1.ToolTipNeeded
        If Not String.IsNullOrEmpty(e.HoveredWord) Then
            e.ToolTipTitle = e.HoveredWord
            e.ToolTipText = GetTooltipContent(Strings.Trim(e.HoveredWord))
        End If

        Dim range As New Range(sender, e.Place, e.Place)
        Dim hoveredWord = range.GetFragment("[^\n]").Text

        e.ToolTipTitle = hoveredWord
        e.ToolTipText = GetTooltipContent(Strings.Trim(hoveredWord))
    End Sub

    Private Function GetTooltipContent(hoveredWord As String) As String
        hoveredWord = hoveredWord.Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)

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
        End Select

        Return Description.GetDescription(hoveredWord)
    End Function

    Private Sub FastColoredTextBox1_TextChanged(sender As Object, e As TextChangedEventArgs) Handles FastColoredTextBox1.TextChanged
        ' clear folding markers of changed range
        e.ChangedRange.ClearFoldingMarkers()
        ' set folding markers
        e.ChangedRange.SetFoldingMarkers("{", "}")
        e.ChangedRange.SetFoldingMarkers("#region", "#endregion")

        e.ChangedRange.ClearStyle(blue, green, red, endSymbol)
        e.ChangedRange.SetStyle(green, "#.*")
        e.ChangedRange.SetStyle(red, "([""].*[""])|(['].*['])|([`].*[`])")
        e.ChangedRange.SetStyle(blue, keywords)
        e.ChangedRange.SetStyle(blue, keyword2)
        e.ChangedRange.SetStyle(endSymbol, ";")
    End Sub
End Class