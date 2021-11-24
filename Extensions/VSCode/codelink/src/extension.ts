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
		let startLine = vscode.window.activeTextEditor?.selection.start.line;
		let endLine = vscode.window.activeTextEditor?.selection.end.line;

		if (startLine != null && vscode.window.activeTextEditor != null) {
			startLine += 1;
			endLine = endLine ? endLine + 1 : endLine;
			cp.exec(`linkWheelCli register --path ${vscode.window.activeTextEditor?.document.fileName}`,
			(err: any, stdout: string, stderr: string) => {
				if (err) {
					vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`)
				}
				else {
					cp.exec(
						`linkWheelCli get-url `
						 + `--file ${vscode.window.activeTextEditor?.document.fileName} `
						 + `--start-line ${startLine}`
						 + (endLine == startLine ? "": ` --end-line ${endLine}`),
						(err: any, stdout: string, stderr: string) => {
							if (err) {
								vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`)
							}
							else {
								console.info(stdout.trim())
								vscode.env.clipboard.writeText(stdout.trim())
							}
						});
				}
			});
		}
	});

	context.subscriptions.push(rightClickDisposable);
}

// this method is called when your extension is deactivated
export function deactivate() { }
