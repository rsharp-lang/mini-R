/// <reference path="../vscode/monaco.d.ts" />

require.config({ paths: { vs: './vscode/min/vs' } });
require(['vs/editor/editor.main'], function () {
    var editor = monaco.editor.create(document.getElementById('container'), {
        value: [
            'imports "aaa" from "bbb";',
            '',
            'let f(x) = console.log("Hello world!");',
            'let hello_world = function(x) {',
            '   return `hello ${x}!`;',
            '};',
            '',
            'print(c(1,2,3,4,5));'
        ].join("\n"),
        language: 'r'
    });
});