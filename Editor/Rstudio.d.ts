/// <reference path="vscode/monaco.d.ts" />
declare namespace rstudio {
    function create(): void;
    function setup(): void;
}
declare namespace rstudio.tooltip {
    function create_tooltip(model: monaco.editor.ITextModel, position: monaco.Position): Promise<unknown>;
    function contentHtml(word: string): string;
    const imports_keyword: string;
    const return_keyword: string;
    const keywords: {
        imports: string;
        return: string;
    };
}
