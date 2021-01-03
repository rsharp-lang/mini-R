Imports System.Runtime.CompilerServices
Imports Config
Imports Microsoft.VisualBasic.My
Imports My
Imports WeifenLuo.WinFormsUI.Docking

Module VisualStudio

    Public ReadOnly Property SolutionView As New ToolWinSolution
    Public ReadOnly Property LinuxServerList As New ToolWinServers
    Public ReadOnly Property Output As New ToolOutput

    Public Const FolderClose As Integer = 2

    Friend WithEvents VS2003Theme1 As New VS2003Theme
    Friend WithEvents VS2005Theme1 As New VS2005Theme
    Friend WithEvents VS2012BlueTheme1 As New VS2012BlueTheme
    Friend WithEvents VS2012DarkTheme1 As New VS2012DarkTheme
    Friend WithEvents VS2012LightTheme1 As New VS2012LightTheme
    Friend WithEvents VS2013BlueTheme1 As New VS2013BlueTheme
    Friend WithEvents VS2013DarkTheme1 As New VS2013DarkTheme
    Friend WithEvents VS2013LightTheme1 As New VS2013LightTheme
    Friend WithEvents VS2015BlueTheme1 As New VS2015BlueTheme
    Friend WithEvents VS2015DarkTheme1 As New VS2015DarkTheme
    Friend WithEvents VS2015LightTheme1 As New VS2015LightTheme

    Friend ReadOnly vsWindow As New List(Of IApplyVsTheme)

    Sub InitializeUI()
        SolutionView.Show(MyApplication.RStudio.DockPanel1)
        SolutionView.DockState = DockState.DockRightAutoHide

        LinuxServerList.Show(MyApplication.RStudio.DockPanel1)
        LinuxServerList.DockState = DockState.DockLeftAutoHide

        Output.Show(MyApplication.RStudio.DockPanel1)
        Output.DockState = DockState.DockBottomAutoHide
    End Sub

    Public Sub OpenSolution()
        If Not Program.Solution Is Nothing Then
            Call VisualStudio.AddDocument(SingletonHolder(Of PackageConfiguration).Instance)
            Call SingletonHolder(Of PackageConfiguration).Instance.LoadSolution()
        End If
    End Sub

    Public Function ConfigRemote() As (plink$, pscp$)
        Dim dir As String = App.ProductSharedDIR & "/putty"

        If $"{dir}/plink.exe".FileLength <> My.Resources.plink.Length Then
            Call My.Resources.plink.FlushStream($"{dir}/plink.exe")
        End If
        If $"{dir}/pscp.exe".FileLength <> My.Resources.pscp.Length Then
            Call My.Resources.pscp.FlushStream($"{dir}/pscp.exe")
        End If

        Return ($"{dir}/plink.exe", $"{dir}/pscp.exe")
    End Function

    Public Sub OpenFile()
        Using file As New OpenFileDialog With {
            .Filter = "R# Package Project(*.Rproj)|*.Rproj|R# script(*.R)|*.R",
            .Title = "Open a R# package project or Script File"
        }
            If file.ShowDialog = DialogResult.OK Then
                If file.FileName.ExtensionSuffix("Rproj") Then
                    Call Program.LoadSolution(Rproj:=file.FileName)
                Else
                    Call AddDocument(New RsharpDevEditor, Sub(c) DirectCast(c, RsharpDevEditor).View(file.FileName))
                End If
            End If
        End Using
    End Sub

    Public Sub AddDocument(doc As DockContent, Optional after As Action(Of DockContent) = Nothing)
        doc.Show(MyApplication.RStudio.DockPanel1)
        doc.DockState = DockState.Document

        If Not after Is Nothing Then
            Call after(doc)
        End If
    End Sub

    <Extension>
    Public Sub ApplyVsTheme(extender As VisualStudioToolStripExtender, ParamArray controls As ToolStrip())
        Dim version = Program.Config.theme
        Dim theme As ThemeBase = Nothing

        Select Case Program.Config.theme
            Case VisualStudioToolStripExtender.VsVersion.Vs2003
                theme = VS2003Theme1
            Case VisualStudioToolStripExtender.VsVersion.Vs2005
                theme = VS2005Theme1
            Case VisualStudioToolStripExtender.VsVersion.Vs2012
                Select Case Program.Config.type
                    Case Config.ThemeType.Blue
                        theme = VS2012BlueTheme1
                    Case Config.ThemeType.Dark
                        theme = VS2012DarkTheme1
                    Case Config.ThemeType.Light
                        theme = VS2012LightTheme1
                End Select

            Case VisualStudioToolStripExtender.VsVersion.Vs2013
                Select Case Program.Config.type
                    Case Config.ThemeType.Blue
                        theme = VS2013BlueTheme1
                    Case Config.ThemeType.Dark
                        theme = VS2013DarkTheme1
                    Case Config.ThemeType.Light
                        theme = VS2013LightTheme1
                End Select

            Case VisualStudioToolStripExtender.VsVersion.Vs2015
                Select Case Program.Config.type
                    Case Config.ThemeType.Blue
                        theme = VS2015BlueTheme1
                    Case Config.ThemeType.Dark
                        theme = VS2015DarkTheme1
                    Case Config.ThemeType.Light
                        theme = VS2015LightTheme1
                End Select

        End Select

        For Each toolstrip As ToolStrip In controls
            Call extender.SetStyle(toolstrip, version, theme)
        Next
    End Sub
End Module
