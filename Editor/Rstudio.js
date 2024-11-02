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

const text = http_get("http://a.com");

print(text);
`;
    function getCodeText() {
        return editor.getModel().getValue();
    }
    rstudio.getCodeText = getCodeText;
    function create() {
        // create demo test
        create_editor(demo_r, 'r');
    }
    rstudio.create = create;
    function create_editor(script, lang) {
        let container = $ts('#container');
        key = md5(script);
        editor = monaco.editor.create(container, {
            value: script,
            language: lang,
            automaticLayout: true,
            glyphMargin: true,
            lightbulb: {
                enabled: monaco.editor.ShowLightbulbIconMode.On
            }
        });
        auto_commit();
    }
    function hashkey() {
        return key;
    }
    rstudio.hashkey = hashkey;
    /**
     * auto commit the script updates to server
    */
    function auto_commit() {
        editor.onDidChangeModelContent((event) => {
            // save to server
            lsp.put_script(getCodeText(), key);
        });
        // initialize of the server environment
        lsp.put_script(demo_r, key);
    }
    function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: (model, position) => rstudio.tooltip.create_tooltip(model, position)
        });
        monaco.languages.registerCompletionItemProvider('r', {
            provideCompletionItems: (model, position) => rstudio.intellisense.create_intellisense(model, position)
        });
    }
    rstudio.setup = setup;
})(rstudio || (rstudio = {}));
/// <reference path="../vscode/monaco.d.ts" />
/// <reference path="../linq.d.ts" />
/// <reference path="./editor.ts" />
function run_vscode() {
    const require = window.require;
    // run the vscode
    require.config({ paths: { vs: './vscode/min/vs' } });
    require(['vs/editor/editor.main'], function () {
        rstudio.setup();
        rstudio.create();
    });
}
$ts(run_vscode);
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
        return fetch(url("/lsp/get/symbol", document) + `&symbol=${symbol}`).then((response) => {
            return response.text();
        });
    }
    lsp.get_symbol_info = get_symbol_info;
    function get_function_symbols() {
        return fetch(url("/lsp/get/functions", "")).then((response) => {
            return response.json();
        });
    }
    lsp.get_function_symbols = get_function_symbols;
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
            let internal = createDependencyProposals(range, word);
            return fetchSymbols(range, word).then(list => {
                return {
                    suggestions: internal.concat(list)
                };
            });
        }
        intellisense.create_intellisense = create_intellisense;
        intellisense.r_keywords = [
            'require', 'library', 'if', 'for', 'return', 'function', 'let', 'const', 'imports', 'from', 'next', 'break', 'while', 'else', 'step'
        ];
        intellisense.r_primitive = [
            'c', 'list', 'str', 'print', 'example', "is.null", "length", "readLines", "writeLines", "help", "as.character", "as.numeric", "readBin", "writeBin", "as.logical"
        ];
        intellisense.r_const = [
            'TRUE', 'FALSE', 'true', 'false', 'NULL', 'NA', 'Inf', 'NaN', 'PI'
        ];
        function fetchSymbols(range, curWord) {
            return lsp.get_function_symbols().then((names) => {
                let funcs = [];
                for (let f of names) {
                    funcs.push({
                        label: f,
                        kind: monaco.languages.CompletionItemKind.Function,
                        documentation: "",
                        insertText: f,
                        range: range
                    });
                }
                return funcs;
            });
        }
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
            for (const item of intellisense.r_primitive) {
                keys.push({
                    label: item,
                    kind: monaco.languages.CompletionItemKind.Function,
                    documentation: "",
                    insertText: item,
                    range: range
                });
            }
            for (const item of intellisense.r_const) {
                keys.push({
                    label: item,
                    kind: monaco.languages.CompletionItemKind.Constant,
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
                return resolveTooltip(word, position);
            }
        }
        tooltip_1.create_tooltip = create_tooltip;
        function resolveTooltip(word, position) {
            // 根据单词显示自定义提示
            return contentHtml(word.word).then(str => {
                const htmlContent = {
                    supportHtml: true,
                    value: str
                };
                const hover = {
                    range: new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn),
                    contents: [htmlContent]
                };
                if (!str) {
                    return null;
                }
                else {
                    return hover;
                }
            });
        }
        function contentHtml(word) {
            if (word in tooltip_1.keywords) {
                return Promise.resolve(tooltip_1.keywords[word]);
            }
            else {
                return lsp.get_symbol_info(rstudio.hashkey(), null, word);
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
        tooltip_1.logical_keyword = tooltip("Logical Vectors", `Create or test for objects of type 'logical', and the basic logical constants.
TRUE and FALSE are reserved words denoting logical constants in the R language, whereas T and F are global variables whose initial values set to these. 
All four are logical(1) vectors.
Logical vectors are coerced to integer vectors in contexts where a numerical value is required, with TRUE being mapped to 1L, FALSE to 0L and NA to NA_integer_.`);
        tooltip_1.let_keyword = tooltip("Create new symbol", "Create a new symbol in current environment frame.");
        tooltip_1.const_keyword = tooltip("Create new symbol", "Create a new symbol with lock binding(which means the symbol value could not be changed) in current environment frame.");
        tooltip_1.from_keyword = tooltip("The .NET clr module source", `The .NET clr package module imports source assembly name, usually be the assembly file base name. 
        Assembly file name with dll extension suffix or the full file path to the dll assembly file also could be accepted.`);
        tooltip_1.function_keyword = tooltip("Define a function", "Define a function in R# runtime, a function is kind of expression collection with parameter accept and value returns to its caller.");
        tooltip_1.if_keyword = tooltip("Control Flow", `These are the basic control-flow constructs of the R language. They function in much the same way as control statements in any Algol-like language. They are all reserved words.
if returns the value of the expression evaluated, or NULL invisibly if none was (which may happen if there is no else).`);
        tooltip_1.for_keyword = tooltip("Control Flow", `These are the basic control-flow constructs of the R language. They function in much the same way as control statements in any Algol-like language. They are all reserved words.
for, while and repeat return NULL invisibly. for sets var to the last used element of seq, or to NULL if it was of length zero.`);
        tooltip_1.step_keyword = tooltip("Set the numeric sequence generator steps", `This keyword controls the different of the generated numeric sequence, default step is 1 for the numeric sequence, example as: <code>1:5</code>; 
        you could use <code>step</code> keyword for produce a sequence with different 0.5: <code>1:5 step 0.5</code>.`);
        tooltip_1.null_keyword = tooltip("The Null Object", `NULL represents the null object in R: it is a reserved word. NULL is often returned by expressions and functions whose value is undefined.
        NULL can be indexed (see Extract) in just about any syntactically legal way: apart from NULL[[]] which is an error, the result is always NULL. 
        Objects with value NULL can be changed by replacement operators and will be coerced to the type of the right-hand side.

NULL is also used as the empty pairlist: see the examples. Because pairlists are often promoted to lists, you may encounter NULL being promoted to an empty list.

Objects with value NULL cannot have attributes as there is only one null object: attempts to assign them are either an error (attr) or promote the object to an empty list with attribute(s) (attributes and structure).`);
        tooltip_1.keywords = {
            "imports": tooltip_1.imports_keyword,
            'return': tooltip_1.return_keyword,
            'TRUE': tooltip_1.logical_keyword,
            'FALSE': tooltip_1.logical_keyword,
            "let": tooltip_1.let_keyword,
            "const": tooltip_1.const_keyword,
            "from": tooltip_1.from_keyword,
            "function": tooltip_1.function_keyword,
            "if": tooltip_1.if_keyword,
            "for": tooltip_1.for_keyword,
            "step": tooltip_1.step_keyword,
            "NULL": tooltip_1.null_keyword
        };
    })(tooltip = rstudio.tooltip || (rstudio.tooltip = {}));
})(rstudio || (rstudio = {}));
//# sourceMappingURL=Rstudio.js.map