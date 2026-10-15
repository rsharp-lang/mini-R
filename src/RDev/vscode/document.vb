Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Public Module document

    Public Property R As RInterpreter

    ''' <summary>
    ''' symbol|function|lambda
    ''' </summary>
    ''' <param name="script"></param>
    ''' <returns></returns>
    Public Iterator Function GetSymbols(script As String) As IEnumerable(Of NamedValue(Of String))
        Dim program As Program

        Try
            program = program.BuildProgram(script)
        Catch ex As Exception
            Return
        End Try

        If program Is Nothing Then
            Return
        End If

        For Each line As Expression In program
            If TypeOf line Is DeclareNewSymbol Then
                Dim newSymbol As DeclareNewSymbol = DirectCast(line, DeclareNewSymbol)

                For Each name As String In newSymbol.names
                    Yield New NamedValue(Of String) With {
                        .name = name,
                        .Value = "symbol",
                        .Description = newSymbol.stackFrame.Line
                    }
                Next
            ElseIf TypeOf line Is DeclareNewFunction Then
                Yield New NamedValue(Of String) With {
                    .Name = DirectCast(line, DeclareNewFunction).funcName,
                    .Value = "function",
                    .Description = DirectCast(line, DeclareNewFunction).stackFrame.Line
                }
            ElseIf TypeOf line Is DeclareLambdaFunction Then
                Yield New NamedValue(Of String) With {
                    .Name = DirectCast(line, DeclareLambdaFunction).name,
                    .Value = "lambda",
                    .Description = DirectCast(line, DeclareLambdaFunction).stackFrame.Line
                }
            End If
        Next
    End Function
End Module