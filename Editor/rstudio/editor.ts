module rstudio {

    /**
     * the language editor core
    */
    let editor: monaco.editor.IStandaloneCodeEditor;
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

    export function create() {
        let container = document.getElementById('container');

        editor = monaco.editor.create(container, {
            value: demo_r,
            language: 'r'
        });
    }

    export function setup() {
        monaco.languages.registerHoverProvider('r', {
            provideHover: (model, position) => rstudio.tooltip.create_tooltip(model, position)
        });
    }
}