Imports System.Collections.Generic

Namespace CVS

    ''' <summary>
    ''' the data contract of the R# script CVS database
    ''' </summary>
    ''' <remarks>
    ''' all of the property names are kept in camelCase on purpose: the webview
    ''' frontend reads the very same json keys, and the json serializer of the
    ''' host side (see <see cref="CvsJson"/>) does not rename any member.
    ''' </remarks>

    ''' <summary>
    ''' one managed R# script inside the database
    ''' </summary>
    Public Class ScriptRecord

        ''' <summary>
        ''' the internal unique id, in the format ``s0001``
        ''' </summary>
        Public Property id As String
        Public Property name As String
        Public Property title As String
        Public Property description As String
        ''' <summary>ISO8601 timestamp</summary>
        Public Property created As String
        ''' <summary>ISO8601 timestamp</summary>
        Public Property updated As String
        ''' <summary>the newest version id of current script</summary>
        Public Property latestVersionId As String
        Public Property versionCount As Integer
        Public Property runCount As Integer

        Public Overrides Function ToString() As String
            Return $"[{id}] {name}"
        End Function
    End Class

    ''' <summary>
    ''' the commandline argument of a R# script, comes from the
    ''' ``ShellScript`` parser of the R# interpreter.
    ''' </summary>
    Public Class ArgumentSpec

        Public Property name As String
        ''' <summary>string / integer / double / boolean ...</summary>
        Public Property type As String
        Public Property defaultValue As String
        Public Property description As String
        ''' <summary>
        ''' TRUE when the script requires this argument (the default value is a
        ''' ``stop(...)`` requirement expression)
        ''' </summary>
        Public Property required As Boolean
        Public Property isLiteral As Boolean

        Public Overrides Function ToString() As String
            Return $"{name} <{type}>"
        End Function
    End Class

    ''' <summary>
    ''' the metadata of a R# script version, parsed from the script header
    ''' comment block and the commandline argument getter expression.
    ''' </summary>
    Public Class ScriptMetadata

        Public Property title As String
        Public Property description As String
        Public Property authors As String()
        ''' <summary>the R# package dependency of current script</summary>
        Public Property dependencies As String()
        Public Property arguments As ArgumentSpec()
        ''' <summary>
        ''' not empty when the R# parser failed on current script text, the
        ''' metadata in such situation is incomplete.
        ''' </summary>
        Public Property parseError As String

        Public Shared Function Empty() As ScriptMetadata
            Return New ScriptMetadata With {
                .arguments = {},
                .authors = {},
                .dependencies = {},
                .description = "",
                .title = ""
            }
        End Function
    End Class

    ''' <summary>
    ''' one version snapshot of a managed R# script
    ''' </summary>
    Public Class VersionRecord

        ''' <summary>
        ''' ``yyyy-MM-dd-xxx``, the xxx part is a database scope auto increment id
        ''' </summary>
        Public Property versionId As String
        ''' <summary>the auto increment UInteger id</summary>
        Public Property serial As UInteger
        Public Property scriptId As String
        ''' <summary>the version id that current version was derived from</summary>
        Public Property parentVersionId As String
        ''' <summary>ISO8601 timestamp</summary>
        Public Property created As String
        ''' <summary>the change comment of current version</summary>
        Public Property comment As String
        Public Property codeLength As Long
        ''' <summary>the sha256 (trimmed) of the script text</summary>
        Public Property codeHash As String
        Public Property metadata As ScriptMetadata

        Public Overrides Function ToString() As String
            Return versionId
        End Function
    End Class

    ''' <summary>
    ''' a name/value pair of the commandline arguments of one script run
    ''' </summary>
    Public Class RunArgument

        Public Property name As String
        Public Property value As String

        Public Overrides Function ToString() As String
            Return $"{name}={value}"
        End Function
    End Class

    ''' <summary>
    ''' the run history record of one script version
    ''' </summary>
    Public Class RunRecord

        ''' <summary>the auto increment UInteger id</summary>
        Public Property runId As UInteger
        Public Property scriptId As String
        Public Property versionId As String
        ''' <summary>ISO8601 timestamp</summary>
        Public Property time As String
        ''' <summary>the working directory of the R# script run</summary>
        Public Property workdir As String
        Public Property arguments As List(Of RunArgument)
        ''' <summary>the raw commandline string</summary>
        Public Property commandLine As String
        Public Property exitCode As Integer
        Public Property durationMs As Long
        Public Property success As Boolean
        Public Property stdoutSize As Long
        Public Property stderrSize As Long

        Public Shared Function Create(scriptId As String, versionId As String) As RunRecord
            Return New RunRecord With {
                .arguments = New List(Of RunArgument),
                .scriptId = scriptId,
                .versionId = versionId,
                .time = NowISO(),
                .success = True
            }
        End Function

        Friend Shared Function NowISO() As String
            Return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        End Function

        Public Overrides Function ToString() As String
            Return $"#{runId} {versionId} exit={exitCode}"
        End Function
    End Class

    ''' <summary>
    ''' the per script index file, stored at ``scripts/{scriptId}/index.json``
    ''' </summary>
    Public Class ScriptIndex

        Public Property script As ScriptRecord
        Public Property versions As New List(Of VersionRecord)
        Public Property runs As New List(Of RunRecord)

    End Class

    ''' <summary>
    ''' the database global index, stored at ``index.json``
    ''' </summary>
    Public Class CvsIndex

        Public Property formatVersion As Integer = 1
        Public Property created As String
        Public Property updated As String
        ''' <summary>auto increment id of the script</summary>
        Public Property nextScriptId As UInteger = 1
        ''' <summary>auto increment id of the version number</summary>
        Public Property nextSerial As UInteger = 1
        ''' <summary>auto increment id of the run history</summary>
        Public Property nextRunId As UInteger = 1
        Public Property scripts As New List(Of ScriptRecord)

    End Class
End Namespace
