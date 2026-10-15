Imports Galaxy.Workbench
Imports RDev

Public NotInheritable Class vscode

    Private Sub New()
    End Sub

    Public Shared Async Function Launch() As Task
        Dim rstudio As String = App.HOME & "/rstudio"
        Dim rscript As String = $"{rstudio}/bin/Rscript.exe"
        Dim languageserver As String = $"{rstudio}/R/languageserver.R"
        Dim clr As String = $"{rstudio}/shares/vscode.dll"
        Dim output = CommonRuntime.GetOutputWindow

        Await Task.Delay(1)

        Call lspclient.LaunchLanguageServer(
            rscript, languageserver,
            vscode:=clr,
            stdout:=Sub(line)
                        Call output.AppendTo("lsp", line)
                    End Sub)
    End Function

End Class
