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

const text = http_get("http://a.com");

print(text);
`;

    export function getCodeText() {
        return editor.getModel().getValue();
    }

    export function create() {
        // create demo test
        create_editor(demo_r, 'r');
    }

    export function create_editor(script: string, lang: 'r' | 'json') {
        let container = $ts('#container');

        key = md5(script);
        editor = monaco.editor.create(container, {
            value: script,
            language: lang,
            automaticLayout: true,
            glyphMargin: true,
            lightbulb: {
                enabled: monaco.editor.ShowLightbulbIconMode.On
            },
            minimap: {
                maxColumn: 120
            }
        });

        auto_commit();
    }

    export function hashkey(): string {
        return key;
    }

    /**
     * auto commit the script updates to server
    */
    function auto_commit() {
        editor.onDidChangeModelContent((event) => {
            // save to server
            lsp.put_script(getCodeText(), key);
        });

        // initialize of the server environment
        lsp.put_script(demo_r, key);
    }

    export function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: (model, position) => rstudio.tooltip.create_tooltip(model, position)
        });
        monaco.languages.registerCompletionItemProvider('r', {
            provideCompletionItems: (model, position) => rstudio.intellisense.create_intellisense(model, position)
        });
    }
}