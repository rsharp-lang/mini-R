Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Public Class FormInspector

    ReadOnly R As RInterpreter = RInterpreter.Rsharp

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {.Filter = "Any kinds(*.*)|*.*"}
            If file.ShowDialog = DialogResult.OK Then
                Text = $"Inspect[{file.FileName}]"
                LoadFile(file.FileName)
            End If
        End Using
    End Sub

    Private Sub LoadFile(path As String)
        Call TreeView1.Nodes.Clear()

        Try
            Dim value As Object = R.Evaluate($"readRDS('{path}');")
            Dim root = TreeView1.Nodes.Add(path.FileName)

            If value Is Nothing Then
                MessageBox.Show("No data could be loaded!", "readRDS", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                Call LoadData(value, root)
            End If
        Catch ex As Exception
            MessageBox.Show("Invalid data file format!", "readRDS", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadData(x As Object, tree As TreeNode)
        Dim type As Type = x.GetType

        Select Case type
            Case GetType(vector)
            Case GetType(list)
                Dim listSet As list = DirectCast(x, list)

                ' is a folder
                For Each name As String In listSet.getNames
                    Dim data = listSet.getByName(name)
                    Dim node = tree.Nodes.Add(name)

                    node.ImageIndex = If(TypeOf data Is list OrElse TypeOf data Is dataframe, Icons.Folder, Icons.xFile)
                    node.SelectedImageIndex = node.ImageIndex
                    node.Tag = data
                Next
            Case GetType(dataframe)
        End Select
    End Sub

    Private Sub FormInspector_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call LoadFile("F:\Metlin.cache")
    End Sub
End Class

Public Enum Icons
    xFile = 0
    Folder = 1
End Enum