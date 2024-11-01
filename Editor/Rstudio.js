var rstudio;
(function (rstudio) {
    /**
     * the language editor core
    */
    let editor;
    let demo_r = `    
imports "JSON" from "base";
        
let f(x) = console.log("Hello world!");
let hello_world = function(x) {
    return \`hello \${x}!\`;
};

print(c(1,2,3,4,5));
str(list(
    a = 123,
    b = [TRUE, TRUE, FALSE],
    c = "XXX"
));
`;
    function create() {
        let container = document.getElementById('container');
        editor = monaco.editor.create(container, {
            value: demo_r,
            language: 'r',
            automaticLayout: true,
            glyphMargin: true,
            lightbulb: {
                enabled: monaco.editor.ShowLightbulbIconMode.On
            }
        });
    }
    rstudio.create = create;
    function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: (model, position) => rstudio.tooltip.create_tooltip(model, position)
        });
        monaco.languages.registerCompletionItemProvider('r', {
            provideCompletionItems: (model, position) => rstudio.intellisense.create_intellisense(model, position)
        });
    }
    rstudio.setup = setup;
})(rstudio || (rstudio = {}));
/// <reference path="../vscode/monaco.d.ts" />
/// <reference path="./editor.ts" />
require.config({ paths: { vs: './vscode/min/vs' } });
require(['vs/editor/editor.main'], function () {
    rstudio.setup();
    rstudio.create();
});
var rstudio;
(function (rstudio) {
    var intellisense;
    (function (intellisense) {
        function create_intellisense(model, position) {
            var suggestions = [
                { label: 'hello', kind: monaco.languages.CompletionItemKind.Text, documentation: 'A greeting word' },
                { label: 'world', kind: monaco.languages.CompletionItemKind.Text, documentation: 'The planet we live on' }
            ];
            return { suggestions: suggestions };
        }
        intellisense.create_intellisense = create_intellisense;
    })(intellisense = rstudio.intellisense || (rstudio.intellisense = {}));
})(rstudio || (rstudio = {}));
var rstudio;
(function (rstudio) {
    var tooltip;
    (function (tooltip_1) {
        function create_tooltip(model, position) {
            // 获取光标位置的单词
            const word = model.getWordAtPosition(position);
            if (!word) {
                return null;
            }
            else {
                return new Promise((resolve, reject) => {
                    resolveTooltip(word, position, resolve);
                });
            }
        }
        tooltip_1.create_tooltip = create_tooltip;
        function resolveTooltip(word, position, resolve) {
            // 根据单词显示自定义提示
            const hoverContent = contentHtml(word.word);
            const htmlContent = {
                supportHtml: true,
                value: hoverContent
            };
            const hover = {
                range: new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn),
                contents: [htmlContent]
            };
            if (!hoverContent) {
                resolve(null);
            }
            else {
                resolve(hover);
            }
        }
        function contentHtml(word) {
            if (word in tooltip_1.keywords) {
                return tooltip_1.keywords[word];
            }
            else {
                return null;
            }
        }
        tooltip_1.contentHtml = contentHtml;
        function tooltip(title, text) {
            return `<h3>${title}</h3><p>${text}</p>`;
        }
        tooltip_1.imports_keyword = tooltip('Loading/attaching of the .NET clr package module', 'similar to the <code>library</code> and <code>require</code> load and attach add-on .NET clr package modules.');
        tooltip_1.return_keyword = tooltip('Returns the function value to caller', `If value is missing, NULL is returned. If it is a single expression, the value of the evaluated expression is returned. 
(The expression is evaluated as soon as return is called, in the evaluation frame of the function and before any on.exit expression is evaluated.)
If the end of a function is reached without calling return, the value of the last evaluated expression is returned.`);
        tooltip_1.list_keyword = tooltip("Lists - Generic and Dotted Pairs", `Functions to construct, coerce and check for both kinds of <code>R</code> lists.`);
        tooltip_1.logical_keyword = tooltip("Logical Vectors", `Create or test for objects of type 'logical', and the basic logical constants.
TRUE and FALSE are reserved words denoting logical constants in the R language, whereas T and F are global variables whose initial values set to these. 
All four are logical(1) vectors.
Logical vectors are coerced to integer vectors in contexts where a numerical value is required, with TRUE being mapped to 1L, FALSE to 0L and NA to NA_integer_.`);
        tooltip_1.keywords = {
            "imports": tooltip_1.imports_keyword,
            'return': tooltip_1.return_keyword,
            'list': tooltip_1.list_keyword,
            'TRUE': tooltip_1.logical_keyword,
            'FALSE': tooltip_1.logical_keyword
        };
    })(tooltip = rstudio.tooltip || (rstudio.tooltip = {}));
})(rstudio || (rstudio = {}));
//# sourceMappingURL=Rstudio.js.map