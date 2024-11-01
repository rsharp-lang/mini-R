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

    export const return_keyword = tooltip(
        'Returns the function value to caller',
        `If value is missing, NULL is returned. If it is a single expression, the value of the evaluated expression is returned. 
(The expression is evaluated as soon as return is called, in the evaluation frame of the function and before any on.exit expression is evaluated.)
If the end of a function is reached without calling return, the value of the last evaluated expression is returned.`);

    export const keywords = {
        "imports": imports_keyword,
        'return': return_keyword
    };
}