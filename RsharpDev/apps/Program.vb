Imports SMRUCC.Rsharp.Interpreter

Friend NotInheritable Class Program

    Public Shared ReadOnly Property REngine As New RInterpreter

    ''' <summary>
    ''' The main entry point for the application.
    ''' </summary>
    Private Sub New()
    End Sub

    <STAThread>
    Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New RsharpDevMain())
    End Sub
End Class
