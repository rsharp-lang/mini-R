Imports WeifenLuo.WinFormsUI.Docking

Public Class ConfigApp

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Select Case ComboBox1.SelectedIndex
            Case 0 ' 2003
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2003
                Program.Config.type = Config.ThemeType.Light
            Case 1 '2005
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2005
                Program.Config.type = Config.ThemeType.Light
            Case 2 ' 2012 B
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2012
                Program.Config.type = Config.ThemeType.Blue
            Case 3 ' 2012 D
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2012
                Program.Config.type = Config.ThemeType.Dark
            Case 4 ' 2012 L
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2012
                Program.Config.type = Config.ThemeType.Light
            Case 5 ' 2013 B
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2013
                Program.Config.type = Config.ThemeType.Blue
            Case 6 '2013 D
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2013
                Program.Config.type = Config.ThemeType.Dark
            Case 7 ' 2013 L
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2013
                Program.Config.type = Config.ThemeType.Light
            Case 8 ' 2015 B
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2015
                Program.Config.type = Config.ThemeType.Blue
            Case 9 '2015 D
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2015
                Program.Config.type = Config.ThemeType.Dark
            Case 10 ' 2015 L
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2015
                Program.Config.type = Config.ThemeType.Light
            Case Else
                ' 2015 L
                Program.Config.theme = VisualStudioToolStripExtender.VsVersion.Vs2015
                Program.Config.type = Config.ThemeType.Light
        End Select

        For Each window In VisualStudio.vsWindow
            Call window.ApplyVsTheme()
        Next

        Call Program.Save()
        Call Me.Close()
    End Sub
End Class