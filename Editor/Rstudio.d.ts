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
    const imports_keyword = "<h1>Loading/Attaching of the .NET clr package module</h1>\n    <p>similar to the <code>library</code> and <code>require</code> load and attach add-on .NET clr package modules.</p>";
    const keywords: {
        imports: string;
    };
}
