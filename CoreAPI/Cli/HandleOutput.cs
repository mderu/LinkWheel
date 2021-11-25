using CommandLine;
using CoreAPI.OutputFormat;

namespace CoreAPI.Cli
{
    [Verb("handle-output")]
    public class HandleOutput
    {
        [Option("format")]
        public string? Format { get; set; } = null;

        [Option("output-delimiter")]
        public string? OutputDelimiter { get; set; }

        [Option("oas", HelpText = "Output Array Start. The character(s) to output at the beginning of a returned array")]
        public string? OutputArrayStart { get; set; }

        [Option("aoe", HelpText = "Output Array End. The character(s) to output at the end of the returned array")]
        public string? OutputArrayEnd { get; set; }

        [Option("fss", HelpText = "Format String Start. The character(s) to mark the start of a JSON Path expression.")]
        public string? FormatStringStart { get; set; }

        [Option("fse", HelpText = "Format String End. The character(s) to mark the end of a JSON Path expression.")]
        public string? FormatStringEnd { get; set; }

        public OutputFormatter Create()
        {
            OutputFormatter outputFormatter = new();

            if (Format is not null)
            {
                outputFormatter.FormatOverride = Format;
            }

            if (OutputDelimiter is not null)
            {
                outputFormatter.OutputDelimiter = Format;
            }

            if (OutputArrayStart is not null)
            {
                outputFormatter.OutputArrayStart = OutputArrayStart;
            }

            if (OutputArrayEnd is not null)
            {
                outputFormatter.OutputArrayEnd = OutputArrayEnd;
            }

            if (FormatStringStart is not null)
            {
                outputFormatter.StartFormat = FormatStringStart;
            }

            if (FormatStringEnd is not null)
            {
                outputFormatter.EndFormat = FormatStringEnd;
            }

            return outputFormatter;
        }
    }
}
