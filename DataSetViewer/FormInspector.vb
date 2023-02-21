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
        Try
            Dim value As Object = R.Evaluate($"readRDS('{path}');")

            If value Is Nothing Then
                MessageBox.Show("No data could be loaded!", "readRDS", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                Call LoadData(value)
            End If
        Catch ex As Exception
            MessageBox.Show("Invalid data file format!", "readRDS", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadData(x As Object)
        Dim type As Type = x.GetType

        Select Case type
            Case GetType(vector)
            Case GetType(list)
            Case GetType(dataframe)
        End Select
    End Sub

    Private Sub FormInspector_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call LoadFile("F:\Metlin.cache")
    End Sub
End Class
