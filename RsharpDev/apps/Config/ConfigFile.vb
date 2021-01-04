Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports WeifenLuo.WinFormsUI.Docking

Namespace Config

    Public Class ConfigFile : Inherits XmlDataModel

        Public Property theme As VisualStudioToolStripExtender.VsVersion
        Public Property type As ThemeType

        <XmlElement>
        Public Property server As LinuxServer()

        Public Property recentFiles As String()

        Public Shared ReadOnly Property FileLocation As String
            Get
                Return App.ProductProgramData & "/config.ini.xml"
            End Get
        End Property

    End Class

    Public Enum ThemeType
        Blue
        Dark
        Light
    End Enum
End Namespace