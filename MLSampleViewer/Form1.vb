Imports System.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

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
End Class
