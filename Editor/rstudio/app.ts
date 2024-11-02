/// <reference path="../vscode/monaco.d.ts" />
/// <reference path="../linq.d.ts" />
/// <reference path="./editor.ts" />

interface ILoadModule {
    (v: string[], load: () => void): void;
    config(config: any);
}

function run_vscode() {
    const require: ILoadModule = (<any>window).require;

    // run the vscode
    require.config({ paths: { vs: './vscode/min/vs' } });
    require(['vs/editor/editor.main'], function () {
        rstudio.setup();
        rstudio.create();
    });
}

