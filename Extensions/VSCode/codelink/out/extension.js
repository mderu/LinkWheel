"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.deactivate = exports.activate = void 0;
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
const vscode = require("vscode");
const cp = require("child_process");
// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
function activate(context) {
    // Use the console to output diagnostic information (console.log) and errors (console.error)
    // This line of code will only be executed once when your extension is activated
    console.log('Congratulations, your extension "codelink" is now active!');
    // The command has been defined in the package.json file
    // Now provide the implementation of the command with registerCommand
    // The commandId parameter must match the command field in package.json
    let disposable = vscode.commands.registerCommand('codelink.helloWorld', () => {
        // The code you place here will be executed every time your command is executed
        // Display a message box to the user
        vscode.window.showInformationMessage('Hello World from CodeLink!');
    });
    context.subscriptions.push(disposable);
    // The command has been defined in the package.json file
    // Now provide the implementation of the command with registerCommand
    // The commandId parameter must match the command field in package.json
    let rightClickDisposable = vscode.commands.registerCommand('codelink.linkLine', () => {
        // The code you place here will be executed every time your command is executed
        // Display a message box to the user
        let currentLine = vscode.window.activeTextEditor?.selection.active.line;
        if (currentLine != null && vscode.window.activeTextEditor != null) {
            cp.exec(`linkWheelCli get-url --file ${vscode.window.activeTextEditor?.document.fileName} --start-line ${currentLine}`, (err, stdout, stderr) => {
                if (err) {
                    vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`);
                }
                else {
                    vscode.env.clipboard.writeText(stdout.trim());
                }
            });
        }
    });
    context.subscriptions.push(rightClickDisposable);
}
exports.activate = activate;
// this method is called when your extension is deactivated
function deactivate() { }
exports.deactivate = deactivate;
//# sourceMappingURL=extension.js.map