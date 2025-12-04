import * as vscode from 'vscode';
import * as path from 'path';

export class EmulatorPanel {
    public static currentPanel: EmulatorPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        // If we already have a panel, show it.
        if (EmulatorPanel.currentPanel) {
            EmulatorPanel.currentPanel._panel.reveal(column);
            return;
        }

        // Otherwise, create a new panel.
        const panel = vscode.window.createWebviewPanel(
            'spectNetEmulator',
            'SpectNet Emulator',
            column || vscode.ViewColumn.One,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')]
            }
        );

        EmulatorPanel.currentPanel = new EmulatorPanel(panel, extensionUri);
    }

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;

        // Set the webview's initial html content
        this._update();

        // Listen for when the panel is disposed
        // This happens when the user closes the panel or when the panel is closed programmatically
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
    }

    public updateVideoFrame(base64Data: string) {
        this._panel.webview.postMessage({ command: 'videoFrame', data: base64Data });
    }

    public dispose() {
        EmulatorPanel.currentPanel = undefined;

        // Clean up our resources
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
                <title>SpectNet Emulator</title>
                <style>
                    body {
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        height: 100vh;
                        margin: 0;
                        background-color: #202020;
                    }
                    canvas {
                        border: 1px solid #444;
                        image-rendering: pixelated;
                    }
                </style>
            </head>
            <body>
                <canvas id="emulatorCanvas" width="352" height="296"></canvas>
                <script>
                    const vscode = acquireVsCodeApi();
                    const canvas = document.getElementById('emulatorCanvas');
                    const ctx = canvas.getContext('2d');
                    
                    // Buffer for decoding base64
                    // We expect 352x296 pixels, 4 bytes per pixel (RGBA)
                    // But the raw buffer from C# might be different. 
                    // Let's assume for now we receive a raw byte array encoded in base64.
                    // If it's raw ARGB/RGBA, we can put it into ImageData.
                    
                    window.addEventListener('message', event => {
                        const message = event.data;
                        switch (message.command) {
                            case 'videoFrame':
                                const base64 = message.data;
                                const binaryString = window.atob(base64);
                                const len = binaryString.length;
                                const bytes = new Uint8ClampedArray(len);
                                for (let i = 0; i < len; i++) {
                                    bytes[i] = binaryString.charCodeAt(i);
                                }
                                
                                // Create ImageData. 
                                // Note: The buffer size must match width * height * 4.
                                // 352 * 296 * 4 = 416768 bytes.
                                // If the buffer size is different, we might need to adjust dimensions.
                                // For now, let's try to create ImageData directly.
                                try {
                                    const imageData = new ImageData(bytes, 352, 296);
                                    ctx.putImageData(imageData, 0, 0);
                                } catch (e) {
                                    console.error('Error drawing frame:', e);
                                }
                                break;
                        }
                    });
                </script>
            </body>
            </html>`;
    }
}
