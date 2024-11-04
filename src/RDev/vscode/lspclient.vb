Imports System.Threading
Imports Darwinism.HPC.Parallel
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline

''' <summary>
''' client code for handling of the lsp backend
''' </summary>
Public Class lspclient

    ''' <summary>
    ''' the http port to the language server backend
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property lsp_server As Integer

    ReadOnly rscript As String
    ReadOnly languageserver As String
    ReadOnly vscode As String
    ReadOnly rstudio As String

    Dim background As RunSlavePipeline
    Dim process As Process
    Dim hook_stdout As Action(Of String) = Nothing

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="rscript">
    ''' the program path to the rscript host
    ''' </param>
    ''' <param name="languageserver">
    ''' the script path to make invoke for start of the language server backend
    ''' </param>
    ''' <param name="vscode">
    ''' the file path to the .net assembly of the vscode web ui
    ''' </param>
    Sub New(rscript As String, languageserver As String, vscode As String)
        Me.rscript = rscript.GetFullPath
        Me.languageserver = languageserver.GetFullPath
        Me.vscode = vscode.GetFullPath
        Me.rstudio = rscript.ParentPath.ParentPath
    End Sub

    Public Function RedirectStdOut(dev As Action(Of String)) As lspclient
        hook_stdout = dev
        Return Me
    End Function

    Public Function Launch() As Integer
        Dim port As Integer = IPCSocket.GetFirstAvailablePort(delay:=0)
        Dim background As String = $"{languageserver.CLIPath} --port {port} --vscode {vscode.CLIPath}"

        Call New Thread(Sub() Call launch(rscript, background, rstudio)).Start()

        Return port
    End Function

    Public Shared Sub LaunchLanguageServer(rscript As String,
                                           languageserver As String,
                                           vscode As String,
                                           Optional stdout As Action(Of String) = Nothing)

        _lsp_server = New lspclient(rscript, languageserver, vscode:=vscode) _
            .RedirectStdOut(stdout) _
            .Launch()
    End Sub

    Private Sub ProcessMessage(line As String)
        If Not hook_stdout Is Nothing Then
            Call hook_stdout(line)
        End If
    End Sub

    Private Sub launch(rscript As String, arguments As String, workdir As String)
        Using log As New LogFile($"{App.ProductProgramData}/vscode.log")
            Call log.WriteLine($"{rscript} {arguments}", "launch_vscode_languageserver", MSG_TYPES.DEBUG)
        End Using

        Call PipelineProcess.ExecSub(
            app:=rscript,
            args:=arguments,
            onReadLine:=AddressOf ProcessMessage,
            workdir:=workdir,
            shell:=False,
            setProcess:=Sub(p) process = p,
            [in]:=Nothing
        )
    End Sub
End Class
