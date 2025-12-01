const vscode = acquireVsCodeApi();

document.addEventListener('DOMContentLoaded', () => {
    const keys = document.querySelectorAll('.key');
    keys.forEach(key => {
        key.addEventListener('click', () => {
            const code = key.getAttribute('data-code');
            const main = key.getAttribute('data-main');
            const keyword = key.getAttribute('data-keyword');
            
            vscode.postMessage({
                command: 'keyClicked',
                code: code,
                main: main,
                keyword: keyword
            });
        });
    });
});
