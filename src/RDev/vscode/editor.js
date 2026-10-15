require.config({ paths: { vs: 'vs' } });

require(['vs/editor/editor.main'], function () {

    /* ==============================
       Message Contracts
    ============================== */

    const Incoming = {
        SetText: "SetText",
        SetReadOnly: "SetReadOnly",
        MarkAsSaved: "MarkAsSaved",
        GetLanguages: "GetLanguages",
        SetLanguage: "SetLanguage",
        UpdateOption: "UpdateOption",
        RegisterTheme: "RegisterTheme",
        SetTheme: "SetTheme",
        RegisterLanguage: "RegisterLanguage",
        // host -> page: settles the promise waiting on this lsp request id
        LspResponse: "lsp/response"
    };

    const Outgoing = {
        TextChanged: "TextChanged",
        IsDirtyChanged: "IsDirtyChanged",
        EditorReady: "EditorReady",
        Languages: "Languages",
        // page -> host -> R# language server (a reply is expected)
        LspRequest: "lsp/request",
        // page -> host -> R# language server (notification, no reply)
        LspNotify: "lsp/notify"
    };

    function send(type, payload) {
        window.chrome.webview.postMessage({ type, payload });
    }

    /* ==============================
       Editor Initialization
    ============================== */

    const editor = monaco.editor.create(
        document.getElementById('container'),
        {
            value: '',
            language: 'r',
            theme: 'vs'
        }
    );

    // expose the editor instance so the host-side bridge (window.setEditorContent /
    // window.getEditorContent) and any ExecuteScriptAsync calls can reach it.
    window.editor = editor;
    window.monaco = monaco;

    //the model with wich the editor works
    const model = editor.getModel();
    let baselineVersion = model.getAlternativeVersionId();
    let lastDirtyState = false;


    // === ResizeObserver ===
    const container = document.getElementById('container');

    const resizeObserver = new ResizeObserver(() => {
        editor.layout();
    });

    resizeObserver.observe(container);

    /* =====================================
       Internal State Logic
    ===================================== */

    function updateDirtyState() {
        // inform the WinForms part that the content was modified
        const currentVersion = model.getAlternativeVersionId();
        const isDirty = currentVersion !== baselineVersion;

        if (isDirty !== lastDirtyState) {

            lastDirtyState = isDirty;

            send(Outgoing.IsDirtyChanged, isDirty);
        }
    }

    let changeTimeout;

    function updateTextChanged() {
        clearTimeout(changeTimeout);

        changeTimeout = setTimeout(() => {
            send(Outgoing.TextChanged, editor.getValue());

            // keep the language server in sync with the (debounced) buffer
            syncDidChange();
        }, 150);

    }

    /* =====================================
       Incoming Handlers (Dispatcher)
    ===================================== */

    function handleSetText(text) {
        if (editor.getValue() !== text) {
            editor.setValue(text);
            //update scroll positions to the most left and top position
            editor.setScrollTop(0);
            editor.setScrollLeft(0);

            //Consider the new text as new and set the IsDirty flag to False.
            baselineVersion = model.getAlternativeVersionId();
            //inform WinForms that IsModified has changed
            updateDirtyState()
        }
    }

    function handleSetReadOnly(value) {
        editor.updateOptions({ readOnly: value });
    }

    function handleMarkAsSaved() {
        editor.pushUndoStop();
        baselineVersion = model.getAlternativeVersionId();
        updateDirtyState();
    }

    function handleGetLanguages() {
        const langs = monaco.languages.getLanguages().map(l => l.id);;
        send(Outgoing.Languages, langs);
    }

    function handleSetLanguage(id) {
        if (!model || !id)
            return;

        if (model.getLanguageId() === id)
            return;

        monaco.editor.setModelLanguage(model, id);
    }

    function handleUpdateOption(payload) {
        if (!editor || !payload?.option)
            return;

        editor.updateOptions({
            [payload.option]: payload.value
        });
    }

    function handleRegisterTheme(payload) {
        monaco.editor.defineTheme(payload.name, {
            base: payload.base,
            inherit: payload.inherit,
            rules: payload.rules,
            colors: payload.colors ?? {}
        });
    }

    function handleSetTheme(themeName) {
        monaco.editor.setTheme(themeName);
    }

    function evaluateJsObjectLiteral(code, errorMessage) {
        try {
            return new Function(
                `"use strict"; return (${code});`
            )();
        }
        catch (e) {
            console.error(errorMessage, e);
            throw e;
        }
    }

    function handleRegisterLanguage(payload) {

        if (!payload?.id)
            return;

        const id = payload.id;

        const exists = monaco.languages
            .getLanguages()
            .some(l => l.id === id);

        if (!exists) {
            monaco.languages.register({ id });
        }

        if (payload.monarch) {
            const definition = evaluateJsObjectLiteral(
                payload.monarch,
                "Failed to evaluate Monarch"
            );

            monaco.languages.setMonarchTokensProvider(id, definition);
        }

        if (payload.configuration) {
            const config = evaluateJsObjectLiteral(
                payload.configuration,
                "Failed to evaluate configuration"
            );

            monaco.languages.setLanguageConfiguration(id, config);
        }
    }

    /* ===================================================
       LSP bridge: Monaco providers <-> WinForms host <-> R# language server
    =================================================== */

    let lspSeq = 0;
    const lspPending = new Map();

    function lspRequest(method, params) {
        const id = ++lspSeq;

        return new Promise((resolve, reject) => {
            lspPending.set(id, { resolve, reject });
            send(Outgoing.LspRequest, { id, method, params });
        });
    }

    function lspNotify(method, params) {
        send(Outgoing.LspNotify, { method, params });
    }

    // host -> page: settle the promise that is waiting on this request id
    function handleLspResponse(payload) {
        const waiter = lspPending.get(payload.id);
        if (!waiter) return;

        lspPending.delete(payload.id);

        if (payload.error) waiter.reject(payload.error);
        else waiter.resolve(payload.result);
    }

    // a provider failure must never break the editor: degrade to the fallback
    async function lspCall(method, params, fallback) {
        try {
            const result = await lspRequest(method, params);
            return result === undefined ? fallback : result;
        }
        catch (e) {
            console.error("rsharp lsp error:", method, e);
            return fallback;
        }
    }

    const docUri = () => model.uri.toString();

    // Monaco positions are 1-based, LSP positions are 0-based
    const lspPosition = (position) => ({
        line: position.lineNumber - 1,
        character: position.column - 1
    });

    const toMonacoRange = (r) => new monaco.Range(
        r.start.line + 1, r.start.character + 1,
        r.end.line + 1, r.end.character + 1
    );

    // LSP CompletionItemKind -> monaco.languages.CompletionItemKind
    // (the two enums are NOT numerically identical)
    const completionKindMap = {
        2: monaco.languages.CompletionItemKind.Method,
        3: monaco.languages.CompletionItemKind.Function,
        6: monaco.languages.CompletionItemKind.Variable,
        14: monaco.languages.CompletionItemKind.Keyword
    };

    // LSP SymbolKind(12 = Function) -> monaco.languages.SymbolKind(11 = Function)
    const symbolKindMap = { 12: monaco.languages.SymbolKind.Function };

    monaco.languages.registerHoverProvider('r', {
        provideHover: async function (m, position) {
            const r = await lspCall('textDocument/hover', {
                textDocument: { uri: docUri() },
                position: lspPosition(position)
            }, null);

            if (!r || !r.contents) return null;

            const value = typeof r.contents === 'string' ? r.contents : r.contents.value;

            return {
                contents: [{ value: value || '' }],
                range: r.range ? toMonacoRange(r.range) : undefined
            };
        }
    });

    monaco.languages.registerCompletionItemProvider('r', {
        triggerCharacters: ['(', '.', ' ', ':'],

        provideCompletionItems: async function (m, position) {
            const list = (await lspCall('textDocument/completion', {
                textDocument: { uri: docUri() },
                position: lspPosition(position)
            }, [])) || [];

            return {
                suggestions: list.map(function (i) {
                    return {
                        label: i.label,
                        kind: completionKindMap[i.kind] ?? monaco.languages.CompletionItemKind.Function,
                        detail: i.detail,
                        insertText: i.label
                    };
                })
            };
        }
    });

    monaco.languages.registerDefinitionProvider('r', {
        provideDefinition: async function (m, position) {
            const list = (await lspCall('textDocument/definition', {
                textDocument: { uri: docUri() },
                position: lspPosition(position)
            }, [])) || [];

            return list.map(function (loc) {
                return {
                    uri: monaco.Uri.parse(loc.uri),
                    range: toMonacoRange(loc.range)
                };
            });
        }
    });

    monaco.languages.registerSignatureHelpProvider('r', {
        signatureHelpTriggerCharacters: ['(', ','],
        signatureHelpRetriggerCharacters: [','],

        provideSignatureHelp: async function (m, position) {
            const r = await lspCall('textDocument/signatureHelp', {
                textDocument: { uri: docUri() },
                position: lspPosition(position)
            }, null);

            if (!r || !r.signatures || !r.signatures.length) return null;

            return r;
        }
    });

    monaco.languages.registerDocumentSymbolProvider('r', {
        provideDocumentSymbols: async function () {
            const list = (await lspCall('textDocument/documentSymbol', {
                textDocument: { uri: docUri() }
            }, [])) || [];

            return list.map(function (s) {
                return {
                    name: s.name,
                    detail: s.detail,
                    kind: symbolKindMap[s.kind] ?? monaco.languages.SymbolKind.Function,
                    range: toMonacoRange(s.range),
                    selectionRange: toMonacoRange(s.selectionRange || s.range)
                };
            });
        }
    });

    /* ===================================================
       Document synchronization (full text sync: the server declares
       textDocumentSync = 1, so every change carries the whole document)
    =================================================== */

    let docVersion = 1;

    function syncDidOpen() {
        docVersion = 1;

        lspNotify('textDocument/didOpen', {
            textDocument: {
                uri: docUri(),
                languageId: model.getLanguageId(),
                version: docVersion,
                text: editor.getValue()
            }
        });
    }

    function syncDidChange() {
        docVersion += 1;

        lspNotify('textDocument/didChange', {
            textDocument: { uri: docUri(), version: docVersion },
            contentChanges: [{ text: editor.getValue() }]
        });
    }

    /*===================================================*/


    const handlers = {
        [Incoming.SetText]: handleSetText,
        [Incoming.SetReadOnly]: handleSetReadOnly,
        [Incoming.MarkAsSaved]: handleMarkAsSaved,
        [Incoming.GetLanguages]: handleGetLanguages,
        [Incoming.SetLanguage]: handleSetLanguage,
        [Incoming.UpdateOption]: handleUpdateOption,
        [Incoming.RegisterTheme]: handleRegisterTheme,
        [Incoming.SetTheme]: handleSetTheme,
        [Incoming.RegisterLanguage]: handleRegisterLanguage,
        [Incoming.LspResponse]: handleLspResponse
    };

    model.onDidChangeContent(() => {
        updateTextChanged();
        updateDirtyState();
    });

    //listen to updates from the WinForms side
    window.chrome.webview.addEventListener('message', event => {
        const msg = event.data;
        const handler = handlers[msg.type];
        if (handler) handler(msg.payload);
    });

    /*===================================================*/
    //notify that the editor has been initialized (requestAnimationFrame is required, because otherwise the editor is not ready for SetText)
    send(Outgoing.EditorReady);

    // register the document with the R# language server: hover / definition /
    // completion all resolve against the server-side document cache, so the
    // document must be opened before any provider request is answered.
    syncDidOpen();
    /*===================================================*/
});

// 调用：设置编辑器内容
window.setEditorContent = function (content, language) {
    window.editor.setValue(content);
    if (language) {
        monaco.editor.setModelLanguage(window.editor.getModel(), language);
    }
};

// 调用：获取编辑器内容
window.getEditorContent = function () {
    return window.editor.getValue();
};