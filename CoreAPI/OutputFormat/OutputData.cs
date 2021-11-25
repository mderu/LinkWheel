using System.Collections.Generic;

namespace CoreAPI.OutputFormat
{
    public class OutputData
    {
        public int ExitCode { get; set; } = 0;
        public Dictionary<string, object> Objects { get; set; }

        public string Format
        {
            get => Objects.GetValueOrDefault("format", "(=$=)").ToString()!;
            set => Objects["format"] = value;
        }

        public OutputData(int exitCode, Dictionary<string, object> objects, string? format = null)
        {
            ExitCode = exitCode;
            Objects = objects;
            if (format is not null)
            {
                Format = format;
            }
        }
    }
}
