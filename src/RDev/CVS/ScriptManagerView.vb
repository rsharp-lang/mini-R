Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
Imports RDev.CVS
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Data.MyersDiff
Imports Ollama
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms

''' <summary>
''' A winform user control for manage the version history of the R# scripts:
''' the left webview2 panel browses the script / version / run history of the
''' zip database and the right monaco editor edits the script text of one
''' specific version.
''' </summary>
Public Class ScriptManagerView

    ''' <summary>the virtual host of the embedded html page</summary>
    Shared ReadOnly host As String = "https://cvs.local/"
    Shared ReadOnly resourceFilter As String = "https://cvs.local/*"

    ''' <summary>
    ''' the file path of the zip database package, by default is located at
    ''' ``%LOCALAPPDATA%\RDev\scripts.rscvs``
    ''' </summary>
    Public Property DatabasePath As String

    ''' <summary>
    ''' the file path of the R# script host program (Rscript.exe)
    ''' </summary>
    Public Property RscriptPath As String

    ''' <summary>
    ''' the underlying zip database of current control
    ''' </summary>
    Public ReadOnly Property Database As CvsDatabase
        Get
            Return db
        End Get
    End Property

    ''' <summary>
    ''' the code editor provider of the host application: the script manager
    ''' exports the script of one version as a temp file, then the delegate
    ''' opens the file inside the editor window of the host application and
    ''' returns the editor instance (a winform form that implements the
    ''' <see cref="IEditor"/> interface).
    '''
    ''' ```
    ''' view.OpenEditor = Function(scriptFile)
    '''                      Dim doc As New RsharpDevVscode
    '''                      Call doc.View(scriptFile)
    '''                      Return doc
    '''                  End Function
    ''' ```
    ''' </summary>
    ''' <returns>the editor window that was created by the host application</returns>
    Public Property OpenEditor As Func(Of IEditor)
    Public Property OpenTaskHost As Func(Of IRunnerHost)

    ''' <summary>
    ''' the editor window that is currently opened by the host application
    ''' </summary>
    Public ReadOnly Property Editor As IEditor
        Get
            Return _editor
        End Get
    End Property

    Public ReadOnly Property CurrentScriptId As String
        Get
            Return scriptId
        End Get
    End Property

    Public ReadOnly Property CurrentVersionId As String
        Get
            Return versionId
        End Get
    End Property

    ''' <summary>raised when a new version was created from the editor</summary>
    Public Event VersionCreated As EventHandler(Of VersionRecord)
    ''' <summary>raised when a run task was finished and written into the database</summary>
    Public Event RunCompleted As EventHandler(Of RunRecord)
    ''' <summary>the status text of current control</summary>
    Public Event StatusMessage As EventHandler(Of String)

    Dim db As CvsDatabase
    Dim runner As ScriptRunner
    Dim channel As WebView2CommandChannel
    Dim pageReady As Boolean
    Dim pending As New List(Of KeyValuePair(Of String, Object))
    Dim WithEvents core As CoreWebView2

    Dim scriptId As String
    Dim versionId As String
    Dim lastArgs As New List(Of RunArgument)
    Dim lastWorkdir As String

    ''' <summary>
    ''' the default working folder for script runs: App.Home/rstudio/bin.
    ''' this replaces the previous default of the runner temp folder so that the
    ''' html page and the toolbar runs both start from a predictable directory.
    ''' </summary>
    Private ReadOnly Property DefaultWorkdir As String
        Get
            Return System.IO.Path.Combine(Microsoft.VisualBasic.App.HOME, "rstudio", "bin")
        End Get
    End Property
    ' the editor window of the host application + the version that it displays
    Dim _editor As IEditor
    Dim _editorVersionId As String
    ' versionId -> scriptId lookup, rebuilt whenever the script list changes
    Dim versionIndex As Dictionary(Of String, String)

    Sub New()
        Call InitializeComponent()
    End Sub

    ''' <summary>
    ''' locate an embedded manifest resource by case-insensitive suffix match.
    ''' </summary>
    Private Shared Function GetManifestResource(suffix As String) As Stream
        Dim asm As Assembly = GetType(ScriptManagerView).Assembly

        For Each name As String In asm.GetManifestResourceNames()
            If name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then
                Return asm.GetManifestResourceStream(name)
            End If
        Next

        Throw New System.Resources.MissingManifestResourceException("Embedded resource ending with '" & suffix & "' was not found in assembly " & asm.FullName & ".")
    End Function

    Private Async Sub ScriptManagerView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If db Is Nothing Then
            Call OpenDatabase(DatabasePath)
        End If

        Try
            Await WebViewLoader.Init(WebView21)
        Catch ex As Exception
            RaiseStatus("webview2 init failed: " & ex.Message, "error")
            Exit Sub
        End Try

        channel = New WebView2CommandChannel(WebView21) With {.IsReady = True}
        core = WebView21.CoreWebView2
        runner = New ScriptRunner(OpenTaskHost, RscriptPath)

        ' serve the html page of the script manager from the embedded resources
        WebView21.CoreWebView2.AddWebResourceRequestedFilter(resourceFilter, CoreWebView2WebResourceContext.All)
        WebView21.CoreWebView2.Navigate(host)
    End Sub

    ''' <summary>
    ''' open or create the zip database package of the R# script CVS
    ''' </summary>
    Public Sub OpenDatabase(Optional path As String = Nothing)
        If path.StringEmpty Then
            path = CvsDatabase.DefaultPath
        End If

        DatabasePath = path
        db = CvsDatabase.Open(path)

        RaiseStatus($"database: {db.FilePath}", "info")
    End Sub

    Private Sub OnWebResourceRequested(sender As Object, e As CoreWebView2WebResourceRequestedEventArgs) Handles core.WebResourceRequested
        Dim def = e.GetDeferral()

        Try
            Dim localPath As String = New Uri(e.Request.Uri).LocalPath.Trim("/"c).ToLower()
            Dim file As String = Nothing
            Dim mime As String = Nothing

            Select Case localPath
                Case "", "index.html", "manager.html"
                    file = "manager.html"
                    mime = "text/html; charset=utf-8"
                Case "manager.css"
                    file = "manager.css"
                    mime = "text/css"
                Case "manager.js"
                    file = "manager.js"
                    mime = "text/javascript"
            End Select

            If file Is Nothing Then
                e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(Nothing, 404, "Not Found", "Content-Type: text/plain")
                Return
            End If

            Using res As Stream = GetManifestResource(file)
                Dim ms As New MemoryStream

                Call res.CopyTo(ms)
                Call ms.Seek(Scan0, SeekOrigin.Begin)

                e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(ms, 200, "OK", "Content-Type: " & mime)
            End Using
        Catch ex As Exception
            If e.Response Is Nothing Then
                e.Response = WebView21.CoreWebView2.Environment.CreateWebResourceResponse(Nothing, 500, "Error", "Content-Type: text/plain")
            End If
        Finally
            def.Complete()
        End Try
    End Sub

    Private Sub OnWebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs) Handles core.WebMessageReceived
        Try
            Using doc As JsonDocument = JsonDocument.Parse(e.WebMessageAsJson)
                Dim root As JsonElement = doc.RootElement
                Dim typeEl As JsonElement

                If Not root.TryGetProperty("type", typeEl) Then
                    Return
                End If

                Dim type As String = typeEl.GetString()
                Dim payload As JsonElement = Nothing

                If root.TryGetProperty("payload", payload) Then
                    Call Dispatch(type, payload)
                Else
                    Call Dispatch(type, Nothing)
                End If
            End Using
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    Private Sub Dispatch(type As String, payload As JsonElement)
        Select Case type
            Case CvsOutgoing.Ready
                pageReady = True
                Call FlushPending()
                Call SendDatabase()
                Call SendScripts()

            Case CvsOutgoing.NewScript
                Dim name As String = CvsPayload.Str(payload, "name")
                Dim srcScript As String = CvsPayload.Str(payload, "sourceScriptId")
                Dim srcVersion As String = CvsPayload.Str(payload, "sourceVersionId")

                If srcScript.StringEmpty OrElse srcVersion.StringEmpty Then
                    Call CreateScript(name)
                Else
                    Call CreateScript(name, srcScript, srcVersion)
                End If

            Case CvsOutgoing.SelectScript
                Call SelectScript(CvsPayload.Str(payload, "scriptId"))

            Case CvsOutgoing.SelectVersion
                Call SelectVersion(CvsPayload.Str(payload, "scriptId"), CvsPayload.Str(payload, "versionId"))

            Case CvsOutgoing.OpenInEditor
                Call OpenInEditor(CvsPayload.Str(payload, "scriptId"), CvsPayload.Str(payload, "versionId"))

            Case CvsOutgoing.SaveVersion
                Call SaveVersion(CvsPayload.Str(payload, "scriptId"), CvsPayload.Str(payload, "comment"))

            Case CvsOutgoing.SuggestComment
                Call SuggestCommentAsync()

            Case CvsOutgoing.RunScript
                Call RunScriptAsync(payload)

            Case CvsOutgoing.KillTask
                Call KillCurrentTask()

            Case CvsOutgoing.RequestOutput
                Call SendRunOutput(CvsPayload.Str(payload, "scriptId"), CvsPayload.UInt(payload, "runId"))

            Case CvsOutgoing.DeleteScript
                Call RemoveScript(CvsPayload.Str(payload, "scriptId"))

            Case CvsOutgoing.SearchScripts
                Call SearchScripts(CvsPayload.Str(payload, "query"))
        End Select
    End Sub

    ' ===================== database -> page =====================

    Private Sub SendDatabase()
        If db Is Nothing Then
            Return
        End If

        Call Post(CvsIncoming.Database, New Dictionary(Of String, Object) From {
            {"path", db.FilePath},
            {"statistic", db.Statistic()}
        })
    End Sub

    Private Sub SendScripts()
        If db Is Nothing Then
            Return
        End If

        Call RebuildVersionIndex()
        Call Post(CvsIncoming.Scripts, db.ListScripts())
    End Sub

    ''' <summary>
    ''' fuzzy search the script code with the qgram full text index, an empty
    ''' query just list all of the scripts of the database.
    ''' </summary>
    Private Sub SearchScripts(query As String)
        If db Is Nothing Then
            Return
        End If
        If query.StringEmpty Then
            Call SendScripts()
            Return
        End If

        Try
            Dim hits As ScriptSearchHit() = db.SearchScripts(query, top:=30)

            Call Post(CvsIncoming.SearchResults, hits)
            RaiseStatus($"search '{query}': {hits.Length} hits", "info")
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    Private Sub SelectScript(scriptId As String)
        If scriptId.StringEmpty Then
            Return
        End If

        Me.scriptId = scriptId

        Dim versions As VersionRecord() = db.ListVersions(scriptId)

        Call Post(CvsIncoming.Versions, versions)

        If versions.Length > 0 Then
            Call SelectVersion(scriptId, versions(versions.Length - 1).versionId)
        Else
            versionId = Nothing

            Call Post(CvsIncoming.Runs, New RunRecord() {})
            Call Post(CvsIncoming.VersionDetail, New Dictionary(Of String, Object) From {
                {"versionId", Nothing},
                {"metadata", Nothing},
                {"workdir", DefaultWorkdir}
            })
        End If
    End Sub

    ''' <summary>
    ''' rebuild the versionId -> scriptId lookup used by <see cref="BuildLineage"/>
    ''' to resolve cross script ancestors. version ids are globally unique (the
    ''' serial counter is database scoped), so a single map is enough.
    ''' </summary>
    Private Sub RebuildVersionIndex()
        versionIndex = New Dictionary(Of String, String)

        If db Is Nothing Then
            Return
        End If

        For Each s As ScriptRecord In db.ListScripts()
            For Each v As VersionRecord In db.ListVersions(s.id)
                If Not versionIndex.ContainsKey(v.versionId) Then
                    versionIndex(v.versionId) = s.id
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' walk the parentVersionId chain from the current version and produce an
    ''' ordered (newest first) list of lineage nodes for the evolution view.
    ''' </summary>
    Private Function BuildLineage(scriptId As String, versionId As String) As List(Of Dictionary(Of String, Object))
        Dim chain As New List(Of Dictionary(Of String, Object))
        Dim sid As String = scriptId
        Dim vid As String = versionId
        Dim guard As Integer = 0

        Do While Not vid.StringEmpty AndAlso guard < 1000
            guard += 1

            Dim srec As ScriptRecord = db.GetScript(sid)
            Dim vrec As VersionRecord = db.GetVersion(sid, vid)

            chain.Add(New Dictionary(Of String, Object) From {
                {"scriptId", sid},
                {"scriptName", If(srec?.name, sid)},
                {"versionId", vid},
                {"comment", If(vrec?.comment, "")},
                {"created", If(vrec?.created, "")}
            })

            If vrec Is Nothing OrElse vrec.parentVersionId.StringEmpty Then
                Exit Do
            End If

            vid = vrec.parentVersionId

            If versionIndex IsNot Nothing AndAlso versionIndex.ContainsKey(vid) Then
                sid = versionIndex(vid)
            End If
        Loop

        Return chain
    End Function

    Private Sub SelectVersion(scriptId As String, versionId As String)
        If Not scriptId.StringEmpty Then
            Me.scriptId = scriptId
        End If
        If versionId.StringEmpty Then
            Return
        End If

        Me.versionId = versionId

        Dim ver As VersionRecord = db.GetVersion(Me.scriptId, versionId)
        Dim workdir As String = If(lastWorkdir, DefaultWorkdir)

        Call Post(CvsIncoming.VersionDetail, New Dictionary(Of String, Object) From {
            {"versionId", versionId},
            {"metadata", If(ver Is Nothing, Nothing, ver.metadata)},
            {"workdir", workdir},
            {"comment", If(ver Is Nothing, Nothing, ver.comment)},
            {"lineage", If(ver Is Nothing, Nothing, BuildLineage(Me.scriptId, versionId))}
        })
        Call Post(CvsIncoming.Runs, db.ListRuns(Me.scriptId, versionId))
    End Sub

    Private Sub SendRunOutput(scriptId As String, runId As UInteger)
        If db Is Nothing OrElse runId = 0 Then
            Return
        End If

        Dim id As String = If(scriptId, Me.scriptId)

        Call Post(CvsIncoming.Output, New Dictionary(Of String, Object) From {
            {"runId", runId},
            {"stdout", db.GetRunOutput(id, runId, stderr:=False)},
            {"stderr", db.GetRunOutput(id, runId, stderr:=True)}
        })
    End Sub

    ' ===================== editor interop =====================

    ''' <summary>
    ''' export the script text of one version as a temp file, so that the host
    ''' application can open it inside its own code editor window.
    ''' </summary>
    Private Function ExportVersionFile(scriptId As String, versionId As String, code As String) As String
        Dim dir As String = System.IO.Path.Combine(runner.TempFolder, scriptId)

        If Not Directory.Exists(dir) Then
            Call Directory.CreateDirectory(dir)
        End If

        Dim scriptPath As String = System.IO.Path.Combine(dir, versionId & ".R")

        Call File.WriteAllText(scriptPath, If(code, ""), New UTF8Encoding(False))

        Return scriptPath
    End Function

    ''' <summary>
    ''' open the script of one version inside the code editor window of the host
    ''' application, the editor window is created by the
    ''' <see cref="OpenEditor"/> delegate.
    ''' </summary>
    ''' <param name="scriptId"></param>
    ''' <param name="versionId"></param>
    Public Sub OpenInEditor(Optional scriptId As String = Nothing, Optional versionId As String = Nothing)
        If db Is Nothing Then
            Return
        End If
        If Not scriptId.StringEmpty Then
            Me.scriptId = scriptId
        End If
        If Not versionId.StringEmpty Then
            Me.versionId = versionId
        End If
        If Me.versionId.StringEmpty Then
            RaiseStatus("no version selected", "warn")
            Return
        End If
        If OpenEditor Is Nothing Then
            RaiseStatus("the host application does not provide a code editor", "warn")
            Return
        End If

        Try
            Dim code As String = db.GetVersionCode(Me.scriptId, Me.versionId)
            Dim scriptFile As String = ExportVersionFile(Me.scriptId, Me.versionId, code)
            Dim editor As IEditor = _OpenEditor()

            If editor Is Nothing Then
                RaiseStatus("the code editor of the host application is unavailable", "error")
                Return
            End If

            Dim scriptName As String = Me.scriptId
            Dim rec As ScriptRecord = db.GetScript(Me.scriptId)

            If rec IsNot Nothing Then
                scriptName = rec.name
            End If

            Call editor.LoadScript(If(code, ""), $"{scriptName} [{Me.versionId}]")

            _editor = editor
            _editorVersionId = Me.versionId

            RaiseStatus($"open {Me.versionId} in editor", "info")
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    ''' <summary>
    ''' save the script of the editor window as a new version when the script
    ''' text was modified
    ''' </summary>
    ''' <param name="scriptId"></param>
    ''' <param name="comment">the change comment of the new version</param>
    Public Async Sub SaveVersion(Optional scriptId As String = Nothing, Optional comment As String = Nothing)
        If db Is Nothing Then
            Return
        End If
        If Not scriptId.StringEmpty Then
            Me.scriptId = scriptId
        End If
        If Me.scriptId.StringEmpty Then
            RaiseStatus("no script selected", "warn")
            Return
        End If
        If _editor Is Nothing Then
            RaiseStatus("please open the script in the code editor at first", "warn")
            Return
        End If

        Try
            ' ask the editor window to save its content and then gets the
            ' current script text back for write into the database
            Dim code As String = Await _editor.SaveScript()

            If code.StringEmpty Then
                RaiseStatus("the script of the editor window is empty", "warn")
                Return
            End If

            Dim parent As String = If(_editorVersionId, Me.versionId)
            Dim ver As VersionRecord = db.SaveVersion(Me.scriptId, code, comment, parentVersionId:=parent)

            If ver Is Nothing Then
                RaiseStatus("the script is not modified", "warn")
            Else
                versionId = ver.versionId
                _editorVersionId = ver.versionId

                RaiseStatus($"new version {ver.versionId}", "ok")
                RaiseEvent VersionCreated(Me, ver)

                Call SendScripts()
                Call Post(CvsIncoming.Versions, db.ListVersions(Me.scriptId))
                Call SelectVersion(Me.scriptId, ver.versionId)
            End If
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    ' ===================== ai change comment =====================

    ''' <summary>
    ''' build the prompt that is sent to the LLM: the diff between the old and
    ''' the new version of the script, produced by the MyersDiff algorithm.
    ''' </summary>
    Private Shared Function BuildCommentPrompt(newCode As String, oldCode As String) As String
        Dim sb As New StringBuilder

        If oldCode.StringEmpty Then
            ' there is no previous version: just let the model summarize the
            ' whole new script as the first version change comment
            Call sb.AppendLine("以下是待保存的 R# 脚本（首个版本）的完整内容：")
            Call sb.AppendLine()
            Call sb.AppendLine(newCode)
            Return sb.ToString()
        End If

        Dim differ As New MyersDiff()
        Dim oldLines As String() = ToLines(oldCode)
        Dim newLines As String() = ToLines(newCode)
        Dim result As DiffResult = differ.Compare(oldLines, newLines)

        ' when the edit distance is huge (Truncated) or the diff is enormous,
        ' fall back to sending both full texts with clear labels
        If result.Truncated OrElse result.Items.Count > 2000 Then
            Call sb.AppendLine("=== 上一版本代码 ===")
            Call sb.AppendLine(oldCode)
            Call sb.AppendLine()
            Call sb.AppendLine("=== 当前（待保存）版本代码 ===")
            Call sb.AppendLine(newCode)
            Return sb.ToString()
        End If

        Call sb.AppendLine(result.ToSummary())
        Call sb.AppendLine()
        Call sb.AppendLine(result.ToUnifiedDiff("上一版本.R", "当前版本.R", contextLines:=3))

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' the same diff that <see cref="BuildCommentPrompt"/> feeds to the LLM, but
    ''' returned as a plain unified diff text for the manual review tab of the
    ''' comment dialog.
    ''' </summary>
    Private Shared Function BuildDiffText(newCode As String, oldCode As String) As String
        If oldCode.StringEmpty Then
            Return "（这是首个版本，没有历史版本可供对比）"
        End If

        Dim differ As New MyersDiff()
        Dim result As DiffResult = differ.Compare(ToLines(oldCode), ToLines(newCode))

        If result.Truncated OrElse result.Items.Count > 2000 Then
            Return result.ToSummary() & vbCrLf & vbCrLf & "（差异过大，已省略逐行对比；请直接在编辑器中审阅当前代码）"
        End If

        Return result.ToUnifiedDiff("上一版本.R", "当前版本.R", contextLines:=3)
    End Function

    ''' <summary>
    ''' split a text into an array of lines (handles both crlf and lf)
    ''' </summary>
    Private Shared Function ToLines(text As String) As String()
        If text Is Nothing Then
            Return New String() {}
        End If

        Return text.Replace(vbCrLf, vbLf).Split(New Char() {vbLf}, StringSplitOptions.None)
    End Function

    ''' <summary>
    ''' generate a change comment for the new version of the script by sending
    ''' the diff between the editor content and the previous version to the LLM.
    ''' returns the generated comment text, or Nothing when the operation failed.
    ''' </summary>
    Private Async Function GenerateCommentAsync() As Task(Of String)
        If _editor Is Nothing Then
            RaiseStatus("请先在代码编辑器中打开脚本", "warn")
            Return Nothing
        End If

        Dim cfg As LLMConfig = LLMConfig.LoadDefault()

        If cfg.provider.StringEmpty Then
            RaiseStatus("LLM 未配置（缺少 llm.ini 的 provider）", "warn")
            Return Nothing
        End If

        Dim parentId As String = If(_editorVersionId, Me.versionId)
        Dim newCode As String = Await _editor.SaveScript()

        If newCode.StringEmpty Then
            RaiseStatus("编辑器内容为空", "warn")
            Return Nothing
        End If

        Dim oldCode As String = If(parentId.StringEmpty, "", db.GetVersionCode(Me.scriptId, parentId))
        Dim prompt As String = BuildCommentPrompt(newCode, oldCode)

        If prompt.StringEmpty Then
            RaiseStatus("没有可生成的内容", "warn")
            Return Nothing
        End If

        Try
            Dim client As LLMClient = cfg.CreateLLm()
            client.system_message = "你是一名 R# 脚本版本管理助手。用户会给你一段脚本的新版本与上一版本的代码差异（统一 diff 格式），请据此撰写一段简洁、面向开发者的版本变更说明（中文，2-4 句，聚焦本次实际修改的内容：新增、删除或调整了哪些功能、参数或行为），不要逐行解释，不要重复代码，不要使用代码块标记。"

            Dim resp As LLMsResponse = Await client.Chat(prompt)
            Dim text As String = If(resp?.output, "")

            If text.StringEmpty Then
                RaiseStatus("LLM 返回为空", "warn")
                Return Nothing
            End If

            Return text.Trim()
        Catch ex As Exception
            RaiseStatus("LLM 生成失败: " & ex.Message, "error")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' compute the unified diff of the editor content against its base version,
    ''' used to populate the review tab of the comment dialog.
    ''' </summary>
    Private Async Function ComputeDiffText() As Task(Of String)
        If _editor Is Nothing Then
            Return "（编辑器未打开，无法计算差异）"
        End If

        Dim parentId As String = If(_editorVersionId, Me.versionId)
        Dim newCode As String = Await _editor.SaveScript()
        Dim oldCode As String = If(parentId.StringEmpty, "", db.GetVersionCode(Me.scriptId, parentId))

        Return BuildDiffText(newCode, oldCode)
    End Function

    ''' <summary>
    ''' host side handler of the html "AI 生成" button: generates a change
    ''' comment and posts it back to the page (so the page can re-enable its
    ''' button even when generation failed).
    ''' </summary>
    Private Async Sub SuggestCommentAsync()
        Dim comment As String = Await GenerateCommentAsync()

        ' always post back so the html button can re-enable itself
        Call Post(CvsIncoming.CommentSuggestion, New Dictionary(Of String, Object) From {
            {"comment", If(comment, "")}
        })

        If Not comment.StringEmpty Then
            RaiseStatus("已生成变更说明", "ok")
        End If
    End Sub

    ''' <summary>
    ''' request the host to kill the currently running task process. the kill is
    ''' delegated to the IRunnerHost instance that ScriptRunner spun up for the
    ''' current run (RsharpDevConsole.StopProcess).
    ''' </summary>
    Private Sub KillCurrentTask()
        If runner Is Nothing OrElse runner.CurrentHost Is Nothing Then
            RaiseStatus("当前没有可终止的任务", "warn")
            Return
        End If

        Try
            Call runner.CurrentHost.Kill()
            RaiseStatus("已发送终止请求，正在关闭任务进程…", "ok")
        Catch ex As Exception
            RaiseStatus("终止失败: " & ex.Message, "error")
        End Try
    End Sub

    ' ===================== script run =====================

    Private Async Sub RunScriptAsync(payload As JsonElement)
        Dim sid As String = If(CvsPayload.Str(payload, "scriptId"), scriptId)
        Dim vid As String = If(CvsPayload.Str(payload, "versionId"), versionId)
        Dim workdir As String = CvsPayload.Str(payload, "workdir")
        Dim args As List(Of RunArgument) = CvsPayload.Arguments(payload, "args")

        lastArgs = args

        If db Is Nothing Then
            Return
        End If
        If vid.StringEmpty Then
            RaiseStatus("no version selected", "warn")
            Return
        End If

        Try
            RaiseStatus("running...", "busy")

            Dim code As String = db.GetVersionCode(sid, vid)
            Dim scriptFile As String = runner.ExportScript(vid, code)

            If workdir.StringEmpty Then
                workdir = DefaultWorkdir
            End If

            If runner Is Nothing Then
                runner = New ScriptRunner(OpenTaskHost, RscriptPath, workdir)
            End If

            lastWorkdir = workdir
            runner.RscriptPath = RscriptPath

            Dim result As RunResult = Await runner.Run(scriptFile, args, workdir)
            Dim run As RunRecord = RunRecord.Create(sid, vid)

            run.workdir = workdir
            run.arguments = args
            run.commandLine = result.commandLine
            run.exitCode = result.exitCode
            run.durationMs = result.durationMs
            run.success = result.success

            Call db.AddRun(run, result.stdout, result.stderr)

            RaiseEvent RunCompleted(Me, run)

            Call Post(CvsIncoming.Runs, db.ListRuns(sid, vid))
            Call Post(CvsIncoming.RunResult, New Dictionary(Of String, Object) From {
                {"ok", result.message Is Nothing},
                {"exitCode", result.exitCode},
                {"durationMs", result.durationMs},
                {"stdout", result.stdout},
                {"stderr", result.stderr},
                {"message", result.message}
            })
            Call Post(CvsIncoming.Output, New Dictionary(Of String, Object) From {
                {"runId", run.runId},
                {"stdout", result.stdout},
                {"stderr", result.stderr}
            })

            If result.message.StringEmpty Then
                RaiseStatus($"exit {result.exitCode} in {result.durationMs}ms", If(result.exitCode = 0, "ok", "error"))
            Else
                RaiseStatus(result.message, "error")
            End If
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    ' ===================== script manage =====================

    Private Sub CreateScript(name As String)
        If db Is Nothing Then
            Return
        End If
        If name.StringEmpty Then
            name = "script.R"
        End If

        Try
            Dim rec As ScriptRecord = db.AddScript(name, NewScriptTemplate(name), comment:="create script")

            scriptId = rec.id
            versionId = rec.latestVersionId

            Call SendScripts()
            Call Post(CvsIncoming.Versions, db.ListVersions(rec.id))
            Call SelectVersion(rec.id, versionId)
            Call OpenInEditor(rec.id, versionId)

            RaiseStatus($"script '{name}' created", "ok")
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    ''' <summary>
    ''' create a brand new script that is forked from an existing version of
    ''' another (or the same) script: the source version code becomes the first
    ''' version of the new script, and its ancestor is recorded in parentVersionId.
    ''' </summary>
    Private Sub CreateScript(name As String, sourceScriptId As String, sourceVersionId As String)
        If db Is Nothing Then
            Return
        End If
        If name.StringEmpty Then
            name = "script.R"
        End If

        Try
            Dim code As String = db.GetVersionCode(sourceScriptId, sourceVersionId)

            If code Is Nothing Then
                code = ""
            End If

            Dim rec As ScriptRecord = db.AddScript(name, code, comment:=$"派生自 {sourceScriptId} {sourceVersionId}", ancestorVersionId:=sourceVersionId)

            scriptId = rec.id
            versionId = rec.latestVersionId

            Call SendScripts()
            Call Post(CvsIncoming.Versions, db.ListVersions(rec.id))
            Call SelectVersion(rec.id, versionId)
            Call OpenInEditor(rec.id, versionId)

            RaiseStatus($"script '{name}' forked from {sourceVersionId}", "ok")
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    Private Sub RemoveScript(scriptId As String)
        If db Is Nothing OrElse scriptId.StringEmpty Then
            Return
        End If

        Try
            Call db.DeleteScript(scriptId)

            Me.scriptId = Nothing
            versionId = Nothing

            Call SendScripts()
            Call Post(CvsIncoming.Versions, New VersionRecord() {})
            Call Post(CvsIncoming.Runs, New RunRecord() {})
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    ''' <summary>
    ''' the template of a new created R# script
    ''' </summary>
    Public Shared Function NewScriptTemplate(name As String) As String
        Dim title As String = If(name.StringEmpty, "unnamed script", name)
        Dim sb As New StringBuilder

        Call sb.AppendLine($"#' @title {title}")
        Call sb.AppendLine($"#' @description a short description of the R# script '{title}'")
        Call sb.AppendLine($"#' @author {Environment.UserName}")
        Call sb.AppendLine()
        Call sb.AppendLine("let input as string = ?""--input"" || ""test.txt"";")
        Call sb.AppendLine("let debug as boolean = ?""--debug"";")
        Call sb.AppendLine()
        Call sb.AppendLine("print(`input=${input}`);")

        Return sb.ToString()
    End Function

    ' ===================== message channel =====================

    ''' <summary>
    ''' post a message to the html page, the webview2 message api requires
    ''' the call happens on the UI thread.
    ''' </summary>
    Private Sub Post(type As String, payload As Object)
        If Me.InvokeRequired Then
            Call Me.BeginInvoke(New Action(Sub() Post(type, payload)))
            Return
        End If
        If channel Is Nothing OrElse Not channel.IsReady OrElse Not pageReady Then
            pending.Add(New KeyValuePair(Of String, Object)(type, payload))
            Return
        End If

        Call channel.Send(type, payload)
    End Sub

    Private Sub FlushPending()
        If channel Is Nothing Then
            Return
        End If

        For Each cmd As KeyValuePair(Of String, Object) In pending
            Call channel.Send(cmd.Key, cmd.Value)
        Next

        pending.Clear()
    End Sub

    Private Sub RaiseStatus(text As String, Optional level As String = "info")
        RaiseEvent StatusMessage(Me, text)

        Call Post(CvsIncoming.Status, New Dictionary(Of String, Object) From {
            {"text", text},
            {"level", level}
        })
    End Sub

    ' ===================== toolbar =====================

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        Dim n As Integer = If(db Is Nothing, 0, db.ListScripts().Length)
        Dim name As String = Interaction.InputBox("请输入新脚本的名称:", "新建 R# 脚本", $"script{n + 1}.R")

        If name.StringEmpty Then
            Return
        End If

        Call CreateScript(name.Trim)
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Call OpenInEditor()
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If scriptId.StringEmpty Then
            RaiseStatus("no script selected", "warn")
            Return
        End If

        Dim diff As String = ""

        Try
            diff = Await ComputeDiffText()
        Catch
            diff = "（计算差异时出错）"
        End Try

        Using dlg As New CommentDialog(AddressOf GenerateCommentAsync, Function() diff)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                Call SaveVersion(scriptId, If(dlg.Comment, ""))
            End If
        End Using
    End Sub

    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        If versionId.StringEmpty Then
            RaiseStatus("no version selected", "warn")
            Return
        End If

        Dim args As List(Of RunArgument) = If(lastArgs, New List(Of RunArgument)())

        If args.Count = 0 AndAlso Not scriptId.StringEmpty AndAlso Not versionId.StringEmpty Then
            ' fill the arguments from the metadata default value when the
            ' html form was not submitted yet
            Dim ver As VersionRecord = db.GetVersion(scriptId, versionId)

            If Not ver Is Nothing AndAlso Not ver.metadata Is Nothing Then
                For Each arg As ArgumentSpec In ver.metadata.arguments.SafeQuery
                    args.Add(New RunArgument With {.name = arg.name, .value = arg.defaultValue})
                Next
            End If
        End If

        Call RunScriptAsyncFromToolbar(args)
    End Sub

    Private Async Sub RunScriptAsyncFromToolbar(args As List(Of RunArgument))
        Dim sid As String = scriptId
        Dim vid As String = versionId

        If db Is Nothing OrElse vid.StringEmpty Then
            Return
        End If

        If runner Is Nothing Then
            runner = New ScriptRunner(OpenTaskHost, RscriptPath)
        End If

        Try
            RaiseStatus("running...", "busy")

            Dim code As String = db.GetVersionCode(sid, vid)
            Dim scriptFile As String = runner.ExportScript(vid, code)
            Dim workdir As String = If(lastWorkdir, DefaultWorkdir)
            Dim result As RunResult = Await runner.Run(scriptFile, args, workdir)
            Dim run As RunRecord = RunRecord.Create(sid, vid)

            run.workdir = workdir
            run.arguments = args
            run.commandLine = result.commandLine
            run.exitCode = result.exitCode
            run.durationMs = result.durationMs
            run.success = result.success

            Call db.AddRun(run, result.stdout, result.stderr)

            RaiseEvent RunCompleted(Me, run)

            Call Post(CvsIncoming.Runs, db.ListRuns(sid, vid))
            Call Post(CvsIncoming.Output, New Dictionary(Of String, Object) From {
                {"runId", run.runId},
                {"stdout", result.stdout},
                {"stderr", result.stderr}
            })

            If result.message.StringEmpty Then
                RaiseStatus($"exit {result.exitCode} in {result.durationMs}ms", If(result.exitCode = 0, "ok", "error"))
            Else
                RaiseStatus(result.message, "error")
            End If
        Catch ex As Exception
            RaiseStatus(ex.Message, "error")
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        Call SendDatabase()
        Call SendScripts()

        If Not scriptId.StringEmpty Then
            Call Post(CvsIncoming.Versions, db.ListVersions(scriptId))
            Call SelectVersion(Nothing, versionId)
        End If
    End Sub
End Class
