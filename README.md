# LinkWheel

LinkWheel is a tool that intercepts links to remote repository websites (such
as GitHub, GitLab, or Helix Swarm), and offers the user the ability to access 
their local copy of those files directly through different actions. With the
default global configuration, users are given the options to either open the
link in their browser, reveal the file in their file explorer, open the file
directly (as if they double clicked the file within the file explorer), or
open the file in Visual Studio (if applicable). The list of actions a user is
given is customizable, and can be defined on a per-repository basis.

## Examples

Below are some examples of what clicking on some links in Discord may look like:

### Open in your Default Web Browser

Sometimes, you don't want to actually intercept the link, and you just want to
visit the website. This option allows you to do so. This option is always
present.

[!(Open the Default Browser)](https://user-images.githubusercontent.com/5975215/147231986-18a7d3af-1a05-40c3-8683-bd39893089f6.mov)

### Open in Visual Studio

Opens the linked file in the currently-running instance of Visual Studio, or a
new instance if Visual Studio is not already open.

[!(Open in Visual Studio)](https://user-images.githubusercontent.com/5975215/147233303-f8303b39-e5d7-48b1-a80a-109faef7f7b3.mov)

### Open File

Behaves as if you double-clicked the file in your File Explorer.

[!(Open File)](https://user-images.githubusercontent.com/5975215/147233409-7c068b9b-7d60-4b6b-9137-0f7896a9b05a.mov)

### Show in Explorer

Reveals the file in your File Explorer

[!(Show In Explorer)](https://user-images.githubusercontent.com/5975215/147233443-ee6b4976-4024-4f9c-a060-f8be655894d7.mov)

## Supported Websites

Unfortunately, many repo hosting websites have different ways of formatting
their URLs. The following websites are supported:

Perforce
* Swarm

Git
* GitHub (including GitHub Enterprise websites)
* GitLab

## Setup/Flow

First, install LinkWheel via the WinInstaller found in the releases. Then,
check each part that you want to have installed. If you don't want to write any
configuration yourself, the default global configuration is a good default.
Then, follow along with any installers that are opened. Installation is
complete when a dialog box appears that tells you to close the installer.

In order for these LinkWheel to work, we first need to register the location of
the repositories you have locally. If you intend on using the IDE extensions,
skip to those instructions below. 

You can register repos by opening LinkWheel directly, and then right-click the
LinkWheel System Tray Icon and click `Register Repo`. If you prefer to use
commandline, you can run the following instead:

```
LinkWheelCli register --path C:\path\to\your\repo\or\any\file\within\the\repo
```

## IDE Extensions

There are IDE extensions for both VS Code and Visual Studio, simply check the
option for the extension within the Windows Installer.

To register a repo, simply right click any line of code in the editor and press
`Copy Link To Clipboard`. This will automatically register the repo if it is
not already registered, and copy a link into your clipboard.

For code hosted on websites such as GitHub, you can select multiple lines of
code and then press `Copy Link To Clipboard` to get an exact link to those
lines.

## Limitations

* **Links are not incercepted when they are clicked on within your browser** 
  (otherwise, clicking on anything in GitHub or Swarm would be a hassle). Note
  that clicking on links in browser-based applications (Discord, Slack, Desktop
  Google Hangouts, etc) still intercepts as expected.
  
  There are some ideas to make a browser extension to allow users to open the
  wheel, but nothing is planned just yet. One workaround for this is to bind 
  `LinkWheelCli`'s `serve-clipboard` command to a keyboard shortcut, which
  would open LinkWheel for any valid link.

* **Windows is the only supported operating system.** I would like to support
  Linux, but my work is done in Windows (or through a terminal), so I haven't
  found myself in a situation that requires Linux support just yet. Any PRs
  aimed at supporting Linux would be greatly appreciated.

* **Links to do not support linking to exact commits or CL numbers**. This is a
  useful but missing feature, but also one that may need some more thinking to
  properly accomplish within IDEs (Do users always want to link to the commit
  they are on? When do they prefer to link to HEAD? How should the CLI handle
  this?).

* **Opening links in Visual Studio doesn't open the file in the correct
  solution.** I have yet to find any evidence that this is possible. If anyone
  has found a fix for this, please let me know and I can update the default 
  global config to support this.

## Known Issues:

* For JSONPath queries, `@.length` isn't supported. This is a 
  [missing feature](https://github.com/JamesNK/Newtonsoft.Json/issues/1318) in
  Newtonsoft.Json. If you'd like to fix that, please fix it upstream within
  that project.
