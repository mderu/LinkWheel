To create the installer, you need to do the following tasks by hand:

1. Create a VSIX for the VSCode extension
2. Place the extension at Resources/linkWheelVSIX_VSCode.vsix
3. Compile the LinkWheelVS project using the Release config.
4. Copy the .vsix file from the bin/Release directory to Resources/linkWheelVSIX_VS.vsix
5. Publish LinkWheel and LinkWheelCli
6. Copy the contents of LinkWheelCli into the LinkWheel publish directory, skipping files that already exist.
7. Zip up the publish directory, and move it to Resources/linkWheel.zip
8. Publish WinInstaller