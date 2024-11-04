/// <reference path="../vscode/monaco.d.ts" />
/// <reference path="../linq.d.ts" />
/// <reference path="./editor.ts" />

interface ILoadModule {
    (v: string[], load: () => void): void;

    /**
     * do config of the environment workspace
    */
    config(config: any);
}

function run_vscode(script_file: string, lang: 'r' | 'json') {
    const require: ILoadModule = (<any>window).require;

    lsp.get_file(script_file).then(script_str => {
        // run the vscode
        require.config({ paths: { vs: './vscode/min/vs' } });
        require(['vs/editor/editor.main'], function () {
            rstudio.setup();
            rstudio.create_editor(script_str, lang);
        });
    });
}

// $ts(run_vscode);