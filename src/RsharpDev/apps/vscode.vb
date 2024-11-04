Imports RDev

Public NotInheritable Class vscode

    Private Sub New()
    End Sub

    Public Shared Sub Launch()
        Dim rstudio As String = App.HOME & "/rstudio"
        Dim rscript As String = $"{rstudio}/bin/Rscript.exe"
        Dim languageserver As String = $"{rstudio}/R/languageserver.R"
        Dim clr As String = $"{rstudio}/shares/vscode.dll"

        Call lspclient.LaunchLanguageServer(
            rscript, languageserver,
            vscode:=clr,
            stdout:=AddressOf VisualStudio.Output.LogLanguageServer)
    End Sub

End Class
