module rstudio {

    /**
     * the language editor core
    */
    let editor: monaco.editor.IStandaloneCodeEditor;
    let key: string;
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

    export function getCodeText() {
        return editor.getModel().getValue();
    }

    export function create() {
        let container = $ts('#container');

        key = md5(demo_r);
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

    export function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: (model, position) => rstudio.tooltip.create_tooltip(model, position)
        });
        monaco.languages.registerCompletionItemProvider('r', {
            provideCompletionItems: (model, position) => rstudio.intellisense.create_intellisense(model, position)
        });

        editor.onDidChangeModelContent((event) => {
            // save to server
            lsp.put_script(editor.getValue(), key);
        });
    }
}