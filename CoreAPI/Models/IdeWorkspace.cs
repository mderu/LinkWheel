using Newtonsoft.Json;

namespace CoreAPI.Models
{
    // TODO: Have `get-actions` validate .idelconfig files with these objects in them.
    // TODO: Make paths relative to the directory they are defined in.

    /// <remarks>
    /// you can use the same variable substitution syntax
    /// that exists for global parameters. The following objects are available:
    ///
    ///  * request:
    ///      url: string  // The URL being exercised
    ///      file: string  // The absolute path to the file specified in the URL
    ///      startLine: int | null  // The first line number linked to, if any is specified.
    ///                             // You can check if it is specified with
    ///                             //   (=$.request[?(@.startLine != null)]=)
    ///                             // And get the value if so with:
    ///                             //   (=$.request[?(@.startLine != null)].startLine=)
    ///      endLine: int | null  // The last line number linked to. See startLine.
    ///  * idelConfig: any  // The contents of the JSON object with the type that matches
    ///                     // the name of this object (in this case "vscode").
    ///  * idelCommand: this  // This object. Format strings do not recurse.
    ///  * repoConfig: RepoConfig  // The RepoConfig object that corresponds to the request
    ///                            // This includes things like the // root, remoteUrl, etc.
    ///                            // See https://github.com/mderu/LinkWheel/blob/master/CoreAPI/Config/RepoConfig.cs
    /// </remarks>
    class IdeWorkspace // More like, any command really.
    {
        /// <summary>
        /// The priority at which this option shows up in the list of actions. Higher priority is earlier.
        /// </summary>
        [JsonProperty("priority")]
        public int Priority { get; set; } = 0;

        /// <summary>
        /// A new-line delimited list of fnmatch strings that accept files. At least one
        /// line must accept the file for the action to show up.
        /// Note that leading whitespace is trimmed.
        ///
        /// If fnmatch is unspecified, all files will automatically match.
        /// </summary>
        [JsonProperty("fnmatch")]
        public string FnMatches { get; set; } = "**";

        /// <summary>
        /// The command to run in running in a bash environment.
        /// </summary>
        [JsonProperty("bashCommand")]
        public string BashCommand { get; set; } = "code \"(=idelConfig.editor.WorkspacePath=)\" & ; sleep .1 ; code (=request.file=)";

        /// <summary>
        /// The command to run if running in a batch environment.
        /// </summary>
        [JsonProperty("batchCommand")]
        public string BatchCommand { get; set; } = "start /B code \"(=idelConfig.editor.WorkspacePath=)\" & powershell sleep .1 & start /B code (=request.file=):(=request.startLine=)";

        /// <summary>
        /// A brief name of what is opened when this action is selected.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// A longer description of what action is taken.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; } = "";

        /// <summary>
        /// A string containing any of the following:
        ///  * A path to an image
        ///  * A path to an executable or dll, with an optional r',\d+' denoting which icon to use.
        ///  * An https:// or http:// URI.
        /// Otherwise, the icon will be null, and a default missing icon will be shown in LinkWheel.
        /// </summary>
        [JsonProperty("iconSource")]
        public string Icon { get; set; } = "";

        /// <summary>
        /// A string containing any of the following:
        ///  * A path to an image
        ///  * A path to an executable or dll, with an optional r',\d+' denoting which icon to use.
        ///  * An https:// or http:// URI.
        /// Otherwise, the icon will be null, and a default missing icon will be shown in LinkWheel.
        /// </summary>
        [JsonProperty("iconSourceSecondary")]
        public string IconSecondary { get; set; } = "";
    }
}
