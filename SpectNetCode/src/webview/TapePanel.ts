import * as vscode from 'vscode';

export class TapePanel {
    public static currentPanel: TapePanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;

        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);

        this._panel.webview.html = this._getWebviewContent();
    }

    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        if (TapePanel.currentPanel) {
            TapePanel.currentPanel._panel.reveal(column);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'spectNetTape',
            'Tape Explorer',
            column || vscode.ViewColumn.One,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')]
            }
        );

        TapePanel.currentPanel = new TapePanel(panel, extensionUri);
    }

    public updateTape(data: any) {
        this._panel.webview.postMessage({ command: 'updateTape', data: data });
    }

    public dispose() {
        TapePanel.currentPanel = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            const x = this._disposables.pop();
            if (x) {
                x.dispose();
            }
        }
    }

    private _getWebviewContent() {
        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Tape Explorer</title>
    <style>
        body { font-family: var(--vscode-font-family); padding: 10px; color: var(--vscode-editor-foreground); background-color: var(--vscode-editor-background); }
        table { width: 100%; border-collapse: collapse; }
        th, td { text-align: left; padding: 5px; border-bottom: 1px solid var(--vscode-panel-border); }
        th { font-weight: bold; }
        .highlight { background-color: var(--vscode-editor-selectionBackground); color: var(--vscode-editor-selectionForeground); }
        .block-type { color: var(--vscode-debugTokenExpression-name); }
        .block-name { color: var(--vscode-debugTokenExpression-string); }
    </style>
</head>
<body>
    <h3>Tape Blocks</h3>
    <table id="tape-table">
        <thead>
            <tr>
                <th>ID</th>
                <th>Type</th>
                <th>Name</th>
                <th>Size</th>
                <th>Start</th>
                <th>Param2</th>
            </tr>
        </thead>
        <tbody>
        </tbody>
    </table>
    <script>
        const vscode = acquireVsCodeApi();
        const tableBody = document.querySelector('#tape-table tbody');

        window.addEventListener('message', event => {
            const message = event.data;
            if (message.command === 'updateTape') {
                const blocks = message.data;
                tableBody.innerHTML = '';
                blocks.forEach((block, index) => {
                    const row = document.createElement('tr');
                    if (block.isCurrent) {
                        row.classList.add('highlight');
                    }
                    
                    row.innerHTML = \`
                        <td>\${index}</td>
                        <td class="block-type">\${block.type}</td>
                        <td class="block-name">\${block.name || '-'}</td>
                        <td>\${block.size}</td>
                        <td>\${block.start !== undefined ? '0x' + block.start.toString(16).toUpperCase().padStart(4, '0') : '-'}</td>
                        <td>\${block.param2 !== undefined ? '0x' + block.param2.toString(16).toUpperCase().padStart(4, '0') : '-'}</td>
                    \`;
                    tableBody.appendChild(row);
                });
            }
        });
    </script>
</body>
</html>`;
    }
}
