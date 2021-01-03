Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel

Namespace Config

    Public Class ConfigFile : Inherits XmlDataModel

        <XmlElement>
        Public Property server As LinuxServer()

        Public Shared ReadOnly Property FileLocation As String
            Get
                Return App.ProductProgramData & "/config.ini.xml"
            End Get
        End Property

    End Class
End Namespace