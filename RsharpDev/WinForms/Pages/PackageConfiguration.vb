Imports System.ComponentModel
Imports WeifenLuo.WinFormsUI.Docking

Public Class PackageConfiguration
    Private Sub PackageConfiguration_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        e.Cancel = True
        DockState = DockState.Hidden
    End Sub
End Class