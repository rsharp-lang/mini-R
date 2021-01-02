Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Public Module Description

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
        End Select

        Return Nothing
    End Function

    <Extension>
    Public Function GetDescription(func As DeclareNewFunction) As String
        Dim text As String = $"Declare a new function symbol '{func.funcName}':"

        Return text
    End Function
End Module
