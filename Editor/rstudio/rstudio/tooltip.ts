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

    export const imports_keyword = `<h1>Loading/Attaching of the .NET clr package module</h1>
    <p>similar to the <code>library</code> and <code>require</code> load and attach add-on .NET clr package modules.</p>`;

    export const keywords = {
        "imports": imports_keyword
    };
}