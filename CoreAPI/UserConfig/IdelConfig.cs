using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Currently, these are just some jotted down ideas. See NOTES.txt.
/// </summary>

namespace CoreAPI.UserConfig
{
    class IdelConfig
    {
        public class Trigger
        {
            /// <summary>
            /// A list of fnmatch strings that attempt to match the local filepath for this trigger.
            /// </summary>
            [JsonProperty("fnmatch")]
            public List<string> FnMatches { get; set; } = new() { "*" };

            /// <summary>
            /// A list of other trigger names (either local or global). If any of these triggers match,
            /// this trigger is disabled. Note that triggers listed here that are disabled by their own
            /// hide_option_when still disable this trigger as well.
            /// 
            /// Elements must fully match another option, or be a regex that can do so.
            /// </summary>
            [JsonProperty("hide_option_when")]
            public List<string> HideOptionWhen { get; set; } = new();

            [JsonProperty("editor")]
            public Editor Editor { get; set; }

            // TODO: Stretch goal
            /// <summary>
            /// A shell command with %url% denoting the clicked URL. Note that linkWheel.exe itself
            /// may have some useful commands here.
            /// 
            /// Examples:
            ///  "git diff $(linkWheel.exe get-git-expression --url %url%) $(linkWheel.exe get-local-file --url %url%)"
            ///  "$(linkWheel.exe get-local-file --url %url%)  # If the matched file is an executable, this would run it"
            /// </summary>
            //[JsonProperty("command")]
            //public string Command { get; set; }
        }

        public class Editor
        {
            /// <summary>
            /// The type of editor to open. Special values are as follows:
            /// * default: The default editor set for the file's extension, according to the user's Operating System.
            /// * nearest_workspace: The nearest sibling or ancestor sibling in the matching file's directory that
            ///       is a workspace file. For example, in the directory structure:
            ///     
            ///           foo.sln
            ///           Source/
            ///             bar.workspace
            ///             Config/
            ///               foo.csproj
            ///               matched_file.cs
            ///     
            ///      `foo.csproj` is the nearest sibling to the matched file that is considerd a workspace file, so
            ///      that workspace file is selected.
            ///      // TODO: Implement the following.
            ///      The files that are considered to be workspace files can be found in the application's
            ///      configuration directory in ide_workspaces.json.
            /// Additional types can be set in ide_workspaces.json. There are some handy defaults within that file
            /// already. If you think there's a common workspace configuration that was missed here, feel free to
            /// submit a pull request to add it to the list of defaults.
            /// </summary>
            [JsonProperty("type")]
            public string Type { get; set; }

            /// <summary>
            /// Overrides the specified type. Opens a workspace file. The path provided can either be relative to the
            /// repo root or absolute (use of absolute paths is only recommended for global/.user.idelconfig files). 
            /// </summary>
            [JsonProperty("workspacePath")]
            public string WorkspacePath { get; set; }
        }
    }
}
