import * as vscode from 'vscode';

export class MemoryPanel {
    public static currentPanel: MemoryPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        if (MemoryPanel.currentPanel) {
            MemoryPanel.currentPanel._panel.reveal(column);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'spectNetMemory',
            'SpectNet Memory',
            column || vscode.ViewColumn.Two,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')]
            }
        );

        MemoryPanel.currentPanel = new MemoryPanel(panel, extensionUri);
    }

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;

        this._update();
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
    }

    public updateMemory(data: any) {
        this._panel.webview.postMessage({ command: 'updateMemory', data: data });
    }

    public dispose() {
        MemoryPanel.currentPanel = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            const x = this._disposables.pop();
            if (x) {
                x.dispose();
            }
        }
    }

    private _update() {
        this._panel.webview.html = this._getHtmlForWebview();
    }

    private _getHtmlForWebview() {
        return `<!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>SpectNet Memory</title>
                <style>
                    body { font-family: var(--vscode-editor-font-family); padding: 0; margin: 0; color: var(--vscode-editor-foreground); background-color: var(--vscode-editor-background); }
                    table { width: 100%; border-collapse: collapse; table-layout: fixed; font-family: monospace; }
                    th, td { text-align: left; padding: 2px 5px; white-space: pre; }
                    th { font-weight: bold; border-bottom: 1px solid var(--vscode-panel-border); }
                    .address { color: var(--vscode-debugTokenExpression-name); width: 60px; }
                    .hex { color: var(--vscode-debugTokenExpression-value); }
                    .ascii { color: var(--vscode-debugTokenExpression-string); }
                </style>
            </head>
            <body>
                <table id="memoryTable">
                    <thead>
                        <tr><th class="address">Addr</th><th class="hex">Hex</th><th class="ascii">ASCII</th></tr>
                    </thead>
                    <tbody>
                        <!-- Rows will be added here -->
                    </tbody>
                </table>
                <script>
                    const vscode = acquireVsCodeApi();
                    const tableBody = document.querySelector('#memoryTable tbody');

                    window.addEventListener('message', event => {
                        const message = event.data;
                        switch (message.command) {
                            case 'updateMemory':
                                const data = message.data;
                                const memory = data.memory; // Base64 encoded memory
                                const startAddress = data.startAddress;
                                
                                const binaryString = window.atob(memory);
                                const len = binaryString.length;
                                const bytes = new Uint8Array(len);
                                for (let i = 0; i < len; i++) {
                                    bytes[i] = binaryString.charCodeAt(i);
                                }

                                tableBody.innerHTML = '';
                                for (let i = 0; i < len; i += 16) {
                                    const row = document.createElement('tr');
                                    
                                    const addrCell = document.createElement('td');
                                    addrCell.className = 'address';
                                    const addr = startAddress + i;
                                    addrCell.textContent = '0x' + addr.toString(16).padStart(4, '0').toUpperCase();
                                    
                                    const hexCell = document.createElement('td');
                                    hexCell.className = 'hex';
                                    let hexText = '';
                                    let asciiText = '';
                                    
                                    for (let j = 0; j < 16; j++) {
                                        if (i + j < len) {
                                            const byte = bytes[i + j];
                                            hexText += byte.toString(16).padStart(2, '0').toUpperCase() + ' ';
                                            asciiText += (byte >= 32 && byte <= 126) ? String.fromCharCode(byte) : '.';
                                        } else {
                                            hexText += '   ';
                                            asciiText += ' ';
                                        }
                                    }
                                    
                                    hexCell.textContent = hexText;
                                    
                                    const asciiCell = document.createElement('td');
                                    asciiCell.className = 'ascii';
                                    asciiCell.textContent = asciiText;
                                    
                                    row.appendChild(addrCell);
                                    row.appendChild(hexCell);
                                    row.appendChild(asciiCell);
                                    tableBody.appendChild(row);
                                }
                                break;
                        }
                    });
                </script>
            </body>
            </html>`;
    }
}
