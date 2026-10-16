Imports Galaxy.Workbench
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualStudio.WinForms.Docking
Imports RibbonLib
Imports RibbonLib.Interop
Imports RsharpDev.RibbonLib.Controls

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
        AddHandler ribbon.ButtonOpenCVS.ExecuteEvent, Sub() Call CommonRuntime.ShowSingleDocument(Of RsharpCVS)()

        AddHandler ribbon.ButtonSave.ExecuteEvent, Sub() Call SaveCurrent()
        AddHandler ribbon.ButtonSaveAs.ExecuteEvent, Sub() Call SaveAs()
    End Sub

    Private Sub SaveCurrent()
        Dim doc As DockContent = CommonRuntime.AppHost.ActiveDocument
        Dim docType As Type = doc?.GetType

        If docType Is Nothing Then
            Call CommonRuntime.Warning("No document is opened.")
            Return
        End If

        If docType.ImplementInterface(Of IFileReference) AndAlso docType.ImplementInterface(Of ISaveHandle) Then
            Dim file As String = DirectCast(doc, IFileReference).FilePath

            If file.StringEmpty(, True) Then
                Using fileDlg As New SaveFileDialog
                    If fileDlg.ShowDialog = DialogResult.OK Then
                        Call DirectCast(doc, ISaveHandle).Save(fileDlg.FileName)
                    End If
                End Using
            Else
                Call DirectCast(doc, ISaveHandle).Save(file)
            End If
        Else
            Call CommonRuntime.Warning($"Current document page [{doc.TabText}] can not be saved.")
        End If
    End Sub

    Private Sub SaveAs()
        Dim doc As DockContent = CommonRuntime.AppHost.ActiveDocument
        Dim docType As Type = doc?.GetType

        If docType Is Nothing Then
            Call CommonRuntime.Warning("No document is opened.")
            Return
        End If

        If docType.ImplementInterface(Of IFileReference) AndAlso docType.ImplementInterface(Of ISaveHandle) Then
            Dim file As String = DirectCast(doc, IFileReference).FilePath
            Dim filter As String = "Save Script File(*.R)|*.R"

            If Not file.StringEmpty(, True) Then
                filter = $"File(*.{file.ExtensionSuffix})|*.{file.ExtensionSuffix}"
            End If

            Using fileDlg As New SaveFileDialog With {.Filter = filter}
                If fileDlg.ShowDialog = DialogResult.OK Then
                    Call DirectCast(doc, ISaveHandle).Save(fileDlg.FileName)
                End If
            End Using
        Else
            Call CommonRuntime.Warning($"Current document page [{doc.TabText}] can not be saved.")
        End If
    End Sub

    Private Sub showAboutSplash()
        Call New SplashScreen() With {.AutoClose = True}.ShowDialog()
    End Sub
End Module
