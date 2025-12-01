import * as vscode from 'vscode';
import * as path from 'path';
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind
} from 'vscode-languageclient/node';
import { KeyboardPanel } from './KeyboardPanel';

let client: LanguageClient;

export function activate(context: vscode.ExtensionContext) {
    console.log('Congratulations, your extension "spectnetcode" is now active!');

    // The server is implemented in C#
    let serverExe = 'dotnet';
    
    // If we are in development mode, we use the debug build of the server
    // In production, we would expect the server to be packaged with the extension
    // For now, we point to the build output of the LanguageServer project
    let serverPath = path.join(context.extensionPath, '..', 'Spect.Net.LanguageServer', 'bin', 'Debug', 'net6.0', 'Spect.Net.LanguageServer.dll');

    // If the extension is installed, we might want to look for the server in a different place
    // but for this development phase, the relative path to the project build is fine.
    
    let serverOptions: ServerOptions = {
        run: { command: serverExe, args: [serverPath], transport: TransportKind.stdio },
        debug: { command: serverExe, args: [serverPath], transport: TransportKind.stdio }
    };

    let clientOptions: LanguageClientOptions = {
        documentSelector: [{ scheme: 'file', language: 'z80asm' }],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher('**/*.z80asm')
        }
    };

    client = new LanguageClient(
        'spectNetLanguageServer',
        'SpectNet Language Server',
        serverOptions,
        clientOptions
    );

    client.start();

    let disposable = vscode.commands.registerCommand('spectnet.helloWorld', () => {
        vscode.window.showInformationMessage('Hello World from SpectNetCode!');
    });

    context.subscriptions.push(disposable);

    context.subscriptions.push(
        vscode.commands.registerCommand('spectnet.keyboardTool', () => {
            KeyboardPanel.createOrShow(context.extensionUri);
        })
    );
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
