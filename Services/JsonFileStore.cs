using System.Text.Json;
using SilksongOverlay.Models;

namespace SilksongOverlay.Services;

public sealed class JsonFileStore
{
    private readonly string _path;

    public JsonFileStore(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
    }

    public OverlayConfiguration Load()
    {
        if (!File.Exists(_path))
        {
            return new OverlayConfiguration();
        }

        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<OverlayConfiguration>(json) ?? new OverlayConfiguration();
    }

    public void Save(OverlayConfiguration config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_path, json);
    }
}
