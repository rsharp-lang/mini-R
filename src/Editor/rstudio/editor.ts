module rstudio {

    /**
     * the language editor core
    */
    let editor: monaco.editor.IStandaloneCodeEditor = null;
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

        if (editor && typeof editor.dispose === 'function') {
            // 编辑器存在，可以继续摧毁
            editor.dispose();
        }

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

    export function jumpToLine(line: number) {
        if (editor && typeof editor.dispose === 'function') {
            editor.revealLine(line);
            editor.setPosition({ lineNumber: line, column: 1 });
        }
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

        if (check_webview2()) {
            let webview: {
                postMessage: (string) => void
            } = (<any>window).chrome.webview;

            document.addEventListener("input", function (evt) {
                webview.postMessage('input');
            });
            document.addEventListener('change', function (evt) {
                webview.postMessage('change');
            });
        }
    }
}