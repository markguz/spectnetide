import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext) {
    console.log('Congratulations, your extension "spectnetcode" is now active!');

    let disposable = vscode.commands.registerCommand('spectnet.helloWorld', () => {
        vscode.window.showInformationMessage('Hello World from SpectNetCode!');
    });

    context.subscriptions.push(disposable);
}

export function deactivate() {}
