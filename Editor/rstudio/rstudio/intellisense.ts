namespace rstudio.intellisense {

    export function create_intellisense(model: monaco.editor.ITextModel, position: monaco.Position): any {
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

    export const r_keywords = [
        'require', 'library', 'if', 'for', 'return', 'function', 'let', 'const', 'imports', 'from', 'next', 'break', 'while', 'else', 'step'
    ];

    export const r_primitive = [
        'c', 'list', 'str', 'print', 'example', "is.null", "length", "readLines", "writeLines", "help", "as.character", "as.numeric", "readBin", "writeBin", "as.logical"
    ];

    export const r_const = [
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

        for (const item of r_keywords) {
            keys.push({
                label: item,
                kind: monaco.languages.CompletionItemKind.Keyword,
                documentation: "",
                insertText: item,
                range: range
            });
        }

        for (const item of r_primitive) {
            keys.push({
                label: item,
                kind: monaco.languages.CompletionItemKind.Function,
                documentation: "",
                insertText: item,
                range: range
            });
        }

        for (const item of r_const) {
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
}

