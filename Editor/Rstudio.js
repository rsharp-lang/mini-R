var rstudio;
(function (rstudio) {
    /**
     * the language editor core
    */
    let editor;
    let demo_r = `    
imports "aaa" from "bbb";
        
let f(x) = console.log("Hello world!");
let hello_world = function(x) {
    return \`hello \${x}!\`;
};

print(c(1,2,3,4,5));

`;
    function create() {
        let container = document.getElementById('container');
        editor = monaco.editor.create(container, {
            value: demo_r,
            language: 'r'
        });
    }
    rstudio.create = create;
    function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: (model, position) => rstudio.tooltip.create_tooltip(model, position)
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
        tooltip_1.keywords = {
            "imports": tooltip_1.imports_keyword,
            'return': tooltip_1.return_keyword
        };
    })(tooltip = rstudio.tooltip || (rstudio.tooltip = {}));
})(rstudio || (rstudio = {}));
//# sourceMappingURL=Rstudio.js.map