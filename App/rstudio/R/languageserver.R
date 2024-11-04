require(languageserver);

[@info "http port for run language server for vscode client."]
let http_port as integer = ?"--port" || 321;
[@info "file path to the .NET clr assembly resource pack for provides vscode UI."]
let vscode as string = ?"--vscode" || stop("The file path to the vscode module must be provided!");

engine::listen(
    port = as.integer(http_port), 
    # resource pack for provides vscode UI
    vscode_clr = vscode
);
