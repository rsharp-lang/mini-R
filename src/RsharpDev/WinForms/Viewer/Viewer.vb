Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualStudio.WinForms.Docking

Public Interface Viewer : Inherits IFileReference, ISaveHandle

    Function View(file As String) As DockContent
End Interface
