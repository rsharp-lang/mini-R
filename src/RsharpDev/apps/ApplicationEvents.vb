Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
    ' The following events are available for MyApplication:
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Public Shared ReadOnly Property RStudio As RsharpDevMain

        Public Shared ReadOnly Property lsp_server As Integer = 321

        Public Shared Sub Register(rstudio As RsharpDevMain)
            MyApplication._RStudio = rstudio
            ' VisualStudio.ConfigRemote()
        End Sub

        Public Shared Async Function LaunchLanguageServer() As Threading.Tasks.Task
            Await Task.Run(Sub() _lsp_server = vscode.Launch)
        End Function

        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException

        End Sub

        Private Sub MyApplication_Shutdown(sender As Object, e As EventArgs) Handles Me.Shutdown
            Call Program.Save()
        End Sub
    End Class
End Namespace
