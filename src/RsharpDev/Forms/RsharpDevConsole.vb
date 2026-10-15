Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Windows.Forms

Public Class RsharpDevConsole

    Dim _stdout As New List(Of String)
    Dim _stderr As New List(Of String)

    Dim program As String = "cmd.exe"
    Dim arguments As String

    Public ReadOnly Property stdout As String
        Get
            Return _stdout.JoinBy(vbCrLf)
        End Get
    End Property

    Public ReadOnly Property stderr As String
        Get
            Return _stderr.JoinBy(vbCrLf)
        End Get
    End Property

    Private Sub RsharpDevConsole_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = $"{program.CLIPath} {arguments}"
        WebViewConsole1.StartProcess(program, arguments)
    End Sub

    Public Function SetTask(program As String, args As String) As RsharpDevConsole
        Me.arguments = args
        Me.program = program

        _stdout.Clear()
        _stderr.Clear()

        Return Me
    End Function

    Private Sub WebViewConsole1_OnConsoleOutput(sender As Object, args As ConsoleEventArgs) Handles WebViewConsole1.OnConsoleOutput
        If args.Type = "stdout" Then
            Call _stdout.Add(args.Content)
        Else
            Call _stderr.Add(args.Content)
        End If
    End Sub
End Class