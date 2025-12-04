import * as vscode from 'vscode';
import * as path from 'path';
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind
} from 'vscode-languageclient/node';
import { KeyboardPanel } from './webview/KeyboardPanel';
import { EmulatorPanel } from './webview/EmulatorPanel';
import { RegistersPanel } from './webview/RegistersPanel';
import { DisassemblyPanel } from './webview/DisassemblyPanel';
import { MemoryPanel } from './webview/MemoryPanel';
import { TapePanel } from './webview/TapePanel';

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

    context.subscriptions.push(
        vscode.commands.registerCommand('spectnet.emulator', () => {
            EmulatorPanel.createOrShow(context.extensionUri);
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('spectnet.registers', () => {
            RegistersPanel.createOrShow(context.extensionUri);
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('spectnet.disassembly', () => {
            DisassemblyPanel.createOrShow(context.extensionUri);
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('spectnet.memory', () => {
            MemoryPanel.createOrShow(context.extensionUri);
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('spectnet.tape', () => {
            TapePanel.createOrShow(context.extensionUri);
        })
    );

    // Listen for custom DAP events
    context.subscriptions.push(vscode.debug.onDidReceiveDebugSessionCustomEvent(e => {
        if (e.event === 'spectnet/videoFrame') {
            if (EmulatorPanel.currentPanel) {
                EmulatorPanel.currentPanel.updateVideoFrame(e.body.data);
            }
        } else if (e.event === 'spectnet/registers') {
            if (RegistersPanel.currentPanel) {
                RegistersPanel.currentPanel.updateRegisters(e.body);
            }
        } else if (e.event === 'spectnet/disassembly') {
            if (DisassemblyPanel.currentPanel) {
                DisassemblyPanel.currentPanel.updateDisassembly(e.body);
            }
        } else if (e.event === 'spectnet/memory') {
            if (MemoryPanel.currentPanel) {
                MemoryPanel.currentPanel.updateMemory(e.body);
            }
        } else if (e.event === 'spectnet/tape') {
            if (TapePanel.currentPanel) {
                TapePanel.currentPanel.updateTape(e.body.blocks);
            }
        }
    }));

    // Register configuration provider
    context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider('spectnet', new SpectNetDebugConfigurationProvider()));
}

class SpectNetDebugConfigurationProvider implements vscode.DebugConfigurationProvider {
    resolveDebugConfiguration(folder: vscode.WorkspaceFolder | undefined, config: vscode.DebugConfiguration, token?: vscode.CancellationToken): vscode.ProviderResult<vscode.DebugConfiguration> {
        // if launch.json is missing or empty
        if (!config.type && !config.request && !config.name) {
            const editor = vscode.window.activeTextEditor;
            if (editor && editor.document.languageId === 'z80asm') {
                config.type = 'spectnet';
                config.name = 'Launch';
                config.request = 'launch';
                config.program = '${file}';
                config.stopOnEntry = true;
            }
        }

        if (!config.program) {
            return vscode.window.showInformationMessage("Cannot find a program to debug").then(_ => {
                return undefined;	// abort launch
            });
        }

        // Load spectnet.config.json
        if (folder) {
            const configPath = path.join(folder.uri.fsPath, 'spectnet.config.json');
            try {
                // We use a simple require or fs.readFileSync here. 
                // Since we are in an extension, fs is available.
                const fs = require('fs');
                if (fs.existsSync(configPath)) {
                    const projectConfig = JSON.parse(fs.readFileSync(configPath, 'utf8'));
                    
                    // Merge configuration
                    if (!config.model && projectConfig.model) {
                        config.model = projectConfig.model;
                    }
                    if (!config.edition && projectConfig.edition) {
                        config.edition = projectConfig.edition;
                    }
                    
                    if (projectConfig.compiler) {
                        if (!config.predefinedSymbols && projectConfig.compiler.predefinedSymbols) {
                            config.predefinedSymbols = projectConfig.compiler.predefinedSymbols;
                        }
                        if (config.defaultStartAddress === undefined && projectConfig.compiler.defaultStartAddress !== undefined) {
                            config.defaultStartAddress = projectConfig.compiler.defaultStartAddress;
                        }
                        if (config.defaultDisplacement === undefined && projectConfig.compiler.defaultDisplacement !== undefined) {
                            config.defaultDisplacement = projectConfig.compiler.defaultDisplacement;
                        }
                    }
                }
            } catch (error) {
                console.error("Failed to load spectnet.config.json", error);
            }
        }

        return config;
    }
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
