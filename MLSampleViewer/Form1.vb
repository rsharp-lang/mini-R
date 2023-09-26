Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Form1
    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {.Filter = "any file(*.*)|*.*"}
            If file.ShowDialog = DialogResult.OK Then
                Dim samples As SampleData() = SampleData _
                    .Load(file.FileName.Open(FileMode.Open, doClear:=False, [readOnly]:=True)) _
                    .ToArray

                For Each sample As SampleData In samples
                    CheckedListBox1.Items.Add(sample)
                Next
            End If
        End Using
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged

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
        Dim check1, check2 As SampleData
        Dim clist = CheckedListBox1.CheckedIndices.ToArray(Of Integer)

        If clist.Length < 2 Then
            MessageBox.Show("You must select 2 data sample at least!")
        Else
            check1 = CheckedListBox1.Items(clist(0))
            check2 = CheckedListBox1.Items(clist(1))

            ' draw compares
            Dim a, b As List(Of (x#, value#))
            Dim offset = check1.features.Length + 2

            a = New List(Of (x As Double, value As Double))
            b = New List(Of (x As Double, value As Double))

            For i As Integer = 0 To check1.features.Length - 1
                a.Add((CDbl(i + 1), check1.features(i)))
                b.Add((CDbl(i + 1), check2.features(i)))
            Next
            For i As Integer = 0 To check1.labels.SafeQuery.Count - 1
                a.Add((CDbl(i + offset), check1.labels(i)))
                b.Add((CDbl(i + offset), check2.labels(i)))
            Next

            Dim img As Image = a.ToArray.PlotAlignment(b.ToArray).AsGDIImage
            Dim temp As String = TempFileSystem.GetAppSysTempFile(".png")

            Call Process.Start(temp)
        End If
    End Sub
End Class
