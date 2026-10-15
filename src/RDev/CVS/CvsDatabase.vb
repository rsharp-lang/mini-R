Imports System.IO
Imports System.IO.Compression
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Serialization

Namespace CVS

    ''' <summary>
    ''' the json helper of the R# script CVS database
    ''' </summary>
    Friend Module CvsJson

        ReadOnly opts As JsonSerializerOptions = New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            .NumberHandling = JsonNumberHandling.AllowReadingFromString,
            .WriteIndented = False
        }

        ''' <summary>
        ''' serialize an object as camelCase friendly json text (all of the data
        ''' contract members are declared in camelCase already, so no naming
        ''' policy is applied here).
        ''' </summary>
        Public Function Serialize(any As Object) As String
            Return JsonSerializer.Serialize(any, opts)
        End Function

        Public Function Deserialize(Of T)(json As String) As T
            If json.StringEmpty Then
                Return Nothing
            End If

            Return JsonSerializer.Deserialize(Of T)(json, opts)
        End Function

        ''' <summary>
        ''' read a member value from a json object text in a safe manner
        ''' </summary>
        Public Function GetString(json As String, member As String) As String
            If json.StringEmpty Then
                Return Nothing
            End If

            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim el As JsonElement

                If doc.RootElement.ValueKind <> JsonValueKind.Object Then
                    Return Nothing
                End If
                If Not doc.RootElement.TryGetProperty(member, el) Then
                    Return Nothing
                End If
                If el.ValueKind = JsonValueKind.Null OrElse el.ValueKind = JsonValueKind.Undefined Then
                    Return Nothing
                End If

                Return el.ToString()
            End Using
        End Function
    End Module

    ''' <summary>
    ''' A zip archive based database file for manage the version history of the
    ''' R# scripts.
    ''' </summary>
    ''' <remarks>
    ''' the database file is a standard zip package with a custom layout:
    '''
    ''' ```
    ''' index.json                                 the global index of all scripts
    ''' scripts/{scriptId}/index.json              versions + run history of one script
    ''' scripts/{scriptId}/src/{versionId}.R       the script text of one version
    ''' scripts/{scriptId}/runs/{runId}.out.txt    the standard output of one run
    ''' scripts/{scriptId}/runs/{runId}.err.txt    the standard error of one run
    ''' ```
    '''
    ''' the archive is opened in <see cref="ZipArchiveMode.Update"/> mode for each
    ''' operation, so there is no long term file handle held by this class and the
    ''' data is always flushed to the disk at the end of each operation.
    ''' </remarks>
    Public Class CvsDatabase : Implements IDisposable

        Public Const INDEX_ENTRY As String = "index.json"
        Public Const FORMAT_VERSION As Integer = 1
        Public Const FileExtension As String = ".rscvs"

        ''' <summary>
        ''' serialize the database operations (the control may be driven from the
        ''' UI thread and the background run thread at the same time)
        ''' </summary>
        Shared ReadOnly sync As New Object

        ReadOnly file As String
        Dim index As CvsIndex
        Dim disposed As Boolean

        ''' <summary>
        ''' the file path of current database package
        ''' </summary>
        Public ReadOnly Property FilePath As String
            Get
                Return file
            End Get
        End Property

        ''' <summary>
        ''' %LOCALAPPDATA%\RDev\scripts.rscvs
        ''' </summary>
        Public Shared ReadOnly Property DefaultPath As String
            Get
                Return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RDev",
                    "scripts" & FileExtension
                )
            End Get
        End Property

        Sub New(file As String)
            Me.file = file
        End Sub

        ''' <summary>
        ''' open or create a R# script CVS database file
        ''' </summary>
        ''' <param name="path">
        ''' the file path of the database package, by default is located at
        ''' ``%LOCALAPPDATA%\RDev\scripts.rscvs``
        ''' </param>
        Public Shared Function Open(Optional path As String = Nothing) As CvsDatabase
            If path.StringEmpty Then
                path = DefaultPath
            End If

            Dim dir As String = Path.GetDirectoryName(path)

            If Not dir.StringEmpty AndAlso Not Directory.Exists(dir) Then
                Call Directory.CreateDirectory(dir)
            End If

            Dim db As New CvsDatabase(path)
            db.index = db.LoadIndex()

            Return db
        End Function

        Private Function LoadIndex() As CvsIndex
            Dim idx As CvsIndex

            Using zip As ZipArchive = OpenArchive()
                Dim json As String = ReadEntry(zip, INDEX_ENTRY)

                If json.StringEmpty Then
                    idx = New CvsIndex With {
                        .formatVersion = FORMAT_VERSION,
                        .created = RunRecord.NowISO(),
                        .updated = RunRecord.NowISO()
                    }

                    Call WriteEntry(zip, INDEX_ENTRY, CvsJson.Serialize(idx))
                Else
                    idx = CvsJson.Deserialize(Of CvsIndex)(json)

                    If idx Is Nothing Then
                        idx = New CvsIndex With {.created = RunRecord.NowISO()}
                    End If
                End If
            End Using

            Return idx
        End Function

        Private Function OpenArchive() As ZipArchive
            Dim fs As New FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)

            Return New ZipArchive(fs, ZipArchiveMode.Update)
        End Function

        Private Shared Function ReadEntry(zip As ZipArchive, name As String) As String
            Dim entry As ZipArchiveEntry = zip.GetEntry(name)

            If entry Is Nothing Then
                Return Nothing
            End If

            Using s As Stream = entry.Open()
                Using rd As New StreamReader(s, New UTF8Encoding(False))
                    Return rd.ReadToEnd()
                End Using
            End Using
        End Function

        Private Shared Sub WriteEntry(zip As ZipArchive, name As String, text As String)
            Dim entry As ZipArchiveEntry = zip.GetEntry(name)

            If text Is Nothing Then
                text = ""
            End If
            If entry IsNot Nothing Then
                Call entry.Delete()
            End If

            entry = zip.CreateEntry(name)

            Using s As Stream = entry.Open()
                Dim buffer As Byte() = New UTF8Encoding(False).GetBytes(text)

                Call s.Write(buffer, Scan0, buffer.Length)
                Call s.Flush()
            End Using
        End Sub

        Private Shared Sub DeleteEntry(zip As ZipArchive, name As String)
            Dim entry As ZipArchiveEntry = zip.GetEntry(name)

            If entry IsNot Nothing Then
                Call entry.Delete()
            End If
        End Sub

        Private Shared Sub DeleteFolder(zip As ZipArchive, prefix As String)
            For Each entry As ZipArchiveEntry In zip.Entries.ToArray()
                If entry.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
                    Call entry.Delete()
                End If
            Next
        End Sub

        Private Shared Function ScriptIndexPath(scriptId As String) As String
            Return $"scripts/{scriptId}/index.json"
        End Function

        Private Shared Function CodePath(scriptId As String, versionId As String) As String
            Return $"scripts/{scriptId}/src/{versionId}.R"
        End Function

        Private Shared Function OutputPath(scriptId As String, runId As UInteger, stderr As Boolean) As String
            If stderr Then
                Return $"scripts/{scriptId}/runs/{runId}.err.txt"
            Else
                Return $"scripts/{scriptId}/runs/{runId}.out.txt"
            End If
        End Function

        Private Function LoadScriptIndex(zip As ZipArchive, scriptId As String) As ScriptIndex
            Dim json As String = ReadEntry(zip, ScriptIndexPath(scriptId))

            If json.StringEmpty Then
                Return Nothing
            End If

            Dim sidx As ScriptIndex = CvsJson.Deserialize(Of ScriptIndex)(json)

            If sidx Is Nothing Then
                Return Nothing
            End If
            If sidx.versions Is Nothing Then
                sidx.versions = New List(Of VersionRecord)
            End If
            If sidx.runs Is Nothing Then
                sidx.runs = New List(Of RunRecord)
            End If

            Return sidx
        End Function

        Private Sub SaveScriptIndex(zip As ZipArchive, sidx As ScriptIndex)
            If sidx.script IsNot Nothing Then
                sidx.script.versionCount = sidx.versions.Count
                sidx.script.runCount = sidx.runs.Count
                sidx.script.updated = RunRecord.NowISO()
            End If

            Call WriteEntry(zip, ScriptIndexPath(sidx.script.id), CvsJson.Serialize(sidx))
        End Sub

        Private Sub SaveGlobalIndex(zip As ZipArchive)
            index.updated = RunRecord.NowISO()

            Call WriteEntry(zip, INDEX_ENTRY, CvsJson.Serialize(index))
        End Sub

        ''' <summary>
        ''' list all scripts inside current database
        ''' </summary>
        Public Function ListScripts() As ScriptRecord()
            SyncLock sync
                Return index.scripts.ToArray()
            End SyncLock
        End Function

        Public Function GetScript(scriptId As String) As ScriptRecord
            SyncLock sync
                For Each s As ScriptRecord In index.scripts
                    If s.id = scriptId Then
                        Return s
                    End If
                Next

                Return Nothing
            End SyncLock
        End Function

        ''' <summary>
        ''' create a new script inside current database, the first version of the
        ''' script is generated from the given script text.
        ''' </summary>
        Public Function AddScript(name As String, code As String, Optional comment As String = Nothing) As ScriptRecord
            SyncLock sync
                Dim scriptId As String = $"s{index.nextScriptId:0000}"
                Dim rec As New ScriptRecord With {
                    .id = scriptId,
                    .name = If(name.StringEmpty, $"script{scriptId}", name),
                    .created = RunRecord.NowISO(),
                    .updated = RunRecord.NowISO(),
                    .versionCount = 0,
                    .runCount = 0
                }

                index.nextScriptId += 1UI
                index.scripts.Add(rec)

                Dim sidx As New ScriptIndex With {
                    .script = rec,
                    .versions = New List(Of VersionRecord),
                    .runs = New List(Of RunRecord)
                }

                Using zip As ZipArchive = OpenArchive()
                    Dim ver As VersionRecord = SaveVersionInternal(zip, sidx, code, comment, parentVersionId:=Nothing, force:=True)

                    rec.latestVersionId = ver.versionId
                    rec.title = ver.metadata?.title
                    rec.description = ver.metadata?.description

                    Call SaveScriptIndex(zip, sidx)
                    Call SaveGlobalIndex(zip)
                End Using

                Return rec
            End SyncLock
        End Function

        ''' <summary>
        ''' remove a script and all of its versions / run history from the database
        ''' </summary>
        Public Function DeleteScript(scriptId As String) As Boolean
            SyncLock sync
                Dim rec As ScriptRecord = GetScript(scriptId)

                If rec Is Nothing Then
                    Return False
                End If

                Call index.scripts.Remove(rec)

                Using zip As ZipArchive = OpenArchive()
                    Call DeleteFolder(zip, $"scripts/{scriptId}/")
                    Call SaveGlobalIndex(zip)
                End Using

                Return True
            End SyncLock
        End Function

        ''' <summary>
        ''' save the script text as a new version when the content was modified
        ''' </summary>
        ''' <returns>
        ''' the new created version record, or nothing when the script text is
        ''' not changed (and <paramref name="force"/> is FALSE).
        ''' </returns>
        Public Function SaveVersion(scriptId As String,
                                   code As String,
                                   Optional comment As String = Nothing,
                                   Optional parentVersionId As String = Nothing,
                                   Optional force As Boolean = False) As VersionRecord

            SyncLock sync
                Using zip As ZipArchive = OpenArchive()
                    Dim sidx As ScriptIndex = LoadScriptIndex(zip, scriptId)

                    If sidx Is Nothing Then
                        Return Nothing
                    End If

                    Dim ver As VersionRecord = SaveVersionInternal(zip, sidx, code, comment, parentVersionId, force)

                    If ver IsNot Nothing Then
                        Call SaveScriptIndex(zip, sidx)
                        Call SaveGlobalIndex(zip)
                    End If

                    Return ver
                End Using
            End SyncLock
        End Function

        Private Function SaveVersionInternal(zip As ZipArchive,
                                             sidx As ScriptIndex,
                                             code As String,
                                             comment As String,
                                             parentVersionId As String,
                                             force As Boolean) As VersionRecord

            If code Is Nothing Then
                code = ""
            End If

            Dim hash As String = CodeHash(code)

            If Not force AndAlso sidx.versions.Count > 0 Then
                Dim last As VersionRecord = sidx.versions(sidx.versions.Count - 1)

                If last.codeHash = hash AndAlso last.codeLength = code.Length Then
                    ' nothing changed, no new version is required
                    Return Nothing
                End If
            End If

            Dim serial As UInteger = index.nextSerial
            Dim versionId As String = $"{DateTime.Now:yyyy-MM-dd}-{serial:000}"

            index.nextSerial += 1UI

            Dim ver As New VersionRecord With {
                .versionId = versionId,
                .serial = serial,
                .scriptId = sidx.script.id,
                .parentVersionId = parentVersionId,
                .created = RunRecord.NowISO(),
                .comment = If(comment, ""),
                .codeLength = code.Length,
                .codeHash = hash,
                .metadata = ScriptMetadataParser.Parse(code)
            }

            Call WriteEntry(zip, CodePath(sidx.script.id, versionId), code)
            Call sidx.versions.Add(ver)

            sidx.script.latestVersionId = versionId

            If Not ver.metadata Is Nothing Then
                sidx.script.title = ver.metadata.title
                sidx.script.description = ver.metadata.description
            End If

            Return ver
        End Function

        ''' <summary>
        ''' the sha256 hash of the script text (trimmed to 32 hex chars)
        ''' </summary>
        Public Shared Function CodeHash(code As String) As String
            If code Is Nothing Then
                code = ""
            End If

            Dim bytes As Byte() = New UTF8Encoding(False).GetBytes(code)

            Using sha As SHA256 = SHA256.Create()
                Dim sum As Byte() = sha.ComputeHash(bytes)
                Dim sb As New StringBuilder

                For i As Integer = 0 To 15
                    Call sb.Append(sum(i).ToString("x2"))
                Next

                Return sb.ToString()
            End Using
        End Function

        ''' <summary>
        ''' the version list of one script, ordered from the oldest to the newest
        ''' </summary>
        Public Function ListVersions(scriptId As String) As VersionRecord()
            SyncLock sync
                Using zip As ZipArchive = OpenArchive()
                    Dim sidx As ScriptIndex = LoadScriptIndex(zip, scriptId)

                    If sidx Is Nothing Then
                        Return {}
                    End If

                    Return sidx.versions.ToArray()
                End Using
            End SyncLock
        End Function

        Public Function GetVersion(scriptId As String, versionId As String) As VersionRecord
            If versionId.StringEmpty Then
                Return Nothing
            End If

            For Each ver As VersionRecord In ListVersions(scriptId)
                If ver.versionId = versionId Then
                    Return ver
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' read the R# script text of one version from the database
        ''' </summary>
        Public Function GetVersionCode(scriptId As String, versionId As String) As String
            If versionId.StringEmpty Then
                Return Nothing
            End If

            SyncLock sync
                Using zip As ZipArchive = OpenArchive()
                    Return ReadEntry(zip, CodePath(scriptId, versionId))
                End Using
            End SyncLock
        End Function

        ''' <summary>
        ''' the run history of one script (or of one specific version)
        ''' </summary>
        Public Function ListRuns(scriptId As String, Optional versionId As String = Nothing) As RunRecord()
            SyncLock sync
                Using zip As ZipArchive = OpenArchive()
                    Dim sidx As ScriptIndex = LoadScriptIndex(zip, scriptId)

                    If sidx Is Nothing Then
                        Return {}
                    End If
                    If versionId.StringEmpty Then
                        Return sidx.runs.ToArray()
                    End If

                    Dim list As New List(Of RunRecord)

                    For Each run As RunRecord In sidx.runs
                        If run.versionId = versionId Then
                            list.Add(run)
                        End If
                    Next

                    Return list.ToArray()
                End Using
            End SyncLock
        End Function

        ''' <summary>
        ''' write a new run history record into the database
        ''' </summary>
        Public Function AddRun(run As RunRecord, stdout As String, stderr As String) As RunRecord
            SyncLock sync
                Using zip As ZipArchive = OpenArchive()
                    Dim sidx As ScriptIndex = LoadScriptIndex(zip, run.scriptId)

                    If sidx Is Nothing Then
                        Return Nothing
                    End If

                    run.runId = index.nextRunId
                    index.nextRunId += 1UI

                    If run.arguments Is Nothing Then
                        run.arguments = New List(Of RunArgument)
                    End If

                    Dim outBytes As Byte() = New UTF8Encoding(False).GetBytes(If(stdout, ""))
                    Dim errBytes As Byte() = New UTF8Encoding(False).GetBytes(If(stderr, ""))

                    run.stdoutSize = outBytes.Length
                    run.stderrSize = errBytes.Length

                    Call WriteEntry(zip, OutputPath(run.scriptId, run.runId, stderr:=False), stdout)
                    Call WriteEntry(zip, OutputPath(run.scriptId, run.runId, stderr:=True), stderr)
                    Call sidx.runs.Add(run)

                    Call SaveScriptIndex(zip, sidx)
                    Call SaveGlobalIndex(zip)
                End Using

                Return run
            End SyncLock
        End Function

        ''' <summary>
        ''' read the standard output/error text of one run history record
        ''' </summary>
        Public Function GetRunOutput(scriptId As String, runId As UInteger, stderr As Boolean) As String
            SyncLock sync
                Using zip As ZipArchive = OpenArchive()
                    Return ReadEntry(zip, OutputPath(scriptId, runId, stderr))
                End Using
            End SyncLock
        End Function

        Public Function DeleteRun(scriptId As String, runId As UInteger) As Boolean
            SyncLock sync
                Using zip As ZipArchive = OpenArchive()
                    Dim sidx As ScriptIndex = LoadScriptIndex(zip, scriptId)

                    If sidx Is Nothing Then
                        Return False
                    End If

                    Dim hit As RunRecord = Nothing

                    For Each run As RunRecord In sidx.runs
                        If run.runId = runId Then
                            hit = run
                            Exit For
                        End If
                    Next

                    If hit Is Nothing Then
                        Return False
                    End If

                    Call sidx.runs.Remove(hit)
                    Call DeleteEntry(zip, OutputPath(scriptId, runId, stderr:=False))
                    Call DeleteEntry(zip, OutputPath(scriptId, runId, stderr:=True))

                    Call SaveScriptIndex(zip, sidx)
                    Call SaveGlobalIndex(zip)
                End Using

                Return True
            End SyncLock
        End Function

        ''' <summary>
        ''' the number of the scripts / versions / runs inside current database
        ''' </summary>
        Public Function Statistic() As Dictionary(Of String, Integer)
            Dim stat As New Dictionary(Of String, Integer)

            SyncLock sync
                stat("scripts") = index.scripts.Count

                Dim versions As Integer = 0
                Dim runs As Integer = 0

                For Each s As ScriptRecord In index.scripts
                    versions += s.versionCount
                    runs += s.runCount
                Next

                stat("versions") = versions
                stat("runs") = runs
            End SyncLock

            Return stat
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposed Then
                If disposing Then
                    ' there is no long term file handle held by this class:
                    ' the zip archive is always closed at the end of each
                    ' database operation, so nothing to flush here.
                End If

                disposed = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Call Dispose(disposing:=True)
            Call GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
