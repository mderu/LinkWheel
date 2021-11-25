// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import * as cp from 'child_process';
import * as pathUtils from 'path';


let registeredPaths: Set<string> = new Set();

// Technically it could be registered, but the plugin doesn't know yet.
function isRegistered(path : string) {
	for (let registeredPath of registeredPaths) {
		let relative = pathUtils.relative(registeredPath, path);
		if (relative && !relative.startsWith('..') && !pathUtils.isAbsolute(relative)) {
			return true;
		}
	}
	return false;
}


function copyLinkToClipboard(path : string, startLine : number, endLine : number) {
	cp.exec(
		`linkWheelCli get-url `
		 + `--file ${path} `
		 + `--start-line ${startLine}`
		 + (endLine == startLine ? "": ` --end-line ${endLine}`),
		(err: any, stdout: string, stderr: string) => {
			if (err) {
				vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`);
			}
			else {
				console.info(`Copying ${stdout.trim()} to clipboard`);
				vscode.env.clipboard.writeText(stdout.trim());
			}
		});
}


// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {

	// The command has been defined in the package.json file
	// Now provide the implementation of the command with registerCommand
	// The commandId parameter must match the command field in package.json
	let rightClickDisposable = vscode.commands.registerCommand('codelink.linkLine', () => {
		// The code you place here will be executed every time your command is executed
		// Display a message box to the user
		let editor = vscode.window.activeTextEditor;

		if (editor == null) {
			vscode.window.showErrorMessage(`Unable to link: no active text editor.`);
			return;
		}

		// Magic Number Explanation: the lines come back zero-indexed, but UIs start with line 1 instead of 0.
		let startLine = editor.selection.start.line + 1;
		let endLine = editor.selection.end.line + 1;

		if (vscode.window.activeTextEditor != null) {
			let path: string = vscode.window.activeTextEditor?.document.fileName;
			if (isRegistered(path)) {
				copyLinkToClipboard(
					path,
					startLine,
					endLine
				);
			}
			else {
				cp.exec(`linkWheelCli register --path ${path}`,
				(err: any, stdout: string, stderr: string) => {
					if (err) {
						vscode.window.showErrorMessage(`Unable to link to the given line: ${err}: ${stderr}`)
					}
					else {
						let repoConfig = JSON.parse(stdout)
						registeredPaths.add(repoConfig.root)

						copyLinkToClipboard(
							path,
							startLine,
							endLine
						);
					}
				});
			}
		}
	});

	context.subscriptions.push(rightClickDisposable);
}

// this method is called when your extension is deactivated
export function deactivate() { }
