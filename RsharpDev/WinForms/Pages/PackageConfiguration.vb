Imports System.ComponentModel
Imports SMRUCC.Rsharp.Development.Package.File
Imports WeifenLuo.WinFormsUI.Docking

Public Class PackageConfiguration

    Public Sub LoadSolution()
        Dim info As DESCRIPTION = Program.Solution.LoadInformation

        txtPackage.Text = info.Package
        txtType.Text = info.Type
        txtTitle.Text = info.Title
        txtVersion.Text = info.Version
        txtDate.Value = Date.Parse(info.Date)
        txtAuthor.Text = info.Author
        txtMaintainer.Text = info.Maintainer
        txtDescription.Text = info.Description
        txtLincese.Text = info.License

        Text = $"Solution '{info.Package}'"
    End Sub

    Private Sub PackageConfiguration_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        e.Cancel = True
        DockState = DockState.Hidden
    End Sub

    Private Sub txtLincese_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles txtLincese.LinkClicked

    End Sub
End Class