Imports System.Threading
Imports Darwinism.HPC.Parallel
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline

Public NotInheritable Class vscode

    Private Sub New()
    End Sub

    Shared background As RunSlavePipeline
    Shared process As Process

    Public Shared Function Launch() As Integer
        Dim port As Integer = IPCSocket.GetFirstAvailablePort(delay:=0)
        Dim rstudio As String = App.HOME & "/rstudio"
        Dim rscript As String = $"{rstudio}/bin/Rscript.exe".GetFullPath
        Dim languageserver As String = $"{rstudio}/R/languageserver.R".GetFullPath
        Dim clr As String = $"{rstudio}/shares/vscode.dll".GetFullPath
        Dim background As String = $"{languageserver.CLIPath} --port {port} --vscode {clr.CLIPath}"

        Call New Thread(Sub() Call launch(rscript, background, rstudio)).Start()

        Return port
    End Function

    Private Shared Sub ProcessMessage(line As String)
        Call VisualStudio.Output.LogLanguageServer(line)
    End Sub

    Private Shared Sub launch(rscript As String, arguments As String, workdir As String)
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
