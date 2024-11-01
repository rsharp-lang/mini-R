namespace rstudio.intellisense {

    export function create_intellisense(model: monaco.editor.ITextModel, position: monaco.Position): any {
        var suggestions = [
            { label: 'hello', kind: monaco.languages.CompletionItemKind.Text, documentation: 'A greeting word' },
            { label: 'world', kind: monaco.languages.CompletionItemKind.Text, documentation: 'The planet we live on' }
        ];

        return { suggestions: suggestions };
    }
}

