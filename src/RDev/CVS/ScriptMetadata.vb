Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Development.Package.File
Imports RScript = SMRUCC.Rsharp.Runtime.Components.Rscript

Namespace CVS

    ''' <summary>
    ''' analyse the R# script text for gets the metadata and the commandline
    ''' arguments of the target script.
    ''' </summary>
    ''' <remarks>
    ''' the parser is implemented based on the ``ShellScript`` module of the R#
    ''' interpreter, which reads the script header comment block and the
    ''' ``?'argument'`` commandline getter expression inside the script.
    ''' </remarks>
    Public Module ScriptMetadataParser

        ''' <summary>
        ''' <see cref="ScriptMetadata.parseError"/>
        ''' </summary>
        Public Function Parse(code As String) As ScriptMetadata
            Dim meta As ScriptMetadata = ScriptMetadata.Empty()

            If code Is Nothing Then
                Return meta
            End If

            Dim fallback As Boolean = False

            Try
                Dim script As RScript = RScript.FromText(code)
                Dim shell As New ShellScript(script)
                Dim doc As CommandLineDocument = shell.AnalysisAllCommands().GetDocument()

                If doc Is Nothing Then
                    fallback = True
                Else
                    meta.title = If(doc.title, "")
                    meta.description = If(doc.info, "")
                    meta.authors = If(doc.authors, {}).ToArray()
                    meta.dependencies = DependencyList(doc.dependency)
                    meta.arguments = ArgumentList(doc.arguments)
                End If
            Catch ex As Exception
                meta.parseError = ex.Message
                fallback = True
            End Try

            If fallback Then
                Call FallbackMeta(code, meta)
            End If

            If meta.title.StringEmpty Then
                meta.title = "<unnamed script>"
            End If

            Return meta
        End Function

        ''' <summary>
        ''' the R# package dependency of the script, in the format
        ''' ``library`` or ``library::package``
        ''' </summary>
        Private Function DependencyList(deps As Dependency()) As String()
            If deps Is Nothing Then
                Return {}
            End If

            Dim list As New List(Of String)

            For Each dep As Dependency In deps
                If dep Is Nothing Then
                    Continue For
                End If

                Dim libname As String = If(dep.library, "")

                If Not dep.packages Is Nothing Then
                    For Each pkg As String In dep.packages
                        If pkg.StringEmpty Then
                            Continue For
                        End If
                        If libname.StringEmpty Then
                            list.Add(pkg)
                        Else
                            list.Add($"{libname}::{pkg}")
                        End If
                    Next
                ElseIf Not libname.StringEmpty Then
                    list.Add(libname)
                End If
            Next

            Return list.Distinct().ToArray()
        End Function

        Private Function ArgumentList(args As CommandLineArgument()) As ArgumentSpec()
            If args Is Nothing Then
                Return {}
            End If

            Dim list As New List(Of ArgumentSpec)

            For Each arg As CommandLineArgument In args
                If arg Is Nothing OrElse arg.name.StringEmpty Then
                    Continue For
                End If

                Dim def As String = If(arg.defaultValue, "")
                Dim required As Boolean = def.StartsWith("<required")

                list.Add(New ArgumentSpec With {
                    .name = arg.name,
                    .type = If(arg.type, "string"),
                    .defaultValue = If(required, "", TrimQuotes(def)),
                    .description = If(arg.description, ""),
                    .required = required,
                    .isLiteral = arg.isLiteral
                })
            Next

            Return list.ToArray()
        End Function

        Private Function TrimQuotes(text As String) As String
            If text Is Nothing Then
                Return ""
            End If

            Return text.Trim(""""c, "'"c)
        End Function

        ''' <summary>
        ''' the script can not be parsed by the R# interpreter (syntax error, or
        ''' the script is under editing), so just read the header comment block
        ''' as the metadata of current script.
        ''' </summary>
        Private Sub FallbackMeta(code As String, meta As ScriptMetadata)
            Dim lines As String() = code.LineTokens
            Dim comment As New List(Of String)

            For Each line As String In lines
                Dim text As String = line.Trim

                If text.StringEmpty Then
                    If comment.Count = 0 Then
                        Continue For
                    Else
                        Exit For
                    End If
                End If
                If Not (text.StartsWith("#") OrElse text.StartsWith("'")) Then
                    Exit For
                End If

                Call comment.Add(text.Trim(" "c, "#"c, "'"c))
            Next

            Dim title As String = Nothing
            Dim desc As New List(Of String)

            For Each line As String In comment
                Dim tag As String = Nothing
                Dim value As String = line

                If Regex.Match(line, "^\s*@\w+").Success Then
                    Dim parts = line.GetTagValue(" ", trim:=True)

                    tag = parts.Name.Trim("@"c).ToLower()
                    value = parts.Value
                End If

                Select Case tag
                    Case "title" : title = value
                    Case "description", "details", "info" : desc.Add(value)
                    Case "author", "authors"
                        meta.authors = value.StringSplit("\s*[,;]\s*")
                    Case Else
                        If title Is Nothing Then
                            title = line
                        Else
                            desc.Add(line)
                        End If
                End Select
            Next

            If Not title Is Nothing Then
                meta.title = title.Trim
            End If
            If desc.Count > 0 AndAlso meta.description.StringEmpty Then
                meta.description = desc.JoinBy(vbLf).Trim
            End If
        End Sub
    End Module
End Namespace
