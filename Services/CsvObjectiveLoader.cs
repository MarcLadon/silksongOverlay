using System.Globalization;
using System.IO;
using SilksongOverlay.Models;

namespace SilksongOverlay.Services;

public sealed class CsvObjectiveLoader
{
    private readonly string _path;

    public CsvObjectiveLoader(string path)
    {
        _path = path;
    }

    public List<Objective> Load()
    {
        var objectives = new List<Objective>();

        if (!File.Exists(_path))
        {
            return objectives;
        }

        try
        {
            var lines = File.ReadAllLines(_path);

            if (lines.Length <= 1)
            {
                return objectives;
            }

            // Colonnes : Order,Name,Location,DetectionType,DetectionValue,ImagePath,Detail,MapImagePath
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = ParseCsvLine(line);

                if (parts.Count < 5)
                {
                    continue;
                }

                if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var order))
                {
                    continue;
                }

                var objective = new Objective
                {
                    Order = order,
                    Name = parts[1],
                    Location = parts[2],
                    DetectionType = parts[3],
                    DetectionValue = parts[4],
                    ImagePath = parts.Count > 5 && !string.IsNullOrWhiteSpace(parts[5]) ? parts[5] : null,
                    Detail = parts.Count > 6 ? parts[6] : string.Empty,
                    MapImagePath = parts.Count > 7 && !string.IsNullOrWhiteSpace(parts[7]) ? parts[7] : null,
                    IsCompleted = false
                };

                objectives.Add(objective);
            }

            objectives = objectives.OrderBy(x => x.Order).ToList();

            for (int i = 0; i < objectives.Count; i++)
            {
                objectives[i].Order = i;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement du CSV: {ex.Message}");
        }

        return objectives;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim().Trim('"'));
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString().Trim().Trim('"'));
        return result;
    }
}
