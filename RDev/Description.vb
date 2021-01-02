Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
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

        Return GetDescription(Program.BuildProgram(text))
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

        End Select

        Return Nothing
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

            Return help.ToString & vbCrLf & vbCrLf & text
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
