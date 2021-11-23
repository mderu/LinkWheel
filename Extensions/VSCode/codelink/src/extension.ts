// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import * as cp from 'child_process';

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {

	// The command has been defined in the package.json file
	// Now provide the implementation of the command with registerCommand
	// The commandId parameter must match the command field in package.json
	let rightClickDisposable = vscode.commands.registerCommand('codelink.linkLine', () => {
		// The code you place here will be executed every time your command is executed
		// Display a message box to the user
		let currentLine = vscode.window.activeTextEditor?.selection.active.line;

		if (currentLine != null && vscode.window.activeTextEditor != null) {
			cp.exec(`linkWheelCli get-url --file ${vscode.window.activeTextEditor?.document.fileName} --start-line ${currentLine}`,
				(err: any, stdout: string, stderr: string) => {
					if (err) {
						vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`)
					}
					else {
						vscode.env.clipboard.writeText(stdout.trim())
					}
				});
		}
	});

	context.subscriptions.push(rightClickDisposable);
}

// this method is called when your extension is deactivated
export function deactivate() { }
