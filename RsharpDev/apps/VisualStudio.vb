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

    Public Sub OpenScript()
        Using file As New OpenFileDialog With {
            .Filter = "R# script(*.R)|*.R",
            .Title = "Open a R# Script File"
        }
            If file.ShowDialog = DialogResult.OK Then
                Dim editor As New RsharpDevEditor

                Call AddDocument(editor)
                Call editor.LoadScript(file.FileName)
            End If
        End Using
    End Sub

    Public Sub AddDocument(doc As DockContent)
        doc.Show(MyApplication.RStudio.DockPanel1)
        doc.DockState = DockState.Document
    End Sub
End Module
