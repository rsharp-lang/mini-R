module rstudio {

    /**
     * the language editor core
    */
    let editor: monaco.editor.IStandaloneCodeEditor;
    let demo_r = `    
imports "aaa" from "bbb";
        
let f(x) = console.log("Hello world!");
let hello_world = function(x) {
    return \`hello \${x}!\`;
};

print(c(1,2,3,4,5));

`;

    export function create() {
        let container = document.getElementById('container')

        editor = monaco.editor.create(container, {
            value: demo_r,
            language: 'r'
        });
    }

    export function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: function (model, position) {
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
        });
    }
}