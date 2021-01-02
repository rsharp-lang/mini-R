Imports My
Imports WeifenLuo.WinFormsUI.Docking

Module VisualStudio

    Public Sub AddDocument(doc As DockContent)
        doc.Show(MyApplication.RStudio.DockPanel1)
        doc.DockState = DockState.Document
    End Sub
End Module
