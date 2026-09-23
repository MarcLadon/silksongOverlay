using SilksongOverlay.Models;
using SilksongOverlay.Services;

namespace SilksongOverlay.Core;

public sealed class ConfigurationService
{
    private readonly JsonFileStore _store;

    public ConfigurationService(string configPath)
    {
        _store = new JsonFileStore(configPath);
    }

    public OverlayConfiguration Load()
    {
        return _store.Load();
    }

    public void Save(OverlayConfiguration configuration)
    {
        _store.Save(configuration);
    }
}
