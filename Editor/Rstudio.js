var rstudio;
(function (rstudio) {
    /**
     * the language editor core
    */
    let editor;
    let key;
    let demo_r = `    
imports "JSON" from "base";
        
let f(x) = console.log("Hello world!");
let hello_world = function(x) {
    return \`hello \${x}!\`;
};

print(c(1,2,3,4,5));
str(list(
    a = 123,
    b = [TRUE, TRUE, FALSE],
    c = "XXX"
));
`;
    function getCodeText() {
        return editor.getModel().getValue();
    }
    rstudio.getCodeText = getCodeText;
    function create() {
        let container = $ts('#container');
        key = md5(demo_r);
        editor = monaco.editor.create(container, {
            value: demo_r,
            language: 'r',
            automaticLayout: true,
            glyphMargin: true,
            lightbulb: {
                enabled: monaco.editor.ShowLightbulbIconMode.On
            }
        });
        // initialize of the server environment
        lsp.put_script(demo_r, key);
    }
    rstudio.create = create;
    function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: (model, position) => rstudio.tooltip.create_tooltip(model, position)
        });
        monaco.languages.registerCompletionItemProvider('r', {
            provideCompletionItems: (model, position) => rstudio.intellisense.create_intellisense(model, position)
        });
        editor.onDidChangeModelContent((event) => {
            // save to server
            lsp.put_script(editor.getValue(), key);
        });
    }
    rstudio.setup = setup;
})(rstudio || (rstudio = {}));
/// <reference path="../vscode/monaco.d.ts" />
/// <reference path="../linq.d.ts" />
/// <reference path="./editor.ts" />
// run the vscode
require.config({ paths: { vs: './vscode/min/vs' } });
require(['vs/editor/editor.main'], function () {
    rstudio.setup();
    rstudio.create();
});
var lsp;
(function (lsp) {
    let ErrorCodes;
    (function (ErrorCodes) {
        // Defined by JSON-RPC
        ErrorCodes.ParseError = -32700;
        ErrorCodes.InvalidRequest = -32600;
        ErrorCodes.MethodNotFound = -32601;
        ErrorCodes.InvalidParams = -32602;
        ErrorCodes.InternalError = -32603;
        /**
         * This is the start range of JSON-RPC reserved error codes.
         * It doesn't denote a real error code. No LSP error codes should
         * be defined between the start and end range. For backwards
         * compatibility the `ServerNotInitialized` and the `UnknownErrorCode`
         * are left in the range.
         *
         * @since 3.16.0
         */
        ErrorCodes.jsonrpcReservedErrorRangeStart = -32099;
        /** @deprecated use jsonrpcReservedErrorRangeStart */
        ErrorCodes.serverErrorStart = ErrorCodes.jsonrpcReservedErrorRangeStart;
        /**
         * Error code indicating that a server received a notification or
         * request before the server has received the `initialize` request.
         */
        ErrorCodes.ServerNotInitialized = -32002;
        ErrorCodes.UnknownErrorCode = -32001;
        /**
         * This is the end range of JSON-RPC reserved error codes.
         * It doesn't denote a real error code.
         *
         * @since 3.16.0
         */
        ErrorCodes.jsonrpcReservedErrorRangeEnd = -32000;
        /** @deprecated use jsonrpcReservedErrorRangeEnd */
        ErrorCodes.serverErrorEnd = ErrorCodes.jsonrpcReservedErrorRangeEnd;
        /**
         * This is the start range of LSP reserved error codes.
         * It doesn't denote a real error code.
         *
         * @since 3.16.0
         */
        ErrorCodes.lspReservedErrorRangeStart = -32899;
        /**
         * A request failed but it was syntactically correct, e.g the
         * method name was known and the parameters were valid. The error
         * message should contain human readable information about why
         * the request failed.
         *
         * @since 3.17.0
         */
        ErrorCodes.RequestFailed = -32803;
        /**
         * The server cancelled the request. This error code should
         * only be used for requests that explicitly support being
         * server cancellable.
         *
         * @since 3.17.0
         */
        ErrorCodes.ServerCancelled = -32802;
        /**
         * The server detected that the content of a document got
         * modified outside normal conditions. A server should
         * NOT send this error code if it detects a content change
         * in it unprocessed messages. The result even computed
         * on an older state might still be useful for the client.
         *
         * If a client decides that a result is not of any use anymore
         * the client should cancel the request.
         */
        ErrorCodes.ContentModified = -32801;
        /**
         * The client has canceled a request and a server has detected
         * the cancel.
         */
        ErrorCodes.RequestCancelled = -32800;
        /**
         * This is the end range of LSP reserved error codes.
         * It doesn't denote a real error code.
         *
         * @since 3.16.0
         */
        ErrorCodes.lspReservedErrorRangeEnd = -32800;
    })(ErrorCodes = lsp.ErrorCodes || (lsp.ErrorCodes = {}));
})(lsp || (lsp = {}));
var lsp;
(function (lsp) {
    lsp.host = "";
    function url(api, key) {
        return `${lsp.host}${api}/?key=${key}`;
    }
    /**
     * get symbol information
     *
     * @param document the key that reference to the specific script document inside the server memory
     * @param offset the position offset on the script document
     * @param symbol the symbol name for get the information
    */
    function get_symbol_info(document, offset, symbol) {
        return fetch(url("/lsp/get/symbol", document)).then((response) => {
        });
    }
    lsp.get_symbol_info = get_symbol_info;
    /**
     * put script text into server memory
     *
     * @param key a hash key that could be used for make reference of this script text
    */
    function put_script(script_str, key) {
        let data = { doc: script_str };
        $ts.post(url("/lsp/put", key), data, (response) => {
        });
    }
    lsp.put_script = put_script;
    /**
     * commit the script document to the filesystem
     *
     * @param key a reference key that associated with a specific script text inside the server memory
     * @param path the local file path for save the script file, must be a local full path
    */
    function commit(key, path) {
        let data = { file: path };
        $ts.post(url("/lsp/save", key), data, (response) => {
        });
    }
    lsp.commit = commit;
})(lsp || (lsp = {}));
var rstudio;
(function (rstudio) {
    var intellisense;
    (function (intellisense) {
        function create_intellisense(model, position) {
            let word = model.getWordUntilPosition(position);
            let range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn
            };
            return {
                suggestions: createDependencyProposals(range, word)
            };
        }
        intellisense.create_intellisense = create_intellisense;
        intellisense.r_keywords = [
            'c', 'require', 'library', 'if', 'for', 'list', 'str', 'print', 'return', 'function', 'let', 'const', 'imports', 'from'
        ];
        function createDependencyProposals(range, curWord) {
            // snippets的定义同上
            // keys（泛指一切待补全的预定义词汇）的定义：
            let keys = [];
            let snippets = [];
            for (const item of intellisense.r_keywords) {
                keys.push({
                    label: item,
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    documentation: "",
                    insertText: item,
                    range: range
                });
            }
            return snippets.concat(keys);
        }
    })(intellisense = rstudio.intellisense || (rstudio.intellisense = {}));
})(rstudio || (rstudio = {}));
var rstudio;
(function (rstudio) {
    var tooltip;
    (function (tooltip_1) {
        function create_tooltip(model, position) {
            // 获取光标位置的单词
            const word = model.getWordAtPosition(position);
            if (!word) {
                return null;
            }
            else {
                return new Promise((resolve, reject) => {
                    resolveTooltip(word, position, resolve);
                });
            }
        }
        tooltip_1.create_tooltip = create_tooltip;
        function resolveTooltip(word, position, resolve) {
            // 根据单词显示自定义提示
            const hoverContent = contentHtml(word.word);
            const htmlContent = {
                supportHtml: true,
                value: hoverContent
            };
            const hover = {
                range: new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn),
                contents: [htmlContent]
            };
            if (!hoverContent) {
                resolve(null);
            }
            else {
                resolve(hover);
            }
        }
        function contentHtml(word) {
            if (word in tooltip_1.keywords) {
                return tooltip_1.keywords[word];
            }
            else {
                return null;
            }
        }
        tooltip_1.contentHtml = contentHtml;
        function tooltip(title, text) {
            return `<h3>${title}</h3><p>${text}</p>`;
        }
        tooltip_1.imports_keyword = tooltip('Loading/attaching of the .NET clr package module', 'similar to the <code>library</code> and <code>require</code> load and attach add-on .NET clr package modules.');
        tooltip_1.return_keyword = tooltip('Returns the function value to caller', `If value is missing, NULL is returned. If it is a single expression, the value of the evaluated expression is returned. 
(The expression is evaluated as soon as return is called, in the evaluation frame of the function and before any on.exit expression is evaluated.)
If the end of a function is reached without calling return, the value of the last evaluated expression is returned.`);
        tooltip_1.list_keyword = tooltip("Lists - Generic and Dotted Pairs", `Functions to construct, coerce and check for both kinds of <code>R</code> lists.`);
        tooltip_1.logical_keyword = tooltip("Logical Vectors", `Create or test for objects of type 'logical', and the basic logical constants.
TRUE and FALSE are reserved words denoting logical constants in the R language, whereas T and F are global variables whose initial values set to these. 
All four are logical(1) vectors.
Logical vectors are coerced to integer vectors in contexts where a numerical value is required, with TRUE being mapped to 1L, FALSE to 0L and NA to NA_integer_.`);
        tooltip_1.keywords = {
            "imports": tooltip_1.imports_keyword,
            'return': tooltip_1.return_keyword,
            'list': tooltip_1.list_keyword,
            'TRUE': tooltip_1.logical_keyword,
            'FALSE': tooltip_1.logical_keyword
        };
    })(tooltip = rstudio.tooltip || (rstudio.tooltip = {}));
})(rstudio || (rstudio = {}));
//# sourceMappingURL=Rstudio.js.map