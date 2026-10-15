Public Class RsharpDevConsole

    Private Sub RsharpDevConsole_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = "R# Console"
        WebViewConsole1.StartProcess("cmd.exe", "")
    End Sub
End Class