Imports System.Text.Json

Namespace CVS

    ''' <summary>
    ''' the message type of the host -> page direction
    ''' </summary>
    Public Class CvsIncoming

        Public Const Database As String = "Database"
        Public Const Scripts As String = "Scripts"
        Public Const Versions As String = "Versions"
        Public Const Runs As String = "Runs"
        Public Const VersionDetail As String = "VersionDetail"
        Public Const Output As String = "Output"
        Public Const RunResult As String = "RunResult"
        Public Const Status As String = "Status"
        ''' <summary>the generated change comment for the new version (host -> page)</summary>
        Public Const CommentSuggestion As String = "CommentSuggestion"
        ''' <summary>the full text search hits of the script code</summary>
        Public Const SearchResults As String = "SearchResults"
    End Class

    ''' <summary>
    ''' the message type of the page -> host direction
    ''' </summary>
    Public Class CvsOutgoing

        Public Const Ready As String = "Ready"
        Public Const NewScript As String = "NewScript"
        Public Const SelectScript As String = "SelectScript"
        Public Const SelectVersion As String = "SelectVersion"
        Public Const OpenInEditor As String = "OpenInEditor"
        Public Const SaveVersion As String = "SaveVersion"
        Public Const RunScript As String = "RunScript"
        Public Const DeleteScript As String = "DeleteScript"
        Public Const RequestOutput As String = "RequestOutput"
        ''' <summary>ask the host to generate a change comment for the new version via the LLM</summary>
        Public Const SuggestComment As String = "SuggestComment"
        ''' <summary>fuzzy search the script code, an empty query lists all scripts</summary>
        Public Const SearchScripts As String = "SearchScripts"
    End Class

    ''' <summary>
    ''' helper for read the member value from a json message payload
    ''' </summary>
    Public Module CvsPayload

        Public Function Str(payload As JsonElement, member As String, Optional [default] As String = Nothing) As String
            Dim el As JsonElement

            If payload.ValueKind <> JsonValueKind.Object Then
                Return [default]
            End If
            If Not payload.TryGetProperty(member, el) Then
                Return [default]
            End If
            If el.ValueKind = JsonValueKind.Null OrElse el.ValueKind = JsonValueKind.Undefined Then
                Return [default]
            End If

            Return el.ToString()
        End Function

        Public Function Bool(payload As JsonElement, member As String, Optional [default] As Boolean = False) As Boolean
            Dim el As JsonElement

            If payload.ValueKind <> JsonValueKind.Object Then
                Return [default]
            End If
            If Not payload.TryGetProperty(member, el) Then
                Return [default]
            End If

            Select Case el.ValueKind
                Case JsonValueKind.True : Return True
                Case JsonValueKind.False : Return False
                Case JsonValueKind.String
                    Dim text As String = el.GetString()

                    If text Is Nothing Then
                        Return [default]
                    End If

                    Return text.TextEquals("true") OrElse text = "1"
                Case JsonValueKind.Number
                    Return el.GetInt32() <> 0
                Case Else
                    Return [default]
            End Select
        End Function

        Public Function UInt(payload As JsonElement, member As String, Optional [default] As UInteger = 0) As UInteger
            Dim el As JsonElement

            If payload.ValueKind <> JsonValueKind.Object Then
                Return [default]
            End If
            If Not payload.TryGetProperty(member, el) Then
                Return [default]
            End If

            If el.ValueKind = JsonValueKind.Number Then
                Return CUInt(el.GetUInt32())
            ElseIf el.ValueKind = JsonValueKind.String Then
                Dim n As UInteger

                If UInteger.TryParse(el.GetString(), n) Then
                    Return n
                End If
            End If

            Return [default]
        End Function

        ''' <summary>
        ''' read the commandline arguments of the run task from the html form
        ''' </summary>
        Public Function Arguments(payload As JsonElement, member As String) As List(Of RunArgument)
            Dim list As New List(Of RunArgument)
            Dim el As JsonElement

            If payload.ValueKind <> JsonValueKind.Object Then
                Return list
            End If
            If Not payload.TryGetProperty(member, el) Then
                Return list
            End If
            If el.ValueKind <> JsonValueKind.Array Then
                Return list
            End If

            For Each item As JsonElement In el.EnumerateArray()
                Dim name As String = Str(item, "name")
                Dim value As String = Str(item, "value", "")

                If name.StringEmpty Then
                    Continue For
                End If

                list.Add(New RunArgument With {.name = name, .value = value})
            Next

            Return list
        End Function
    End Module
End Namespace
