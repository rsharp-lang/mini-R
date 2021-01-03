Public Class AddLinuxServer

    ''' <summary>
    ''' OK
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If addConfig() Then
            Call Me.Close()
        End If
    End Sub

    Private Function addConfig() As Boolean

    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub
End Class