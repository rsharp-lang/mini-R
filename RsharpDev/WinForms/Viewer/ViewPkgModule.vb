Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports WeifenLuo.WinFormsUI.Docking

Public Class ViewPkgModule : Implements Viewer

    Public Property FilePath As String Implements IFileReference.FilePath

    Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Function View(file As String) As DockContent Implements Viewer.View
        FilePath = file
        Text = file.FileName

        Dim assembly As Assembly = Assembly.UnsafeLoadFrom(file)
        Dim packages As Type() = assembly.GetTypes _
            .Where(Function(pkg)
                       Return Not pkg.GetCustomAttribute(Of PackageAttribute) Is Nothing
                   End Function) _
            .ToArray

        If packages.Length = 0 Then
            TabPage2.Hide()
            TabPage2.Visible = False
            Controls.Remove(TabPage2)
        End If

        For Each pkg As Type In packages
            Dim name As PackageAttribute = pkg.GetCustomAttribute(Of PackageAttribute)
            Dim node As New TreeNode With {.Text = name.Namespace}

            For Each method As MethodInfo In pkg.GetMethods
                Dim exportApi As ExportAPIAttribute = method.GetCustomAttribute(Of ExportAPIAttribute)

                If Not exportApi Is Nothing Then
                    node.Nodes.Add(exportApi.Name)
                End If
            Next

            TreeView1.Nodes.Add(node)
        Next

        Dim info As AssemblyInfo = assembly.FromAssembly

        txtTitle.Text = info.AssemblyTitle
        txtInfo.Text = info.AssemblyDescription
        txtCompany.Text = info.AssemblyCompany
        txtCopyright.Text = info.AssemblyCopyright
        txtProduct.Text = info.AssemblyProduct
        txtTrademark.Text = info.AssemblyTrademark
        DateTimePicker1.Value = info.BuiltTime

        Return Me
    End Function

    Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function

    Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Throw New NotImplementedException()
    End Function
End Class