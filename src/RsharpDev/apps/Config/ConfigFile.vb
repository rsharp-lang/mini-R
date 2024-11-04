Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports WeifenLuo.WinFormsUI.Docking

Namespace Config

    Public Class ConfigFile : Inherits XmlDataModel

        Public Property theme As VisualStudioToolStripExtender.VsVersion
        Public Property type As ThemeType

        <XmlElement>
        Public Property server As LinuxServer()

        Public Property recentFiles As NamedValue()

        Public Shared ReadOnly Property FileLocation As String
            Get
                Return App.ProductProgramData & "/config.ini.xml"
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="file">file full path</param>
        Public Sub AddRecent(file As String)
            recentFiles = recentFiles _
                .JoinIterates(New NamedValue With {.name = file.GetFullPath, .text = Now.ToString}) _
                .GroupBy(Function(f) f.name) _
                .Select(Function(g) g.First) _
                .ToArray
        End Sub

    End Class

    Public Enum ThemeType
        Blue
        Dark
        Light
    End Enum
End Namespace