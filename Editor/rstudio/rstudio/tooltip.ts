module rstudio.tooltip {

    export function create_tooltip(model: monaco.editor.ITextModel, position: monaco.Position) {
        // 获取光标位置的单词
        const word = model.getWordAtPosition(position);

        if (!word) {
            return null;
        }

        // 根据单词显示自定义提示
        const hoverContent = contentHtml(word.word);
        const hover = {
            range: new monaco.Range(
                position.lineNumber,
                word.startColumn,
                position.lineNumber,
                word.endColumn
            ),
            contents: [
                {
                    supportHtml: true,
                    value: hoverContent
                }
            ]
        };

        if (!hoverContent) {
            return null;
        } else {
            return hover;
        }
    }

    export function contentHtml(word: string): string {
        if (word in keywords) {
            return keywords[word];
        } else {
            return null;
        }
    }

    function tooltip(title: string, text: string) {
        return `<h3>${title}</h3><p>${text}</p>`;
    }

    export const imports_keyword = tooltip(
        'Loading/attaching of the .NET clr package module',
        'similar to the <code>library</code> and <code>require</code> load and attach add-on .NET clr package modules.');

    export const keywords = {
        "imports": imports_keyword
    };
}