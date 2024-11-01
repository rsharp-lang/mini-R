namespace rstudio.intellisense {

    export function create_intellisense(model: monaco.editor.ITextModel, position: monaco.Position): any {
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

    export const r_keywords = [
        'c', 'require', 'library', 'if', 'for', 'list', 'str', 'print', 'return', 'function', 'let', 'const', 'imports', 'from'
    ];

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

        return snippets.concat(keys);
    }
}

