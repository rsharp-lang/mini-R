Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Windows.Forms
Imports Microsoft.VisualBasic.Windows.Forms.Win32
Imports RDev.CVS

Public Class RsharpDevConsole : Implements IRunnerHost

    Dim _stdout As New List(Of String)
    Dim _stderr As New List(Of String)

    Dim program As String = "cmd.exe"
    Dim arguments As String
    Dim t0 As Date

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
        TabText = "Console Host"
    End Sub

    Dim running As Boolean = False
    Dim exitCode As Integer = -1

    Public Sub Run()
        running = True
        TabText = $"{program.CLIPath} {arguments}".Trim
        WebViewConsole1.StartProcess(program, arguments, workdir)
        t0 = Now
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

    Dim workdir As String

    Private Sub IRunnerHost_SetTask(program As String, args As String, workdir As String) Implements IRunnerHost.SetTask
        Me.workdir = workdir

        Call SetTask(program, args)
        Call Run()
    End Sub

    Public Async Function GetResult() As Task(Of RunResult) Implements IRunnerHost.GetResult
        Do While running AndAlso App.Running
            Await Task.Delay(100)
        Loop

        Return New RunResult With {
            .durationMs = (Now - t0).TotalMilliseconds,
            .exitCode = exitCode,
            .stderr = stderr,
            .stdout = stdout,
            .success = exitCode = 0
        }
    End Function

    Private Sub WebViewConsole1_ProcessExisted(args As ProcessEventArgs) Handles WebViewConsole1.ProcessExisted
        exitCode = args.Code
    End Sub
End Class