/// <reference path="../vscode/monaco.d.ts" />
/// <reference path="./editor.ts" />

require.config({ paths: { vs: './vscode/min/vs' } });
require(['vs/editor/editor.main'], function () {
    rstudio.setup();
    rstudio.create();
});