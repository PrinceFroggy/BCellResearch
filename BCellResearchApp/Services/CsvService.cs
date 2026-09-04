using BCellResearchApp.Models;
using System.Globalization;

namespace BCellResearchApp.Services;

public static class CsvService
{
    public static List<BCellClone> Load(string path)
    {
        var result = new List<BCellClone>();
        foreach (var line in File.ReadLines(path).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var p = line.Split(',');
            if (p.Length < 5) continue;
            result.Add(new BCellClone
            {
                CloneId = p[0].Trim(),
                AntibodySequence = p[1].Trim(),
                Antigen = p[2].Trim(),
                MemoryCell = bool.TryParse(p[3], out var m) && m,
                Confidence = double.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var c) ? c : 0
            });
        }
        return result;
    }
}
