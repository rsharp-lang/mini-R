Imports Galaxy.Workbench
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualStudio.WinForms.Docking
Imports RibbonLib
Imports RibbonLib.Interop

Module RibbonMenu

    Dim ribbon As RibbonItems

    Public ReadOnly Property Menu As RibbonItems
        Get
            Return ribbon
        End Get
    End Property

    Public Sub Hook(ui As Ribbon)
        ribbon = New RibbonItems(ui)
        ribbon.SoluationTabGroup.ContextAvailable = ContextAvailability.NotAvailable
        ribbon.SoluationTabGroup.Label = "Solution [RsharpDev]"
        ribbon.SolutionTab.Label = "Solution [RsharpDev]"

        AddHandler ribbon.ButtonNew.ExecuteEvent, Sub() Call VisualStudio.AddDocument(New RsharpDevVscode)
        AddHandler ribbon.License.ExecuteEvent, Sub() Call New RsharpDevAbout().ShowDialog()
        AddHandler ribbon.About.ExecuteEvent, Sub() Call showAboutSplash()
        AddHandler ribbon.ButtonOpen.ExecuteEvent, Sub() Call VisualStudio.OpenFile()
        AddHandler ribbon.ConfigServer.ExecuteEvent, Sub() VisualStudio.LinuxServerList.DockState = DockState.DockLeft
        AddHandler ribbon.Console.ExecuteEvent, Sub() VisualStudio.AddDocument(SingletonHolder(Of RsharpDevConsole).Instance)
        AddHandler ribbon.ViewProperty.ExecuteEvent, Sub() Call VisualStudio.OpenSolution()
        AddHandler ribbon.Config.ExecuteEvent, Sub() Call New ConfigApp().ShowDialog()
        AddHandler ribbon.StartPage.ExecuteEvent, Sub() Call VisualStudio.AddDocument(SingletonHolder(Of StartPage).Instance)
        AddHandler ribbon.Close.ExecuteEvent, Sub() Call DirectCast(CommonRuntime.AppHost, Form).Close()
    End Sub

    Private Sub showAboutSplash()
        Call New SplashScreen() With {.AutoClose = True}.ShowDialog()
    End Sub
End Module
