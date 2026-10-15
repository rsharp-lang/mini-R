''' <summary>
''' The editor interface of the R# script editor window of the host application.
''' </summary>
''' <remarks>
''' The script manager it self does not contains any code editor, the host
''' application creates the editor window (a winform form, for example the
''' monaco based <see cref="VsCodeEditor"/> document window) through the
''' <see cref="ScriptManagerView.OpenEditor"/> delegate of the script manager
''' control, and then the script manager drives the editor through the two
''' methods of this interface:
'''
''' 1. <see cref="LoadScript"/> loads the script text of one database version
'''    into the editor window;
''' 2. <see cref="SaveScript"/> asks the editor to save its content, and returns
'''    the current script text, so that the script manager can write the text
'''    back into the zip database as a new version.
''' </remarks>
Public Interface IEditor

    ''' <summary>
    ''' load the R# script text into the editor window
    ''' </summary>
    ''' <param name="code">
    ''' the R# script text of one version of the database
    ''' </param>
    ''' <param name="title">
    ''' an optional display title of current script, for example
    ''' ``demo.R [2026-10-16-001]``
    ''' </param>
    Sub LoadScript(code As String, Optional title As String = Nothing)

    ''' <summary>
    ''' save the script inside the editor window and then gets the current
    ''' script text of the editor.
    ''' </summary>
    ''' <returns>
    ''' the script text that is displayed inside the editor window; a nothing
    ''' or empty value means the save operation was cancelled (or failed), so
    ''' the caller should not create a new version in such situation.
    ''' </returns>
    Function SaveScript() As Task(Of String)

End Interface
