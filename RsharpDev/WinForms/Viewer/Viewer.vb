Imports Microsoft.VisualBasic.ComponentModel
Imports WeifenLuo.WinFormsUI.Docking

Public Interface Viewer : Inherits IFileReference, ISaveHandle

    Function View(file As String) As DockContent
End Interface
