Imports RDev
Imports SMRUCC.Rsharp.Interpreter
Imports WeifenLuo.WinFormsUI.Docking

Friend NotInheritable Class Program

    Public Shared ReadOnly Property REngine As New RInterpreter
    Public Shared ReadOnly Property Solution As Solution

    Shared Sub New()
        Call RDev.DescriptionTooltip.SetEngine(REngine)
    End Sub

    Public Shared Sub LoadSolution(Rproj As String)
        _Solution = Solution.LoadRproj(Rproj)

        VisualStudio.SolutionView.TreeView1.Nodes(Scan0).Text = $"Solution '{Solution.LoadInformation.Package}'"
        VisualStudio.SolutionView.DockState = DockState.DockRight
    End Sub

    Public Shared Sub Initialize()

    End Sub

End Class
