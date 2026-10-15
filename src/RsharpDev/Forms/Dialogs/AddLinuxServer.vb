Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net

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
        Dim linux As New Config.LinuxServer

        If txtAlias.Text.StringEmpty Then
            linux.name = "New Linux Server"
        Else
            linux.name = txtAlias.Text
        End If
        If txtIP.Text.StringEmpty Then
            MessageBox.Show("IP address of the server can not be empty!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        ElseIf Not txtPort.Text.IsPattern("\d+") Then
            MessageBox.Show("Port number of the server should be an integer value!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End If

        linux.location = New IPEndPoint(txtIP.Text, txtPort.Text)

        If txtUser.Text.StringEmpty Then
            MessageBox.Show("No user name!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Else
            linux.user = txtUser.Text
        End If
        If txtPwd.Text.StringEmpty Then
            MessageBox.Show("No password!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Else
            linux.password = txtPwd.Text
        End If

        linux.description = txtDescription.Text

        Program.Config.server = Program.Config.server _
            .JoinIterates({linux}) _
            .ToArray
        Program.Save()

        Return True
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub
End Class