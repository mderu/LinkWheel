using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Text;

namespace CoreAPI.OutputFormat
{
    public class OutputFormatter
    {
        /// <summary>
        /// The left and right delimiters used to specify that what is in between is a JSONPath expression.
        /// This combination is the default because I can't think of any language where this is valid syntax.
        /// </summary>
        public string StartFormat { get; set; } = "(=";
        public string EndFormat { get; set; } = "=)";

        public string OutputArrayStart { get; set; } = "";  // "["
        public string OutputArrayEnd { get; set; } = "";  // "]"

        /// <summary>
        /// If null, uses the OS's default new line as a delimiter (i.e., \r\n or \n).
        /// </summary>
        public string? OutputDelimiter { protected get; set; } = null;

        public string OutputDelimiterValue => OutputDelimiter ?? Environment.NewLine;

        public string? FormatOverride { get; set; }

        public string GetOutput(OutputData outputObject, string? formatOverride = null)
        {
            int curIndex = 0;
            StringBuilder result = new();

            string format = formatOverride ?? FormatOverride ?? outputObject.Format;

            while (true)
            {
                int startExpressionIndex = format.IndexOf(StartFormat, curIndex);
                if (startExpressionIndex != -1)
                {
                    int endIndex = format.IndexOf(EndFormat, startExpressionIndex + OutputArrayStart.Length);
                    if (endIndex == -1)
                    {
                        throw new ArgumentException(
                            $"No matching end formatting delimiter `{StartFormat}` to match start " +
                            $"formatting delimiter `{EndFormat}` found at column {startExpressionIndex + 1}" + Environment.NewLine
                            + format + Environment.NewLine
                            + new string(' ', startExpressionIndex - 1) + "^");
                    }
                    result.Append(format[curIndex..startExpressionIndex]);

                    string jsonPathQuery = format[(startExpressionIndex + StartFormat.Length)..endIndex];
                    var tokens = JObject.FromObject(outputObject.Objects).SelectTokens(jsonPathQuery);
                    result.Append(OutputArrayStart);
                    result.AppendJoin(OutputDelimiterValue,
                        tokens.Select(token =>
                        {
                            if (token.Type == JTokenType.String)
                            {
                                return token.ToString();
                            }
                            return token.ToString(Formatting.None);
                        }));
                    result.Append(OutputArrayEnd);

                    curIndex = endIndex + EndFormat.Length;
                }
                else
                {
                    result.Append(format[curIndex..]);
                    break;
                }
            }
            return result.ToString();
        }
    }
}
