Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Form1

    Dim features As New List(Of SampleDistribution)

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {.Filter = "any file(*.*)|*.*"}
            If file.ShowDialog = DialogResult.OK Then
                Dim samples As SampleData() = SampleData _
                    .Load(file.FileName.Open(FileMode.Open, doClear:=False, [readOnly]:=True)) _
                    .ToArray
                Dim d As SampleDistribution
                Dim v As Double()

                For Each sample As SampleData In samples
                    CheckedListBox1.Items.Add(sample)
                Next

                For i As Integer = 0 To samples(0).features.Length - 1
                    v = samples.Select(Function(si) si.features(i)).ToArray
                    d = New SampleDistribution(v, estimateQuantile:=False)

                    Call features.Add(d)
                Next

                Text = $"[{file.FileName}]"
            End If
        End Using
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        Dim i As Integer = ListBox1.SelectedIndex

        If i < 0 Then
            Return
        End If

        Dim d As SampleDistribution = features(i)
        Dim sb As New StringBuilder

        sb.AppendLine($"min: {d.min}")
        sb.AppendLine($"max: {d.max}")
        sb.AppendLine($"mean: {d.average}")
        sb.AppendLine($"std: {d.stdErr}")
        sb.AppendLine($"mode: {d.mode}")
        sb.AppendLine($"CI 95%: {d.CI95Range.GetJson}")

        TextBox1.Clear()
        TextBox1.Text = sb.ToString
    End Sub

    Private Sub CheckedListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CheckedListBox1.SelectedIndexChanged
        If CheckedListBox1.SelectedIndex >= 0 Then
            Dim item As SampleData = CheckedListBox1.Items(CheckedListBox1.SelectedIndex)

            ListBox1.Items.Clear()
            ListBox2.Items.Clear()

            For Each x As Double In item.features
                ListBox1.Items.Add(x)
            Next
            ' test data has no label
            For Each y As Double In item.labels.SafeQuery
                ListBox2.Items.Add(y)
            Next
        End If
    End Sub

    Private Sub ExportJSONToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportJSONToolStripMenuItem.Click
        Dim data As New List(Of SampleData)

        For Each i As Integer In CheckedListBox1.CheckedIndices
            data.Add(CheckedListBox1.Items(i))
        Next

        If data.Count = 0 Then
            MessageBox.Show("No data selected!")
        Else
            Using file As New SaveFileDialog With {.Filter = "json data(*.json)|*.json"}
                If file.ShowDialog = DialogResult.OK Then
                    Call data.ToArray.GetJson.SaveTo(file.FileName)
                End If
            End Using
        End If
    End Sub

    Private Sub ComparesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ComparesToolStripMenuItem.Click
        Dim clist = CheckedListBox1.CheckedIndices.ToArray(Of Integer)

        If clist.Length < 2 Then
            MessageBox.Show("You must select 2 data sample at least!")
        Else
            Dim viewer As New Form2
            Dim memoryData As New System.Data.DataSet
            Dim df As DataTable = memoryData.Tables.Add("memoryData")
            Dim sampleSet As New List(Of SampleData)
            Dim r As DataRow

            For Each i As Integer In clist
                sampleSet.Add(CheckedListBox1.Items(i))
                df.Columns.Add(sampleSet.Last.id, GetType(Double))
            Next

            For i As Integer = 0 To sampleSet(0).features.Length - 1
                r = df.Rows.Add()

                For j As Integer = 0 To sampleSet.Count - 1
                    r.Item(j) = sampleSet(j).features(i)
                Next
            Next

            For i As Integer = 0 To sampleSet(0).labels.SafeQuery.Count - 1
                r = df.Rows.Add

                For j As Integer = 0 To sampleSet.Count - 1
                    r.Item(j) = sampleSet(j).labels(i)
                Next
            Next

            viewer.DataGridView1.DataSource = memoryData
            viewer.DataGridView1.DataMember = df.TableName
            viewer.ShowDialog()
        End If
    End Sub
End Class
