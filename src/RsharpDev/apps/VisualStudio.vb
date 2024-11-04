Imports System.Runtime.CompilerServices
Imports Config
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.My
Imports My
Imports WeifenLuo.WinFormsUI.Docking

Module VisualStudio

    Public ReadOnly Property SolutionView As ToolWinSolution = SingletonHolder(Of ToolWinSolution).Instance
    Public ReadOnly Property LinuxServerList As ToolWinServers = SingletonHolder(Of ToolWinServers).Instance
    Public ReadOnly Property Output As ToolOutput = SingletonHolder(Of ToolOutput).Instance

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

    Friend RefreshRecentList As Action

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

    Public Sub OpenFile()
        Using file As New OpenFileDialog With {
            .Filter = "R# Package Project(*.Rproj)|*.Rproj|R# script(*.R)|*.R",
            .Title = "Open a R# package project or Script File"
        }
            If file.ShowDialog = DialogResult.OK Then
                Call OpenFile(file.FileName)
            End If
        End Using
    End Sub

    Public Sub OpenFile(fileName As String)
        If fileName.ExtensionSuffix("Rproj") Then
            Call Program.LoadSolution(Rproj:=fileName)
        Else
            Call AddDocument(New RsharpDevVscode, fileName, Sub(c) DirectCast(c, RsharpDevVscode).View(fileName))
        End If

        Program.Config.AddRecent(fileName)
        Program.Save()

        Call VisualStudio.RefreshRecentList()
    End Sub

    Public Sub AddDocument(doc As DockContent, Optional file As String = Nothing, Optional after As Action(Of DockContent) = Nothing)
        If TypeOf doc Is IFileReference Then
            DirectCast(doc, IFileReference).FilePath = file
        End If

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
