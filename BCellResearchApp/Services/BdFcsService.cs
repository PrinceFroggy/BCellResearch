using BCellResearchApp.Models;
using System.Globalization;
using System.Text;

namespace BCellResearchApp.Services;

public sealed class BdFcsService
{
    public BdIntegrationProfile Profile { get; } = new();

    public BdFlowFileSummary ReadSummary(string path)
    {
        using var fs = File.OpenRead(path);
        if (fs.Length < 58) throw new InvalidDataException("File is too small to be a valid FCS file.");

        var header = new byte[58];
        fs.ReadExactly(header);
        var ascii = Encoding.ASCII.GetString(header);
        if (!ascii.StartsWith("FCS", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The selected file does not begin with an FCS header.");

        long ParseHeaderLong(int start, int length)
        {
            var s = ascii.Substring(start, length).Trim();
            return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

        var summary = new BdFlowFileSummary
        {
            FileName = Path.GetFileName(path),
            FcsVersion = ascii[..6].Trim(),
            TextStart = ParseHeaderLong(10, 8),
            TextEnd = ParseHeaderLong(18, 8),
            DataStart = ParseHeaderLong(26, 8),
            DataEnd = ParseHeaderLong(34, 8)
        };

        if (summary.TextStart <= 0 || summary.TextEnd < summary.TextStart || summary.TextEnd >= fs.Length)
            throw new InvalidDataException("FCS TEXT segment offsets are invalid or unsupported.");

        fs.Position = summary.TextStart;
        var textLength = checked((int)(summary.TextEnd - summary.TextStart + 1));
        var textBytes = new byte[textLength];
        fs.ReadExactly(textBytes);
        var text = Encoding.ASCII.GetString(textBytes);
        if (text.Length < 2) throw new InvalidDataException("FCS TEXT segment is empty.");

        var delimiter = text[0];
        var tokens = SplitEscaped(text.AsSpan(1), delimiter);
        for (var i = 0; i + 1 < tokens.Count; i += 2)
        {
            var key = tokens[i].Trim();
            var value = tokens[i + 1].Trim();
            if (key.Length > 0) summary.Keywords[key] = value;
        }

        summary.ParameterCount = GetInt(summary.Keywords, "$PAR");
        summary.EventCount = GetLong(summary.Keywords, "$TOT");
        summary.Cytometer = Get(summary.Keywords, "$CYT");
        summary.SampleId = Get(summary.Keywords, "$SMNO", "$SRC", "SAMPLE ID");
        summary.Date = Get(summary.Keywords, "$DATE");

        for (var p = 1; p <= summary.ParameterCount; p++)
        {
            summary.Parameters.Add(new BdFlowParameter
            {
                Index = p,
                Name = Get(summary.Keywords, $"$P{p}N"),
                Stain = Get(summary.Keywords, $"$P{p}S"),
                Bits = GetInt(summary.Keywords, $"$P{p}B"),
                Range = GetDouble(summary.Keywords, $"$P{p}R")
            });
        }

        return summary;
    }

    private static List<string> SplitEscaped(ReadOnlySpan<char> input, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == delimiter)
            {
                if (i + 1 < input.Length && input[i + 1] == delimiter)
                {
                    current.Append(delimiter);
                    i++;
                }
                else
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
            }
            else current.Append(c);
        }
        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }

    private static string Get(Dictionary<string, string> d, params string[] keys)
    {
        foreach (var key in keys) if (d.TryGetValue(key, out var v)) return v;
        return "";
    }
    private static int GetInt(Dictionary<string, string> d, string key) => int.TryParse(Get(d, key), out var v) ? v : 0;
    private static long GetLong(Dictionary<string, string> d, string key) => long.TryParse(Get(d, key), out var v) ? v : 0;
    private static double GetDouble(Dictionary<string, string> d, string key) => double.TryParse(Get(d, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;
}
