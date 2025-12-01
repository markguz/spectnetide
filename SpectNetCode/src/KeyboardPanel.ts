import * as vscode from 'vscode';
import * as path from 'path';

export class KeyboardPanel {
    public static currentPanel: KeyboardPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;

        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);

        this._panel.webview.html = this._getHtmlForWebview(this._panel.webview);

        this._panel.webview.onDidReceiveMessage(
            message => {
                switch (message.command) {
                    case 'keyClicked':
                        this._handleKeyClick(message);
                        return;
                }
            },
            null,
            this._disposables
        );
    }

    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        if (KeyboardPanel.currentPanel) {
            KeyboardPanel.currentPanel._panel.reveal(column);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'spectNetKeyboard',
            'ZX Spectrum Keyboard',
            column || vscode.ViewColumn.One,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'src', 'webview')]
            }
        );

        KeyboardPanel.currentPanel = new KeyboardPanel(panel, extensionUri);
    }

    private _handleKeyClick(message: any) {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            return;
        }

        let textToInsert = '';
        if (message.keyword) {
            textToInsert = message.keyword;
        } else if (message.main) {
            textToInsert = message.main;
        }

        if (textToInsert) {
            editor.edit(editBuilder => {
                editBuilder.insert(editor.selection.active, textToInsert);
            });
        }
    }

    public dispose() {
        KeyboardPanel.currentPanel = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            const x = this._disposables.pop();
            if (x) {
                x.dispose();
            }
        }
    }

    private _getHtmlForWebview(webview: vscode.Webview) {
        const scriptPathOnDisk = vscode.Uri.joinPath(this._extensionUri, 'src', 'webview', 'keyboard.js');
        const scriptUri = webview.asWebviewUri(scriptPathOnDisk);

        const stylePathOnDisk = vscode.Uri.joinPath(this._extensionUri, 'src', 'webview', 'keyboard.css');
        const styleUri = webview.asWebviewUri(stylePathOnDisk);

        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link href="${styleUri}" rel="stylesheet">
    <title>ZX Spectrum Keyboard</title>
</head>
<body>
    <div class="keyboard-row">
        ${this._createKey("1", "1", "!", "EDIT", "DEF FN", "blue")}
        ${this._createKey("2", "2", "@", "CAPS LOCK", "FN", "red")}
        ${this._createKey("3", "3", "#", "TRUE VIDEO", "LINE", "magenta")}
        ${this._createKey("4", "4", "$", "INV VIDEO", "OPEN #", "green")}
        ${this._createKey("5", "5", "%", "◄", "CLOSE #", "cyan")}
        ${this._createKey("6", "6", "&", "▼", "MOVE", "yellow")}
        ${this._createKey("7", "7", "'", "▲", "ERASE", "white")}
        ${this._createKey("8", "8", "(", "►", "POINT", "white")}
        ${this._createKey("9", "9", ")", "GRAPHICS", "CAT", "white")}
        ${this._createKey("0", "0", "_", "DELETE", "FORMAT", "black")}
    </div>
    <div class="keyboard-row">
        ${this._createKey("Q", "Q", "<=", "SIN", "ASN", "", "PLOT")}
        ${this._createKey("W", "W", "<>", "COS", "ACS", "", "DRAW")}
        ${this._createKey("E", "E", ">=", "TAN", "ATN", "", "REM")}
        ${this._createKey("R", "R", "<", "INT", "VERIFY", "", "RUN")}
        ${this._createKey("T", "T", ">", "RND", "MERGE", "", "RAND")}
        ${this._createKey("Y", "Y", "AND", "STR$", "[", "", "RETURN")}
        ${this._createKey("U", "U", "OR", "CHR$", "]", "", "IF")}
        ${this._createKey("I", "I", "AT", "CODE", "IN", "", "INPUT")}
        ${this._createKey("O", "O", ";", "PEEK", "OUT", "", "POKE")}
        ${this._createKey("P", "P", "\"", "TAB", "(C)", "", "PRINT")}
    </div>
    <div class="keyboard-row">
        ${this._createKey("A", "A", "STOP", "READ", "~", "", "NEW")}
        ${this._createKey("S", "S", "NOT", "RESTORE", "|", "", "SAVE")}
        ${this._createKey("D", "D", "STEP", "DATA", "\\", "", "DIM")}
        ${this._createKey("F", "F", "TO", "SGN", "{", "", "FOR")}
        ${this._createKey("G", "G", "THEN", "ABS", "}", "", "GOTO")}
        ${this._createKey("H", "H", "↑", "SQR", "CIRCLE", "", "GOSUB")}
        ${this._createKey("J", "J", "-", "VAL", "VAL$", "", "LOAD")}
        ${this._createKey("K", "K", "+", "LEN", "SCREEN$", "", "LIST")}
        ${this._createKey("L", "L", "=", "USR", "ATTR", "", "LET")}
        <div class="key key-enter" data-code="Enter" data-keyword="ENTER">
            <span class="key-main">ENTER</span>
        </div>
    </div>
    <div class="keyboard-row">
        <div class="key key-caps" data-code="CShift">
            <span class="key-main">CAPS SHIFT</span>
        </div>
        ${this._createKey("Z", "Z", ":", "LN", "BEEP", "", "COPY")}
        ${this._createKey("X", "X", "£", "EXP", "INK", "", "CLEAR")}
        ${this._createKey("C", "C", "?", "LPRINT", "PAPER", "", "CONT")}
        ${this._createKey("V", "V", "/", "LLIST", "FLASH", "", "CLS")}
        ${this._createKey("B", "B", "*", "BIN", "BRIGHT", "", "BORDER")}
        ${this._createKey("N", "N", ",", "INKEY$", "OVER", "", "NEXT")}
        ${this._createKey("M", "M", ".", "PI", "INVERSE", "", "PAUSE")}
        <div class="key key-sym" data-code="SShift">
            <span class="key-main">SYMBOL SHIFT</span>
        </div>
        <div class="key key-space" data-code="Space" data-main=" ">
            <span class="key-main">SPACE</span>
        </div>
    </div>
    <script src="${scriptUri}"></script>
</body>
</html>`;
    }

    private _createKey(code: string, main: string, sshift: string, ext: string, extshift: string, color: string = "", keyword: string = "") {
        const colorClass = color ? `color-${color}` : "";
        const keywordAttr = keyword ? `data-keyword="${keyword}"` : "";
        return `
        <div class="key ${colorClass}" data-code="${code}" data-main="${main}" ${keywordAttr}>
            <span class="key-sshift">${sshift}</span>
            <span class="key-ext">${ext}</span>
            <span class="key-main">${main}</span>
            <span class="key-keyword">${keyword}</span>
        </div>`;
    }
}
