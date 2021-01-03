Imports System.Runtime.CompilerServices
Imports My
Imports WeifenLuo.WinFormsUI.Docking

Module VisualStudio

    Public ReadOnly Property SolutionView As New ToolWinSolution
    Public ReadOnly Property LinuxServerList As New ToolWinServers

    Sub InitializeUI()
        SolutionView.Show(MyApplication.RStudio.DockPanel1)
        SolutionView.DockState = DockState.DockRightAutoHide

        LinuxServerList.Show(MyApplication.RStudio.DockPanel1)
        LinuxServerList.DockState = DockState.DockLeftAutoHide
    End Sub

    Public Sub OpenFile()
        Using file As New OpenFileDialog With {
            .Filter = "R# Package Project(*.Rproj)|*.Rproj|R# script(*.R)|*.R",
            .Title = "Open a R# package project or Script File"
        }
            If file.ShowDialog = DialogResult.OK Then
                If file.FileName.ExtensionSuffix("Rproj") Then
                    Call Program.LoadSolution(Rproj:=file.FileName)
                Else
                    Dim editor As New RsharpDevEditor

                    Call AddDocument(editor)
                    Call editor.LoadScript(file.FileName)
                End If
            End If
        End Using
    End Sub

    Public Sub AddDocument(doc As DockContent)
        doc.Show(MyApplication.RStudio.DockPanel1)
        doc.DockState = DockState.Document
    End Sub

    <Extension>
    Public Sub ApplyVsTheme(extender As VisualStudioToolStripExtender,
                            version As VisualStudioToolStripExtender.VsVersion,
                            theme As ThemeBase,
                            ParamArray controls As ToolStrip())

        For Each toolstrip As ToolStrip In controls
            Call extender.SetStyle(toolstrip, version, theme)
        Next
    End Sub
End Module
