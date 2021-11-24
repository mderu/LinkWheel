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
    // The command has been defined in the package.json file
    // Now provide the implementation of the command with registerCommand
    // The commandId parameter must match the command field in package.json
    let rightClickDisposable = vscode.commands.registerCommand('codelink.linkLine', () => {
        // The code you place here will be executed every time your command is executed
        // Display a message box to the user
        let startLine = vscode.window.activeTextEditor?.selection.start.line;
        let endLine = vscode.window.activeTextEditor?.selection.end.line;
        if (startLine != null && vscode.window.activeTextEditor != null) {
            startLine += 1;
            endLine = endLine ? endLine + 1 : endLine;
            cp.exec(`linkWheelCli register --path ${vscode.window.activeTextEditor?.document.fileName}`, (err, stdout, stderr) => {
                if (err) {
                    vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`);
                }
                else {
                    cp.exec(`linkWheelCli get-url `
                        + `--file ${vscode.window.activeTextEditor?.document.fileName} `
                        + `--start-line ${startLine}`
                        + (endLine == startLine ? "" : ` --end-line ${endLine}`), (err, stdout, stderr) => {
                        if (err) {
                            vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`);
                        }
                        else {
                            console.info(stdout.trim());
                            vscode.env.clipboard.writeText(stdout.trim());
                        }
                    });
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