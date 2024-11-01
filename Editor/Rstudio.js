var rstudio;
(function (rstudio) {
    /**
     * the language editor core
    */
    var editor;
    var demo_r = "    \nimports \"aaa\" from \"bbb\";\n        \nlet f(x) = console.log(\"Hello world!\");\nlet hello_world = function(x) {\n    return `hello ${x}!`;\n};\n\nprint(c(1,2,3,4,5));\n\n";
    function create() {
        var container = document.getElementById('container');
        editor = monaco.editor.create(container, {
            value: demo_r,
            language: 'r'
        });
    }
    rstudio.create = create;
    function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: function (model, position) { return rstudio.tooltip.create_tooltip(model, position); }
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
            var word = model.getWordAtPosition(position);
            if (!word) {
                return null;
            }
            // 根据单词显示自定义提示
            var hoverContent = contentHtml(word.word);
            var hover = {
                range: new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn),
                contents: [
                    {
                        supportHtml: true,
                        value: hoverContent
                    }
                ]
            };
            if (!hoverContent) {
                return null;
            }
            else {
                return hover;
            }
        }
        tooltip_1.create_tooltip = create_tooltip;
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
            return "<h3>".concat(title, "</h3><p>").concat(text, "</p>");
        }
        tooltip_1.imports_keyword = tooltip('Loading/attaching of the .NET clr package module', 'similar to the <code>library</code> and <code>require</code> load and attach add-on .NET clr package modules.');
        tooltip_1.keywords = {
            "imports": tooltip_1.imports_keyword
        };
    })(tooltip = rstudio.tooltip || (rstudio.tooltip = {}));
})(rstudio || (rstudio = {}));
//# sourceMappingURL=Rstudio.js.map