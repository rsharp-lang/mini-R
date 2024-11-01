module rstudio.tooltip {

    export function create_tooltip(model: monaco.editor.ITextModel, position: monaco.Position) {
        // 获取光标位置的单词
        const word = model.getWordAtPosition(position);

        if (!word) {
            return null;
        }

        // 根据单词显示自定义提示
        const hoverContent = `这是关于 "${word.word}" 的自定义提示。`;
        const hover = {
            range: new monaco.Range(
                position.lineNumber,
                word.startColumn,
                position.lineNumber,
                word.endColumn
            ),
            contents: [
                { value: hoverContent }
            ]
        };

        return hover;
    }

    export const imports_keyword = ``;
}