/// <reference path="vscode/monaco.d.ts" />
declare namespace rstudio {
    function getCodeText(): string;
    function create(): void;
    function setup(): void;
}
declare namespace rstudio.intellisense {
    function create_intellisense(model: monaco.editor.ITextModel, position: monaco.Position): any;
    const r_keywords: string[];
}
declare namespace rstudio.tooltip {
    function create_tooltip(model: monaco.editor.ITextModel, position: monaco.Position): any;
    function contentHtml(word: string): string;
    const imports_keyword: string;
    const return_keyword: string;
    const list_keyword: string;
    const logical_keyword: string;
    const keywords: {
        imports: string;
        return: string;
        list: string;
        TRUE: string;
        FALSE: string;
    };
}
