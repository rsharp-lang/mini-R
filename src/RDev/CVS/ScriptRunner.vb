Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine

Namespace CVS

    ''' <summary>
    ''' the result of one R# script run task
    ''' </summary>
    Public Class RunResult

        Public Property stdout As String
        Public Property stderr As String
        Public Property exitCode As Integer
        Public Property durationMs As Long
        ''' <summary>the full commandline string of current run task</summary>
        Public Property commandLine As String
        ''' <summary>the temp script file that was executed</summary>
        Public Property scriptFile As String
        Public Property success As Boolean
        ''' <summary>not empty when the run task itself was failed</summary>
        Public Property message As String

        Public Shared Function Failed(message As String) As RunResult
            Return New RunResult With {
                .success = False,
                .message = message,
                .stdout = "",
                .stderr = "",
                .exitCode = -1
            }
        End Function
    End Class

    Public Interface IRunnerHost

        Sub SetTask(program As String, args As String, workdir As String)

        Function GetResult() As Task(Of RunResult)

    End Interface

    ''' <summary>
    ''' run the R# script via the R# script host (``Rscript.exe``) and then
    ''' capture the standard output/error text of the child process.
    ''' </summary>
    Public Class ScriptRunner

        ''' <summary>
        ''' the file path of the R# script host program, by default it is
        ''' resolved from the application home directory.
        ''' </summary>
        Public Property RscriptPath As String

        ''' <summary>
        ''' the directory that used for store the temp script file of the run task
        ''' </summary>
        Public Property TempFolder As String

        Dim host As Func(Of IRunnerHost)

        Sub New(host As Func(Of IRunnerHost), Optional rscript As String = Nothing, Optional temp As String = Nothing)
            Me.host = host
            Me.RscriptPath = If(rscript, ResolveRscript())
            Me.TempFolder = If(temp, DefaultTempFolder)
        End Sub

        Public Shared ReadOnly Property DefaultTempFolder As String
            Get
                Return System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RDev", "tmp")
            End Get
        End Property

        ''' <summary>
        ''' guess the location of the R# script host program
        ''' </summary>
        ''' <param name="hint">
        ''' the user specified directory or file path of the R# script host
        ''' </param>
        Public Shared Function ResolveRscript(Optional hint As String = Nothing) As String
            Dim candidates As New List(Of String)

            If Not hint.StringEmpty Then
                candidates.Add(hint)
            End If

            Dim env As String = Environment.GetEnvironmentVariable("RSHARP_CLI")

            If Not env.StringEmpty Then
                candidates.Add(env)
            End If

            Dim home As String = Microsoft.VisualBasic.App.HOME
            Dim startup As String = String.Empty

            Try
                startup = System.Windows.Forms.Application.StartupPath
            Catch ex As Exception
                startup = AppDomain.CurrentDomain.BaseDirectory
            End Try

            For Each dir As String In {home, startup}
                If dir.StringEmpty Then
                    Continue For
                End If

                ' Rscript.exe is the R# script host of the GCModeller runtime
                candidates.Add(System.IO.Path.Combine(dir, "Rscript.exe"))
                candidates.Add(System.IO.Path.Combine(dir, "R#.exe"))
                candidates.Add(System.IO.Path.Combine(dir, "R.exe"))
            Next

            For Each exe As String In candidates
                If exe.StringEmpty Then
                    Continue For
                End If
                If File.Exists(exe) Then
                    Return exe.GetFullPath
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' export the script text of one version as a temp script file, the
        ''' R# script host requires a real file for run the script.
        ''' </summary>
        Public Function ExportScript(versionId As String, code As String) As String
            If Not Directory.Exists(TempFolder) Then
                Call Directory.CreateDirectory(TempFolder)
            End If

            Dim name As String = versionId

            If name.StringEmpty Then
                name = "script"
            End If

            For Each c As Char In System.IO.Path.GetInvalidFileNameChars()
                name = name.Replace(c, "-"c)
            Next

            Dim scriptPath As String = System.IO.Path.Combine(TempFolder, name & ".R")
            Dim utf8 As New UTF8Encoding(False)

            Call File.WriteAllText(scriptPath, If(code, ""), utf8)

            Return scriptPath
        End Function

        ''' <summary>
        ''' build the commandline arguments of the R# script
        ''' </summary>
        ''' <param name="args">
        ''' the name/value pairs that comes from the html parameter form
        ''' </param>
        ''' <returns>
        ''' a commandline string in the format ``--name value`` (a boolean
        ''' switch or an empty value is rendered as a single ``--name`` flag)
        ''' </returns>
        Public Shared Function BuildArgumentLine(args As IEnumerable(Of RunArgument)) As String
            Dim cli As New List(Of String)

            If args Is Nothing Then
                Return ""
            End If

            For Each arg As RunArgument In args
                If arg Is Nothing OrElse arg.name.StringEmpty Then
                    Continue For
                End If

                Dim name As String = arg.name.Trim("-"c, "/"c)

                If name.StringEmpty Then
                    Continue For
                End If
                If arg.value.StringEmpty Then
                    cli.Add($"--{name}")
                ElseIf arg.value.TextEquals("TRUE") Then
                    cli.Add($"--{name}")
                ElseIf arg.value.TextEquals("FALSE") Then
                    ' a FALSE boolean flag is just omitted from the commandline
                    Continue For
                Else
                    cli.Add($"--{name} {arg.value.CLIToken}")
                End If
            Next

            Return cli.JoinBy(" ")
        End Function

        ''' <summary>
        ''' run the given R# script file and wait for its exit
        ''' </summary>
        ''' <param name="scriptFile">the R# script file that is to be executed</param>
        ''' <param name="args">the commandline arguments of the script</param>
        ''' <param name="workdir">the working directory of the script run</param>
        Public Async Function Run(scriptFile As String, args As IEnumerable(Of RunArgument), Optional workdir As String = Nothing) As Task(Of RunResult)
            If RscriptPath.StringEmpty OrElse Not File.Exists(RscriptPath) Then
                Return RunResult.Failed("R# script host 'Rscript.exe' is not found, please setup the location of the R# interpreter at first.")
            End If
            If Not File.Exists(scriptFile) Then
                Return RunResult.Failed($"the script file '{scriptFile}' is missing.")
            End If
            If Not workdir.StringEmpty AndAlso Not Directory.Exists(workdir) Then
                Try
                    Call Directory.CreateDirectory(workdir)
                Catch ex As Exception
                    workdir = Nothing
                End Try
            End If

            Dim argsLine As String = BuildArgumentLine(args)
            Dim cli As String = $"""{scriptFile}"" {argsLine}".Trim()
            Dim rhost As IRunnerHost = host()

            Call rhost.SetTask(RscriptPath, cli, If(workdir.StringEmpty, Nothing, workdir))

            Dim result As RunResult = Await rhost.GetResult

            result.commandLine = $"{RscriptPath} {cli}"
            result.scriptFile = scriptFile

            Return result
        End Function
    End Class
End Namespace
