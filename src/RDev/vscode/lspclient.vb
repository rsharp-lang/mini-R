Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports languageserver
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

''' <summary>
''' client code for handling of the lsp backend
''' </summary>
Public Class lspclient

    ' the in-process R# language server. it is created lazily on first use so the
    ' (slow) REnv load never delays the editor startup, and warmed up on a
    ' background thread once the WebView is up.
    Shared _lsp As Lazy(Of RsharpLspServer)

    ' the R# interpreter + its global environment are not thread safe: every LSP
    ' call must be serialized.
    ReadOnly _lspLock As New Threading.SemaphoreSlim(1, 1)
    ReadOnly _lsplog As Action(Of String)

    Sub New(log As Action(Of String))
        _lsplog = log

        ' create the in-process R# language server lazily and warm it up on a
        ' background thread: the REnv load + global symbol scan take seconds and
        ' must not block the UI thread (nor the editor startup).
        If _lsp Is Nothing Then
            _lsp = New Lazy(Of RsharpLspServer)(Function() New RsharpLspServer(Nothing, Nothing, New DebugTextWriter(_lsplog)))
        End If
    End Sub

    Public Async Function Init() As Task(Of lspclient)
        Return Await Task.Run(Function()
                                  Try
                                      Dim server = _lsp.Value

                                      _lsplog("[rsharp-lsp] server ready: " & server.GetType.Name)
                                  Catch ex As Exception
                                      _lsplog("[rsharp-lsp] warmup failed: " & ex.Message)
                                  End Try

                                  Return Me
                              End Function)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Async Function handleLspRequest(id As Long, method As String, paramsJSON As String) As Task(Of Dictionary(Of String, Object))
        Return Await CreateResponse(server:=_lsp.Value, id, method, paramsJSON)
    End Function

    Private Async Function CreateResponse(server As RsharpLspServer, id As Long, method As String, paramsJSON As String) As Task(Of Dictionary(Of String, Object))
        Dim reply As Dictionary(Of String, Object)

        Call _lspLock.Wait()

        Try
            Dim req = JsonRpc.MakeRequest(CInt(id), method, JsonElement.ParseJSON(paramsJSON))
            Dim resp = Await server.HandleMessageAsync(req)
            Dim resultNode As Object = Nothing
            Dim resEl = resp("result")

            ' pass the raw json element straight through: rebuilding
            ' it from a dictionary would expose the LSP keys to the
            ' serializer naming policy
            If resEl IsNot Nothing Then
                Using doc As System.Text.Json.JsonDocument = System.Text.Json.JsonDocument.Parse(resEl.BuildJsonString)
                    resultNode = doc.RootElement.Clone()
                End Using
            End If

            reply = New Dictionary(Of String, Object) From {
                {"id", id},
                {"result", resultNode}
            }
        Catch ex As Exception
            reply = New Dictionary(Of String, Object) From {{"id", id}, {"error", ex.Message}}
        Finally
            Call _lspLock.Release()
        End Try

        Return reply
    End Function

    Public Async Sub handleLspNotification(method As String, paramsJSON As String)
        Dim server = _lsp.Value

        Call _lspLock.Wait()

        Try
            Dim note = JsonRpc.MakeNotification(method, JsonElement.ParseJSON(paramsJSON))
            Dim parsed = Await server.HandleMessageAsync(note)

            Call _lsplog($"lsp/get_notify: {parsed.BuildJsonString}")
        Catch ex As Exception
            Call _lsplog("[rsharp-lsp] notify error: " & ex.Message)
        Finally
            Call _lspLock.Release()
        End Try
    End Sub

    ''' <summary>
    ''' routes the language server log to the debugger output. a GUI host has no
    ''' console attached, so the default stderr sink is useless (and may not even
    ''' be openable).
    ''' </summary>
    Private Class DebugTextWriter
        Inherits TextWriter

        Public Overrides ReadOnly Property Encoding As Encoding
            Get
                Return Encoding.UTF8
            End Get
        End Property

        ReadOnly _log As Action(Of String)

        Sub New()
            _log = AddressOf System.Diagnostics.Debug.WriteLine
        End Sub

        Sub New(log As Action(Of String))
            _log = log
        End Sub

        Public Overrides Sub Write(value As String)
            _log(value)
        End Sub

        Public Overrides Sub WriteLine(value As String)
            _log(value)
        End Sub
    End Class
End Class
