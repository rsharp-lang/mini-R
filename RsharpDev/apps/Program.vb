Imports Config
Imports Microsoft.VisualBasic.Linq
Imports My
Imports RDev
Imports RibbonLib.Interop
Imports SMRUCC.Rsharp.Interpreter
Imports WeifenLuo.WinFormsUI.Docking

Friend NotInheritable Class Program

    Public Shared ReadOnly Property REngine As New RInterpreter
    Public Shared ReadOnly Property Solution As Solution
    Public Shared ReadOnly Property Config As ConfigFile

    Shared Sub New()
        Call RDev.DescriptionTooltip.SetEngine(REngine)
    End Sub

    ''' <summary>
    ''' save config
    ''' </summary>
    Public Shared Sub Save()
        Call Config.GetXml.SaveTo(ConfigFile.FileLocation)
    End Sub

    Public Shared Sub LoadSolution(Rproj As String)
        Dim ribbon = MyApplication.RStudio.ribbon

        _Solution = Solution.LoadRproj(Rproj)

        VisualStudio.SolutionView.TreeView1.Nodes(Scan0).Text = $"Solution '{Solution.LoadInformation.Package}'"
        VisualStudio.SolutionView.DockState = DockState.DockRight

        ribbon.SoluationTabGroup.ContextAvailable = ContextAvailability.Active
        ribbon.SoluationTabGroup.Label = $"Solution [{Solution.LoadInformation.Package}]"
        ribbon.SolutionTab.Label = ribbon.SoluationTabGroup.Label

        MyApplication.RStudio.Text = $"R# Develop Studio [{Rproj}]"

        VisualStudio.SolutionView.TreeView1.Nodes(Scan0).Nodes.Clear()
        VisualStudio.SolutionView.TreeView1.Nodes(Scan0).DoCall(Sub(root) listFolder(root, Rproj.ParentPath))
    End Sub

    Private Shared Sub listFolder(explorer As TreeNode, folder As String)
        For Each dir As String In folder.ListDirectory
            Dim dirNode As New TreeNode With {
                .Text = dir.BaseName,
                .ImageIndex = VisualStudio.FolderClose,
                .SelectedImageIndex = .ImageIndex
            }

            explorer.Nodes.Add(dirNode)
            listFolder(dirNode, dir)
        Next

        For Each file As String In folder.EnumerateFiles
            Dim fileName As String = file.FileName
            Dim fileNode As New TreeNode With {.Text = fileName, .Tag = file}

            If fileName = "DESCRIPTION" Then
                fileNode.ImageIndex = 6
            ElseIf fileName = "LICENSE" Then
                fileNode.ImageIndex = 7
            ElseIf fileName.ExtensionSuffix("R") Then
                fileNode.ImageIndex = 4

                For Each item In DescriptionTooltip.GetSymbols(file.ReadAllText)
                    With fileNode.Nodes.Add(item.Name)
                        If item.Value = "symbol" Then
                            .ImageIndex = 12
                        Else
                            .ImageIndex = 11
                        End If

                        .SelectedImageIndex = .ImageIndex
                    End With
                Next
            ElseIf fileName.ExtensionSuffix("csv", "xls", "xlsx") Then
                fileNode.ImageIndex = 8
            ElseIf fileName.ExtensionSuffix("xml", "html", "htm") Then
                fileNode.ImageIndex = 9
            ElseIf fileName.ExtensionSuffix("dll") Then
                fileNode.ImageIndex = 10
            ElseIf fileName.ExtensionSuffix("png", "jpg", "bitmap", "gif") Then
                fileNode.ImageIndex = 14
            ElseIf fileName.ExtensionSuffix("sh", "bat", "cmd") Then
                fileNode.ImageIndex = 13
            Else
                fileNode.ImageIndex = 1
            End If

            fileNode.SelectedImageIndex = fileNode.ImageIndex
            explorer.Nodes.Add(fileNode)
        Next
    End Sub

    Public Shared Sub Initialize()
        If ConfigFile.FileLocation.FileExists Then
            _Config = ConfigFile.FileLocation.LoadXml(Of ConfigFile)(throwEx:=False)
        End If

        If Config Is Nothing Then
            _Config = New ConfigFile
        End If

        If Config.theme = VisualStudioToolStripExtender.VsVersion.Unknown Then
            Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2015
            Config.type = ThemeType.Light
        End If
    End Sub

End Class
