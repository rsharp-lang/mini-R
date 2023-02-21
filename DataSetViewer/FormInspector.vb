Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Public Class FormInspector

    ReadOnly R As RInterpreter = RInterpreter.Rsharp
    ReadOnly viewer As New Dictionary(Of Type, Control)

    Dim df As dataframe
    Dim current As Control

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {.Filter = "Any kinds(*.*)|*.*"}
            If file.ShowDialog = DialogResult.OK Then
                LoadFile(file.FileName)
            End If
        End Using
    End Sub

    Private Overloads Function Show(Of T As Control)() As T
        Dim type As Type = GetType(T)

        For Each item In viewer
            item.Value.Visible = False
            item.Value.Dock = DockStyle.None
        Next

        If viewer.ContainsKey(type) Then
            viewer(type).Visible = True
            viewer(type).Dock = DockStyle.Fill
        End If

        Call viewer.TryGetValue(type, current)

        Return current
    End Function

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

    Private Sub AddViewer(Of T As Control)(v As T)
        viewer.Add(GetType(T), v)
        v.Visible = True
        v.Dock = DockStyle.None
    End Sub

    Private Sub FormInspector_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call AddViewer(TextBox1)
        Call AddViewer(DataGridView1)

        Call LoadFile("F:\Metlin.cache")
    End Sub

    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect

    End Sub

    Private Sub ViewValue(value As Object, tree As TreeNode)
        If value Is Nothing Then
            ' do nothing 
            Show(Of TextBox).Text = ""
        ElseIf value.GetType.IsArray Then
            Dim array As Array = REnv.TryCastGenericArray(DirectCast(value, Array), R.globalEnvir)
            Dim type As Type = array.GetType.GetElementType

            If type Is Nothing OrElse Not DataFramework.IsPrimitive(type) Then
                ' load tree
                tree.ImageIndex = Icons.Folder
                tree.SelectedImageIndex = Icons.Folder

                For i As Integer = 0 To array.Length - 1
                    Call LoadData(array.GetValue(i), tree.Nodes.Add($"[{i + 1}]"))
                Next

                Call tree.Expand()
            Else
                ' view array 
                Show(Of TextBox).Text = DirectCast(value, Array) _
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

    Private Sub ViewAsDataFrameToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ViewAsDataFrameToolStripMenuItem.Click
        Dim node As TreeNode = Nothing

        Try
            node = TreeView1.SelectedNode
        Catch ex As Exception

        End Try

        If Not node Is Nothing Then
            Call ViewAsDataFrame(node.Tag, "as.data.frame(x);")
        End If
    End Sub

    Private Sub ViewAsDataFrame(value As Object, exp As String)
        If value Is Nothing Then
            Return
        ElseIf TypeOf value Is vector Then
            Dim list As New list
            Dim v As Array = DirectCast(value, vector).data

            For i As Integer = 0 To v.Length - 1
                list.add(i + 1, v.GetValue(i))
            Next

            value = list
        End If

        If TypeOf value Is list OrElse TypeOf value Is dataframe Then
            Dim df = R.Evaluate(exp, ("x", value))

            If Program.isException(df) OrElse Not TypeOf df Is dataframe Then
                Return
            Else
                Dim tableData As dataframe = DirectCast(df, dataframe)
                Dim view As DataGridView = Show(Of DataGridView)()

                view.Rows.Clear()
                view.Columns.Clear()

                For Each col In tableData.colnames
                    Call view.Columns.Add(col, col)
                Next

                For Each row In tableData.forEachRow
                    Call view.Rows.Add(row.value)
                Next

                Me.df = df
            End If
        End If
    End Sub

    Private Sub TransposeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TransposeToolStripMenuItem.Click
        Dim node As TreeNode = Nothing

        Try
            node = TreeView1.SelectedNode
        Catch ex As Exception

        End Try

        If Not node Is Nothing Then
            Call ViewAsDataFrame(node.Tag, "t(as.data.frame(x));")
        End If
    End Sub

    Private Sub ExportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem.Click
        If TypeOf current Is DataGridView Then
            Using file As New SaveFileDialog With {.Filter = "Excel Table(*.csv)|*.csv"}
                If file.ShowDialog = DialogResult.OK Then
                    Call R.Evaluate($"write.csv(x, file = '{file.FileName}');", ("x", Me.df))
                End If
            End Using
        ElseIf TypeOf current Is TextBox Then
            Using file As New SaveFileDialog With {.Filter = "Plain Text(*.txt)|*.txt"}
                If file.ShowDialog = DialogResult.OK Then
                    Call R.Evaluate($"writeLines(x, con = '{file.FileName}');", ("x", TextBox1.Text))
                End If
            End Using
        Else

        End If
    End Sub
End Class

Public Enum Icons
    xFile = 0
    Folder = 1
End Enum