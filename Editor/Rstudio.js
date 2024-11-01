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
            provideHover: function (model, position) {
                // 获取光标位置的单词
                var word = model.getWordAtPosition(position);
                if (!word) {
                    return null;
                }
                // 根据单词显示自定义提示
                var hoverContent = "\u8FD9\u662F\u5173\u4E8E \"".concat(word.word, "\" \u7684\u81EA\u5B9A\u4E49\u63D0\u793A\u3002");
                var hover = {
                    range: new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn),
                    contents: [
                        { value: hoverContent }
                    ]
                };
                return hover;
            }
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
//# sourceMappingURL=Rstudio.js.map