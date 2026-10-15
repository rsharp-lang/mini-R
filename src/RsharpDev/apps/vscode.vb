Imports Galaxy.Workbench
Imports RDev

Public NotInheritable Class vscode

    Private Sub New()
    End Sub

    Public Shared Sub Launch()
        Dim rstudio As String = App.HOME & "/rstudio"
        Dim rscript As String = $"{rstudio}/bin/Rscript.exe"
        Dim languageserver As String = $"{rstudio}/R/languageserver.R"
        Dim clr As String = $"{rstudio}/shares/vscode.dll"
        Dim output = CommonRuntime.GetOutputWindow

        Call lspclient.LaunchLanguageServer(
            rscript, languageserver,
            vscode:=clr,
            stdout:=Sub(line)
                        Call output.AppendTo("lsp", line)
                    End Sub)
    End Sub

End Class
