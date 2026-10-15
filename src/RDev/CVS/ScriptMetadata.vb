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
            If meta.arguments Is Nothing OrElse meta.arguments.Length = 0 Then
                ' the R# parser is unavailable for current script (for example
                ' the script is under editing, or the R# runtime was compiled
                ' in debug mode), so just scan the commandline getter expression
                ' from the script text directly.
                meta.arguments = FallbackArguments(code)
            End If

            If meta.title.StringEmpty Then
                meta.title = "<unnamed script>"
            End If

            Return meta
        End Function

        ''' <summary>
        ''' the R# tokenizer of a debug compiled runtime throws a
        ''' ``NotImplementedException`` on the ``?"--argument"`` commandline
        ''' getter token, so the argument list is also scanned from the script
        ''' text with a regex pattern as the fallback solution.
        ''' </summary>
        ''' <remarks>
        ''' a VB module member can not be declared as Shared, so the compiled
        ''' pattern is declared as a plain module field here.
        ''' </remarks>
        Private ReadOnly argGetter As New Regex("\?\s*([""']?)(?<name>[\w\.\-/]+)\1", RegexOptions.Compiled)

        Private Function FallbackArguments(code As String) As ArgumentSpec()
            Dim list As New List(Of ArgumentSpec)
            Dim seen As New HashSet(Of String)

            For Each line As String In code.LineTokens
                Dim m As Match = argGetter.Match(line)

                If Not m.Success Then
                    Continue For
                End If

                Dim name As String = m.Groups("name").Value.Trim("-"c, "/"c)

                If name.StringEmpty OrElse seen.Contains(name) Then
                    Continue For
                End If

                Dim rest As String = line.Substring(m.Index + m.Length).Trim()
                Dim type As String = InferType(line, rest)
                Dim defaultValue As String = ""
                Dim required As Boolean = False

                seen.Add(name)

                If rest.StartsWith("||") Then
                    Dim expr As String = rest.Substring(2).Trim().TrimEnd(";"c).Trim()

                    If expr.StartsWith("stop(") Then
                        required = True
                    Else
                        defaultValue = TrimQuotes(expr)
                    End If
                End If

                list.Add(New ArgumentSpec With {
                    .name = name,
                    .type = type,
                    .defaultValue = defaultValue,
                    .description = "",
                    .required = required,
                    .isLiteral = False
                })
            Next

            Return list.ToArray()
        End Function

        ''' <summary>
        ''' guess the data type of one commandline argument from its declaration
        ''' expression or its default value literal
        ''' </summary>
        Private Function InferType(line As String, rest As String) As String
            Dim decl As Match = Regex.Match(line, "^\s*(let|const)\s+[\w\.]+\s+as\s+(?<type>\w+)")

            If decl.Success Then
                Select Case decl.Groups("type").Value.ToLower()
                    Case "integer", "int", "long" : Return "integer"
                    Case "double", "numeric", "single", "decimal" : Return "double"
                    Case "boolean", "logical", "bool" : Return "boolean"
                    Case Else : Return "string"
                End Select
            End If

            Dim def As Match = Regex.Match(rest, "\|\|\s*(?<v>.+?)\s*;?\s*$")
            Dim value As String = If(def.Success, def.Groups("v").Value.Trim(), "")

            If value.StringEmpty Then
                Return "string"
            End If
            If Regex.IsMatch(value, "^[""'].*[""']$") Then
                Return "string"
            End If
            If value.TextEquals("TRUE") OrElse value.TextEquals("FALSE") Then
                Return "boolean"
            End If
            If Regex.IsMatch(value, "^-?\d+$") Then
                Return "integer"
            End If
            If Regex.IsMatch(value, "^-?\d*\.\d+$") Then
                Return "double"
            End If

            Return "string"
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
