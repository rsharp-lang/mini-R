Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualStudio.WinForms.Docking
Imports RsharpDev.My
Imports ThemeVS2015

Module VisualStudio

    Public ReadOnly Property SolutionView As ToolWinSolution = SingletonHolder(Of ToolWinSolution).Instance
    Public ReadOnly Property LinuxServerList As ToolWinServers = SingletonHolder(Of ToolWinServers).Instance

    Public Const FolderClose As Integer = 2

    Friend WithEvents VS2015BlueTheme1 As New VS2015BlueTheme
    Friend WithEvents VS2015DarkTheme1 As New VS2015DarkTheme
    Friend WithEvents VS2015LightTheme1 As New VS2015LightTheme

    Friend RefreshRecentList As Action

    Sub InitializeUI()
        SolutionView.Show(MyApplication.RStudio.DockPanel1)
        SolutionView.DockState = DockState.DockRightAutoHide

        LinuxServerList.Show(MyApplication.RStudio.DockPanel1)
        LinuxServerList.DockState = DockState.DockLeftAutoHide
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
