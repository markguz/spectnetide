import * as vscode from 'vscode';

export class RegistersPanel {
    public static currentPanel: RegistersPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        if (RegistersPanel.currentPanel) {
            RegistersPanel.currentPanel._panel.reveal(column);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'spectNetRegisters',
            'SpectNet Registers',
            column || vscode.ViewColumn.Two,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')]
            }
        );

        RegistersPanel.currentPanel = new RegistersPanel(panel, extensionUri);
    }

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;

        this._update();
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
    }

    public updateRegisters(data: any) {
        this._panel.webview.postMessage({ command: 'updateRegisters', data: data });
    }

    public dispose() {
        RegistersPanel.currentPanel = undefined;
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
                <title>SpectNet Registers</title>
                <style>
                    body { font-family: var(--vscode-font-family); padding: 10px; color: var(--vscode-editor-foreground); }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { text-align: left; padding: 5px; border-bottom: 1px solid var(--vscode-panel-border); }
                    th { font-weight: bold; }
                    .value { font-family: var(--vscode-editor-font-family); }
                </style>
            </head>
            <body>
                <h2>Z80 Registers</h2>
                <table id="registersTable">
                    <thead>
                        <tr><th>Register</th><th>Value</th></tr>
                    </thead>
                    <tbody>
                        <!-- Rows will be added here -->
                    </tbody>
                </table>
                <script>
                    const vscode = acquireVsCodeApi();
                    const tableBody = document.querySelector('#registersTable tbody');

                    window.addEventListener('message', event => {
                        const message = event.data;
                        switch (message.command) {
                            case 'updateRegisters':
                                const regs = message.data;
                                tableBody.innerHTML = '';
                                for (const [key, value] of Object.entries(regs)) {
                                    const row = document.createElement('tr');
                                    const nameCell = document.createElement('td');
                                    nameCell.textContent = key.replace('_', "'");
                                    const valueCell = document.createElement('td');
                                    valueCell.textContent = value; // Value is already formatted string
                                    valueCell.className = 'value';
                                    row.appendChild(nameCell);
                                    row.appendChild(valueCell);
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
