Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text
Imports WeifenLuo.WinFormsUI.Docking

Public Class ViewDataFrame : Implements Viewer

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Function View(file As String) As DockContent Implements Viewer.View
        Dim table As DataFrame = DataFrame.Load(file).MeasureTypeSchema
        Dim col As DataGridViewTextBoxColumn

        For Each name As String In table.HeadTitles
            col = New DataGridViewTextBoxColumn With {
                .HeaderText = name.Trim(""""c, " "c),
                .[ReadOnly] = True
            }

            If table.GetFieldType(table.GetOrdinal(name)) Is GetType(Double) Then
                col.DefaultCellStyle.Format = "G3"
            End If

            DataGridView1.Columns.Add(col)
        Next

        For Each row As Object() In table.EnumerateRowObjects
            Call DataGridView1.Rows.Add(row)
        Next

        Text = file.FileName

        Return Me
    End Function

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function
End Class