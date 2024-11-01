/// <reference path="vscode/monaco.d.ts" />
declare namespace rstudio {
    function create(): void;
    function setup(): void;
}
declare namespace rstudio.tooltip {
    function create_tooltip(model: monaco.editor.ITextModel, position: monaco.Position): {
        range: monaco.Range;
        contents: {
            supportHtml: boolean;
            value: string;
        }[];
    };
    function contentHtml(word: string): string;
    const imports_keyword: string;
    const keywords: {
        imports: string;
    };
}
