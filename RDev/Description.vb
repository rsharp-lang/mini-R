Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Public Module Description

    Dim REngine As RInterpreter

    Public Sub SetEngine(engine As RInterpreter)
        REngine = engine
    End Sub

    Public Function GetDescription(text As String) As String
        If text.Last = "{"c Then
            text = text & "}"
        ElseIf text.Last = "(" Then
            text = text & ")"
        ElseIf text.Last = "[" Then
            text = text & "]"
        End If

        If text.StartsWith(":>") Then
            text = text.Substring(2).Trim
        End If
        If text.StringEmpty Then
            Return Nothing
        End If

        Try
            Return GetDescription(Program.BuildProgram(text))
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function GetDescription(program As Program) As String
        Return program.AsEnumerable.Select(AddressOf GetDescription).JoinBy(vbCrLf)
    End Function

    Public Function GetDescription(expr As Expression) As String
        Select Case expr.GetType
            Case GetType(SymbolReference) : Return $"A R# symbol named '{DirectCast(expr, SymbolReference).symbol}'"
            Case GetType(DeclareNewFunction) : Return DirectCast(expr, DeclareNewFunction).GetDescription()
            Case GetType(DeclareNewSymbol) : Return DirectCast(expr, DeclareNewSymbol).GetDescription
            Case GetType(FunctionInvoke) : Return DirectCast(expr, FunctionInvoke).GetDescription
            Case GetType(UsingClosure) : Return DirectCast(expr, UsingClosure).GetDescription
            Case GetType(IfBranch) : Return DirectCast(expr, IfBranch).GetDescription
            Case GetType(ForLoop) : Return DirectCast(expr, ForLoop).GetDescription

        End Select

        Return Nothing
    End Function

    <Extension>
    Public Function GetDescription([for] As ForLoop) As String
        Return $"Loop through each elements in the given sequence."
    End Function

    <Extension>
    Public Function GetDescription([if] As IfBranch) As String
        Return "Execute the next closure if the given test condition is TRUE."
    End Function

    <Extension>
    Public Function GetDescription([using] As UsingClosure) As String
        Return $"A closure that will auto dispose the target symbol after finished the closure executation."
    End Function

    <Extension>
    Public Function GetDescription(calls As FunctionInvoke) As String
        Dim func As Object = calls.GetFunctionVar(REngine.globalEnvir)

        If TypeOf func Is Message Then
            Return $"Run R# function '{calls.funcName}'."
        ElseIf TypeOf func Is RMethodInfo Then
            Dim method As RMethodInfo = func
            Dim text As String = method.GetPrintContent.Replace("``", "")
            Dim help As New StringBuilder
            Dim docs As ProjectMember = REngine.globalEnvir.packages.packageDocs.GetAnnotations(method.GetRawDeclares)

            If Not docs Is Nothing Then
                Using writer As New StringWriter(help)
                    Call docs.PrintText(writer)
                End Using
            End If

            Return help.ToString _
                .Replace("``", "") _
                .Replace("@T:", "") _
                .Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF) & vbCrLf & vbCrLf & text
        Else
            Return $"Run R# function '{calls.funcName}'."
        End If
    End Function

    <Extension>
    Public Function GetDescription(declareSymbol As DeclareNewSymbol) As String
        Dim text As String

        If declareSymbol.isTuple Then
            text = $"Declare symbols '{declareSymbol.names.JoinBy("', '")}' in tuple."
        Else
            text = $"Declare new symbol: '{declareSymbol.names(Scan0)}'"
        End If

        Return text
    End Function

    <Extension>
    Public Function GetDescription(func As DeclareNewFunction) As String
        Dim text As String = $"Declare a new function symbol '{func.funcName}':"

        Return text
    End Function
End Module
