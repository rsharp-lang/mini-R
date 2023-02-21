Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Public Class FormInspector

    ReadOnly R As RInterpreter = RInterpreter.Rsharp

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {.Filter = "Any kinds(*.*)|*.*"}
            If file.ShowDialog = DialogResult.OK Then
                LoadFile(file.FileName)
            End If
        End Using
    End Sub

    Private Sub LoadFile(path As String)
        TreeView1.Nodes.Clear()
        Text = $"Inspect[{path.FileName}]"

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

    Private Shared Function getName(name As String, x As Object) As String
        If x Is Nothing Then
            Return "null"
        ElseIf TypeOf x Is list Then
            Return $"{name} [{DirectCast(x, list).length} elements]"
        ElseIf TypeOf x Is dataframe Then
            Return $"{name} [{DirectCast(x, dataframe).nrows}x{DirectCast(x, dataframe).ncols}]"
        Else
            Return name
        End If
    End Function

    Private Sub LoadData(x As Object, tree As TreeNode)
        Dim type As Type = x.GetType

        Select Case type
            Case GetType(list)
                Dim listSet As list = DirectCast(x, list)

                ' is a folder
                For Each name As String In listSet.getNames
                    Dim data = listSet.getByName(name)
                    Dim node = tree.Nodes.Add(getName(name, data))

                    node.ImageIndex = If(TypeOf data Is list OrElse TypeOf data Is dataframe, Icons.Folder, Icons.xFile)
                    node.SelectedImageIndex = node.ImageIndex
                    node.Tag = data
                Next

                Call tree.Expand()
            Case GetType(dataframe)
                Dim df As dataframe = DirectCast(x, dataframe)

                ' is a folder of vectors
                For Each name As String In df.colnames
                    Dim data As Array = df(name)
                    Dim node = tree.Nodes.Add($"{name}<{data.Length}>")

                    node.ImageIndex = Icons.xFile
                    node.SelectedImageIndex = Icons.xFile
                    node.Tag = data
                Next

                Call tree.Expand()
        End Select
    End Sub

    Private Sub FormInspector_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call LoadFile("F:\Metlin.cache")
    End Sub

    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect

    End Sub

    Private Sub ViewValue(value As Object, tree As TreeNode)
        If value Is Nothing Then
            ' do nothing 
            TextBox1.Text = ""
        ElseIf value.GetType.IsArray Then
            Dim array As Array = REnv.TryCastGenericArray(DirectCast(value, Array), R.globalEnvir)
            Dim type As Type = array.GetType.GetElementType

            If type Is Nothing OrElse Not DataFramework.IsPrimitive(type) Then
                ' load tree
                tree.ImageIndex = Icons.Folder
                tree.SelectedImageIndex = Icons.Folder

                For i As Integer = 0 To array.Length - 1
                    Call LoadData(array.GetValue(i), tree)
                Next

                Call tree.Expand()
            Else
                ' view array 
                TextBox1.Text = DirectCast(value, Array) _
                    .AsObjectEnumerator _
                    .Select(Function(o) any.ToString(o)) _
                    .JoinBy(vbCrLf)
            End If
        ElseIf value.GetType Is GetType(vector) Then
            Call ViewValue(DirectCast(value, vector).data, tree)
        End If
    End Sub

    Private Sub TreeView1_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles TreeView1.NodeMouseDoubleClick
        If e.Node.ImageIndex = Icons.Folder Then
            If e.Node.Nodes.Count = 0 Then
                Call LoadData(e.Node.Tag, e.Node)
            End If
        Else
            Call ViewValue(e.Node.Tag, e.Node)
        End If
    End Sub
End Class

Public Enum Icons
    xFile = 0
    Folder = 1
End Enum