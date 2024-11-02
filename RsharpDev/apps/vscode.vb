Imports Darwinism.HPC.Parallel
Imports Microsoft.VisualBasic.ApplicationServices

Public NotInheritable Class vscode

    Private Sub New()
    End Sub

    Public Shared Function Launch() As Integer
        Dim port As Integer = IPCSocket.GetFirstAvailablePort(delay:=0)
        Dim rstudio As String = App.HOME & "/rstudio"
        Dim rscript As String = $"{rstudio}/bin/Rscript.exe".GetFullPath
        Dim languageserver As String = $"{rstudio}/R/languageserver.R".GetFullPath
        Dim clr As String = $"{rstudio}/shares/vscode.dll".GetFullPath
        Dim background As String = $"{rscript.CLIPath} {rscript.CLIPath} --port {port} --vscode {clr.CLIPath}"

    End Function

End Class
