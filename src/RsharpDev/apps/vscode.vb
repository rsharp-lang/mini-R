Imports RDev

Public NotInheritable Class vscode

    Private Sub New()
    End Sub

    Public Shared Function Launch() As Integer
        Dim rstudio As String = App.HOME & "/rstudio"
        Dim rscript As String = $"{rstudio}/bin/Rscript.exe"
        Dim languageserver As String = $"{rstudio}/R/languageserver.R"
        Dim clr As String = $"{rstudio}/shares/vscode.dll"
        Dim client As New lspclient(rscript, languageserver, vscode:=clr)

        Return client _
            .RedirectStdOut(AddressOf VisualStudio.Output.LogLanguageServer) _
            .Launch()
    End Function

End Class
