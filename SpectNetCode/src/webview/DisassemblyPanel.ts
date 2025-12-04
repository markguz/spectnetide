import * as vscode from 'vscode';

export class DisassemblyPanel {
    public static currentPanel: DisassemblyPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        if (DisassemblyPanel.currentPanel) {
            DisassemblyPanel.currentPanel._panel.reveal(column);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'spectNetDisassembly',
            'SpectNet Disassembly',
            column || vscode.ViewColumn.Two,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')]
            }
        );

        DisassemblyPanel.currentPanel = new DisassemblyPanel(panel, extensionUri);
    }

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;

        this._update();
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
    }

    public updateDisassembly(data: any) {
        this._panel.webview.postMessage({ command: 'updateDisassembly', data: data });
    }

    public dispose() {
        DisassemblyPanel.currentPanel = undefined;
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
                <title>SpectNet Disassembly</title>
                <style>
                    body { font-family: var(--vscode-editor-font-family); padding: 0; margin: 0; color: var(--vscode-editor-foreground); background-color: var(--vscode-editor-background); }
                    table { width: 100%; border-collapse: collapse; table-layout: fixed; }
                    th, td { text-align: left; padding: 2px 5px; white-space: pre; }
                    th { font-weight: bold; border-bottom: 1px solid var(--vscode-panel-border); }
                    .address { color: var(--vscode-debugTokenExpression-name); width: 60px; }
                    .opcodes { color: var(--vscode-debugTokenExpression-value); width: 100px; }
                    .instruction { color: var(--vscode-debugTokenExpression-string); }
                    .current-pc { background-color: var(--vscode-editor-selectionBackground); }
                </style>
            </head>
            <body>
                <table id="disassemblyTable">
                    <thead>
                        <tr><th class="address">Addr</th><th class="opcodes">OpCodes</th><th class="instruction">Instruction</th></tr>
                    </thead>
                    <tbody>
                        <!-- Rows will be added here -->
                    </tbody>
                </table>
                <script>
                    const vscode = acquireVsCodeApi();
                    const tableBody = document.querySelector('#disassemblyTable tbody');

                    window.addEventListener('message', event => {
                        const message = event.data;
                        switch (message.command) {
                            case 'updateDisassembly':
                                const data = message.data;
                                const items = data.items;
                                const currentPc = data.pc;
                                
                                tableBody.innerHTML = '';
                                items.forEach(item => {
                                    const row = document.createElement('tr');
                                    if (item.address === currentPc) {
                                        row.className = 'current-pc';
                                    }
                                    
                                    const addrCell = document.createElement('td');
                                    addrCell.className = 'address';
                                    addrCell.textContent = item.addressHex;
                                    
                                    const opCodesCell = document.createElement('td');
                                    opCodesCell.className = 'opcodes';
                                    opCodesCell.textContent = item.opCodes;
                                    
                                    const instrCell = document.createElement('td');
                                    instrCell.className = 'instruction';
                                    instrCell.textContent = item.instruction;
                                    
                                    row.appendChild(addrCell);
                                    row.appendChild(opCodesCell);
                                    row.appendChild(instrCell);
                                    tableBody.appendChild(row);
                                });
                                break;
                        }
                    });
                </script>
            </body>
            </html>`;
    }
}
