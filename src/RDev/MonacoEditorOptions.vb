''' <summary>
''' Canonical Monaco editor option keys (camelCase), matching the names accepted by
''' <c>editor.updateOptions(...)</c> on the frontend (see editor.js handleUpdateOption).
'''
''' Use these constants as the first argument of <see cref="VsCodeEditor.UpdateOption"/>,
''' e.g. <c>editor.UpdateOption(MonacoEditorOptions.TabSize, 4)</c>, so option names are
''' discoverable through IntelliSense and free of typos. The second argument (value) must
''' match the documented type / allowed values shown in each member's summary.
'''
''' Reference: Monaco <c>IEditorOptions</c> (https://microsoft.github.io/monaco-editor/docs.html).
''' </summary>

Public Module MonacoEditorOptions

#Region "Text & Indentation"
    ''' <summary>number of spaces per tab. value: integer (e.g. 4)</summary>
    Public Const TabSize As String = "tabSize"
    ''' <summary>size of indentation in spaces, or the literal "tabSize" to follow TabSize. value: integer | "tabSize"</summary>
    Public Const IndentSize As String = "indentSize"
    ''' <summary>insert spaces when pressing Tab. value: boolean</summary>
    Public Const InsertSpaces As String = "insertSpaces"
    ''' <summary>auto-detect indentation from content. value: boolean</summary>
    Public Const DetectIndentation As String = "detectIndentation"
    ''' <summary>remove auto-inserted whitespace on empty lines. value: boolean</summary>
    Public Const TrimAutoWhitespace As String = "trimAutoWhitespace"
    ''' <summary>word-based completion source. value: 'off' | 'currentDocument' | 'matchingDocuments' | 'allDocuments'</summary>
    Public Const WordBasedSuggestions As String = "wordBasedSuggestions"
    ''' <summary>restrict word-based suggestions to the same language. value: boolean</summary>
    Public Const WordBasedSuggestionsOnlySameLanguage As String = "wordBasedSuggestionsOnlySameLanguage"
    ''' <summary>keep peek editors open when navigating. value: boolean</summary>
    Public Const StablePeek As String = "stablePeek"
    ''' <summary>max line length tokenized synchronously. value: integer</summary>
    Public Const MaxTokenizationLineLength As String = "maxTokenizationLineLength"
#End Region

#Region "Font & Typography"
    ''' <summary>font family CSS value. value: string (e.g. "'Cascadia Code', Consolas, monospace")</summary>
    Public Const FontFamily As String = "fontFamily"
    ''' <summary>font size in pixels. value: integer (e.g. 14)</summary>
    Public Const FontSize As String = "fontSize"
    ''' <summary>font weight. value: string ('normal' | 'bold' | '100'..'900')</summary>
    Public Const FontWeight As String = "fontWeight"
    ''' <summary>enable font ligatures. value: boolean | string (e.g. "'calt', 'liga'")</summary>
    Public Const FontLigatures As String = "fontLigatures"
    ''' <summary>line height in pixels; 0 = auto. value: integer</summary>
    Public Const LineHeight As String = "lineHeight"
    ''' <summary>letter spacing in pixels. value: integer</summary>
    Public Const LetterSpacing As String = "letterSpacing"
#End Region

#Region "Layout & Scrolling"
    ''' <summary>allow scrolling past the last line. value: boolean</summary>
    Public Const ScrollBeyondLastLine As String = "scrollBeyondLastLine"
    ''' <summary>extra scrollable columns past the end of a line. value: integer</summary>
    Public Const ScrollBeyondLastColumn As String = "scrollBeyondLastColumn"
    ''' <summary>animated scrolling. value: boolean</summary>
    Public Const SmoothScrolling As String = "smoothScrolling"
    ''' <summary>zoom editor with Ctrl/Cmd + mouse wheel. value: boolean</summary>
    Public Const MouseWheelZoom As String = "mouseWheelZoom"
    ''' <summary>mouse wheel scroll multiplier. value: double</summary>
    Public Const MouseWheelScrollSensitivity As String = "mouseWheelScrollSensitivity"
    ''' <summary>fast (wheel) scroll multiplier. value: double</summary>
    Public Const FastScrollSensitivity As String = "fastScrollSensitivity"
    ''' <summary>scroll along the predominant axis only. value: boolean</summary>
    Public Const ScrollPredominantAxis As String = "scrollPredominantAxis"
    ''' <summary>number of overview ruler lanes. value: integer</summary>
    Public Const OverviewRulerLanes As String = "overviewRulerLanes"
    ''' <summary>draw a border around the overview ruler. value: boolean</summary>
    Public Const OverviewRulerBorder As String = "overviewRulerBorder"
    ''' <summary>hide the cursor in the overview ruler. value: boolean</summary>
    Public Const HideCursorInOverviewRuler As String = "hideCursorInOverviewRuler"
    ''' <summary>top/bottom inner padding. value: object { top: integer, bottom: integer } (Dictionary)</summary>
    Public Const Padding As String = "padding"
    ''' <summary>soft-wrap long lines. value: 'off' | 'on' | 'wordWrapColumn' | 'bounded'</summary>
    Public Const WordWrap As String = "wordWrap"
    ''' <summary>column at which to wrap when wordWrap is 'wordWrapColumn' or 'bounded'. value: integer</summary>
    Public Const WordWrapColumn As String = "wordWrapColumn"
    ''' <summary>indentation of wrapped lines. value: 'none' | 'same' | 'indent' | 'deepIndent'</summary>
    Public Const WrappingIndent As String = "wrappingIndent"
    ''' <summary>wrapping algorithm. value: 'simple' | 'advanced'</summary>
    Public Const WrappingStrategy As String = "wrappingStrategy"
#End Region

#Region "Minimap (nested object)"
    ''' <summary>minimap configuration. value: object { enabled: boolean, side: 'right'|'left', showSlider: 'always'|'mouseover', renderCharacters: boolean, maxColumn: integer, size: 'proportional'|'fill'|'fit', scale: integer }</summary>
    Public Const Minimap As String = "minimap"
#End Region

#Region "Cursor"
    ''' <summary>cursor blink style. value: 'blink' | 'smooth' | 'phase' | 'expand' | 'solid'</summary>
    Public Const CursorBlinking As String = "cursorBlinking"
    ''' <summary>smooth caret animation. value: 'off' | 'explicit' | 'on'</summary>
    Public Const CursorSmoothCaretAnimation As String = "cursorSmoothCaretAnimation"
    ''' <summary>cursor rendering style. value: 'line' | 'block' | 'underline' | 'line-thin' | 'block-outline' | 'underline-thin'</summary>
    Public Const CursorStyle As String = "cursorStyle"
    ''' <summary>cursor width in pixels (block styles). value: integer</summary>
    Public Const CursorWidth As String = "cursorWidth"
    ''' <summary>extra lines shown around the cursor. value: integer</summary>
    Public Const CursorSurroundingLines As String = "cursorSurroundingLines"
    ''' <summary>when to apply surrounding-lines padding. value: 'default' | 'all'</summary>
    Public Const CursorSurroundingLinesStyle As String = "cursorSurroundingLinesStyle"
    ''' <summary>modifier to add multiple cursors. value: 'ctrlCmd' | 'alt'</summary>
    Public Const MultiCursorModifier As String = "multiCursorModifier"
    ''' <summary>merge overlapping multi-cursor selections. value: boolean</summary>
    Public Const MultiCursorMergeOverlapping As String = "multiCursorMergeOverlapping"
#End Region

#Region "Line Numbers & Rulers"
    ''' <summary>line number rendering. value: 'on' | 'off' | 'relative' | 'interval'</summary>
    Public Const LineNumbers As String = "lineNumbers"
    ''' <summary>minimum characters reserved for line numbers. value: integer</summary>
    Public Const LineNumbersMinChars As String = "lineNumbersMinChars"
    ''' <summary>show the glyph margin (for breakpoints, etc.). value: boolean</summary>
    Public Const GlyphMargin As String = "glyphMargin"
    ''' <summary>width of the line-decorations area. value: integer | string (e.g. "10px")</summary>
    Public Const LineDecorationsWidth As String = "lineDecorationsWidth"
    ''' <summary>vertical rulers. value: array of integer | array of object { column: integer, color: string }</summary>
    Public Const Rulers As String = "rulers"
    ''' <summary>current-line highlight. value: 'none' | 'gutter' | 'line' | 'all'</summary>
    Public Const RenderLineHighlight As String = "renderLineHighlight"
    ''' <summary>only highlight the current line when the editor is focused. value: boolean</summary>
    Public Const RenderLineHighlightOnlyWhenFocus As String = "renderLineHighlightOnlyWhenFocus"
    ''' <summary>select text on line-number click. value: boolean</summary>
    Public Const SelectOnLineNumbers As String = "selectOnLineNumbers"
#End Region

#Region "Folding"
    ''' <summary>enable code folding. value: boolean</summary>
    Public Const Folding As String = "folding"
    ''' <summary>folding strategy. value: 'auto' | 'indentation'</summary>
    Public Const FoldingStrategy As String = "foldingStrategy"
    ''' <summary>highlight folded regions. value: boolean</summary>
    Public Const FoldingHighlight As String = "foldingHighlight"
    ''' <summary>when to show folding controls. value: 'always' | 'mouseover'</summary>
    Public Const ShowFoldingControls As String = "showFoldingControls"
    ''' <summary>fold import statements by default. value: boolean</summary>
    Public Const FoldingImportsByDefault As String = "foldingImportsByDefault"
#End Region

#Region "Suggest / IntelliSense"
    ''' <summary>trigger suggestions on trigger characters. value: boolean</summary>
    Public Const SuggestOnTriggerCharacters As String = "suggestOnTriggerCharacters"
    ''' <summary>enable quick suggestions. value: boolean | object { other: boolean, comments: boolean, strings: boolean }</summary>
    Public Const QuickSuggestions As String = "quickSuggestions"
    ''' <summary>delay before quick suggestions appear. value: integer (ms)</summary>
    Public Const QuickSuggestionsDelay As String = "quickSuggestionsDelay"
    ''' <summary>default item selected in the suggest widget. value: 'first' | 'recentlyUsed' | 'recentlyUsedByPrefix'</summary>
    Public Const SuggestSelection As String = "suggestSelection"
    ''' <summary>snippet suggestion ordering. value: 'top' | 'bottom' | 'inline' | 'none'</summary>
    Public Const SnippetSuggestions As String = "snippetSuggestions"
    ''' <summary>enable tab completion. value: 'on' | 'off' | 'onlySnippets'</summary>
    Public Const TabCompletion As String = "tabCompletion"
    ''' <summary>show inline suggestions. value: boolean | object { enabled: boolean, showToolbar: 'always'|'onHover' }</summary>
    Public Const InlineSuggest As String = "inlineSuggest"
    ''' <summary>font size of the suggest widget. value: integer</summary>
    Public Const SuggestFontSize As String = "suggestFontSize"
    ''' <summary>line height of the suggest widget. value: integer</summary>
    Public Const SuggestLineHeight As String = "suggestLineHeight"
#End Region

#Region "Behavior & Editing"
    ''' <summary>make the editor read-only. value: boolean (also exposed via VsCodeEditor.SetReadOnly)</summary>
    Public Const ReadOnlyOption As String = "readOnly"
    ''' <summary>mark the DOM as read-only (no textarea editing). value: boolean</summary>
    Public Const DomReadOnly As String = "domReadOnly"
    ''' <summary>auto-close brackets. value: 'always' | 'languageDefined' | 'beforeWhitespace' | 'never'</summary>
    Public Const AutoClosingBrackets As String = "autoClosingBrackets"
    ''' <summary>auto-delete closing pairs. value: 'always' | 'auto' | 'never'</summary>
    Public Const AutoClosingDelete As String = "autoClosingDelete"
    ''' <summary>auto-overtype closing pairs. value: 'always' | 'auto' | 'never'</summary>
    Public Const AutoClosingOvertype As String = "autoClosingOvertype"
    ''' <summary>auto-close quotes. value: 'always' | 'languageDefined' | 'beforeWhitespace' | 'never'</summary>
    Public Const AutoClosingQuotes As String = "autoClosingQuotes"
    ''' <summary>auto-surround selections with quotes/brackets. value: 'languageDefined' | 'quotes' | 'brackets' | 'never'</summary>
    Public Const AutoSurround As String = "autoSurround"
    ''' <summary>format on paste. value: boolean</summary>
    Public Const FormatOnPaste As String = "formatOnPaste"
    ''' <summary>format as you type. value: boolean</summary>
    Public Const FormatOnType As String = "formatOnType"
    ''' <summary>enable drag &amp; drop of text. value: boolean</summary>
    Public Const DragAndDrop As String = "dragAndDrop"
    ''' <summary>adapt to OS high-contrast mode. value: boolean</summary>
    Public Const AutoDetectHighContrast As String = "autoDetectHighContrast"
    ''' <summary>show the context menu. value: boolean</summary>
    Public Const ContextMenu As String = "contextMenu"
    ''' <summary>render clickable links. value: boolean</summary>
    Public Const Links As String = "links"
    ''' <summary>bracket matching highlight. value: 'never' | 'near' | 'always'</summary>
    Public Const MatchBrackets As String = "matchBrackets"
    ''' <summary>characters treated as word separators. value: string</summary>
    Public Const WordSeparators As String = "wordSeparators"
    ''' <summary>use tab stops when indenting. value: boolean</summary>
    Public Const UseTabStops As String = "useTabStops"
    ''' <summary>copy includes syntax highlighting HTML. value: boolean</summary>
    Public Const CopyWithSyntaxHighlighting As String = "copyWithSyntaxHighlighting"
    ''' <summary>rounded selection corners. value: boolean</summary>
    Public Const RoundedSelection As String = "roundedSelection"
    ''' <summary>highlight the current selection. value: boolean</summary>
    Public Const SelectionHighlight As String = "selectionHighlight"
    ''' <summary>highlight word occurrences. value: 'off' | 'singleFile' | 'multiFile'</summary>
    Public Const OccurrencesHighlight As String = "occurrencesHighlight"
#End Region

#Region "Appearance & Rendering"
    ''' <summary>editor color theme. value: string (e.g. 'vs' | 'vs-dark' | 'hc-black', or a registered theme) (also VsCodeEditor.SetTheme)</summary>
    Public Const Theme As String = "theme"
    ''' <summary>render whitespace characters. value: 'none' | 'boundary' | 'selection' | 'trailing' | 'all'</summary>
    Public Const RenderWhitespace As String = "renderWhitespace"
    ''' <summary>render control characters. value: boolean</summary>
    Public Const RenderControlCharacters As String = "renderControlCharacters"
    ''' <summary>render validation decorations. value: 'on' | 'off' | 'editable'</summary>
    Public Const RenderValidationDecorations As String = "renderValidationDecorations"
    ''' <summary>render a final newline when the model ends without one. value: boolean</summary>
    Public Const RenderFinalNewline As String = "renderFinalNewline"
    ''' <summary>render color decorators. value: boolean</summary>
    Public Const ColorDecorators As String = "colorDecorators"
    ''' <summary>indent guides. value: object { indentation: boolean, bracketPairs: boolean|'active', highlightActiveIndentation: boolean }</summary>
    Public Const Guides As String = "guides"
    ''' <summary>bracket pair colorization. value: object { enabled: boolean }</summary>
    Public Const BracketPairColorization As String = "bracketPairColorization"
    ''' <summary>dim unused variables/labels. value: boolean</summary>
    Public Const ShowUnused As String = "showUnused"
    ''' <summary>strikethrough deprecated symbols. value: boolean</summary>
    Public Const ShowDeprecated As String = "showDeprecated"
    ''' <summary>disable monospace rendering optimizations. value: boolean</summary>
    Public Const DisableMonospaceOptimizations As String = "disableMonospaceOptimizations"
    ''' <summary>keep overflowing widgets inside the editor. value: boolean</summary>
    Public Const FixedOverflowWidgets As String = "fixedOverflowWidgets"
    ''' <summary>extra CSS class on the editor container. value: string</summary>
    Public Const ExtraEditorClassName As String = "extraEditorClassName"
#End Region

#Region "Advanced nested objects (value = Dictionary/JsonElement)"
    ''' <summary>scrollbar configuration. value: object { vertical: 'auto'|'visible'|'hidden', horizontal, useShadows, handleMouseWheel, alwaysConsumeMouseWheel, horizontalScrollbarSize, verticalScrollbarSize, ... }</summary>
    Public Const Scrollbar As String = "scrollbar"
    ''' <summary>hover behavior. value: boolean | object { enabled: boolean, delay: integer, sticky: boolean }</summary>
    Public Const Hover As String = "hover"
    ''' <summary>full suggest-widget configuration. value: object (filterGraceful, snippetsPreventQuickSuggestions, preview, showIcons, maxVisibleSuggestions, showMethods, showFunctions, ...)</summary>
    Public Const Suggest As String = "suggest"
    ''' <summary>find widget configuration. value: object { addExtraSpaceOnTop: boolean, autoFindInSelection: 'never'|'always'|'multiline', seedSearchStringFromSelection: 'always'|'selection'|'never' }</summary>
    Public Const Find As String = "find"
    ''' <summary>go-to-location behavior. value: object { multiple: 'peek'|'gotoAndPeek'|'goto', alternativeDefinitionCommand, ... }</summary>
    Public Const GotoLocation As String = "gotoLocation"
    ''' <summary>lightbulb (quick fixes) behavior. value: boolean | object { enabled: boolean }</summary>
    Public Const Lightbulb As String = "lightbulb"
    ''' <summary>parameter hints behavior. value: boolean | object { enabled: boolean }</summary>
    Public Const ParameterHints As String = "parameterHints"
    ''' <summary>sticky scroll behavior. value: boolean | object { enabled: boolean, maxLineCount: integer }</summary>
    Public Const StickyScroll As String = "stickyScroll"
    ''' <summary>unicode highlighting. value: boolean | object { ambiguousCharacters: boolean, invisibleCharacters: boolean, nonBasicASCII: boolean }</summary>
    Public Const UnicodeHighlighting As String = "unicodeHighlighting"
    ''' <summary>inlay hints behavior. value: boolean | object { enabled: boolean, fontSize: integer, fontFamily: string, padding: boolean }</summary>
    Public Const InlayHints As String = "inlayHints"
    ''' <summary>escape hatch for experimental editor features. value: object</summary>
    Public Const Experimental As String = "experimental"
#End Region

End Module

''' <summary>
''' strongly-typed word-wrap modes for <see cref="VsCodeEditor.SetWordWrap"/>,
''' mapping to Monaco's wordWrap values.
''' </summary>
Public Enum WordWrapMode
    Off
    [On]
    WordWrapColumn
    Bounded
End Enum

''' <summary>
''' strongly-typed line-number modes for <see cref="VsCodeEditor.SetLineNumbers"/>,
''' mapping to Monaco's lineNumbers values.
''' </summary>
Public Enum LineNumbersMode
    [On]
    Off
    Relative
    Interval
End Enum
