"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.activate = activate;
exports.deactivate = deactivate;
const vscode = require("vscode");
function activate(context) {
    console.log('Congratulations, your extension "spectnetcode" is now active!');
    let disposable = vscode.commands.registerCommand('spectnet.helloWorld', () => {
        vscode.window.showInformationMessage('Hello World from SpectNetCode!');
    });
    context.subscriptions.push(disposable);
}
function deactivate() { }
//# sourceMappingURL=extension.js.map