using System.Text.Json;

namespace SilksongOverlay.Core;

public sealed class FileObjectiveDetector : IObjectiveDetector
{
    private readonly string _path;
    private CancellationTokenSource? _stopSource;

    public FileObjectiveDetector(string path)
    {
        _path = path;
    }

    public event EventHandler<ObjectiveDetectedEventArgs>? ObjectiveDetected;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _stopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");

        string? lastContent = null;

        try
        {
            while (!_stopSource.Token.IsCancellationRequested)
            {
                if (File.Exists(_path))
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(_path, _stopSource.Token);

                        if (!string.Equals(content, lastContent, StringComparison.Ordinal))
                        {
                            lastContent = content;
                            var detectedEvent = JsonSerializer.Deserialize<DetectedEvent>(content);

                            if (detectedEvent is not null &&
                                !string.IsNullOrWhiteSpace(detectedEvent.Type) &&
                                !string.IsNullOrWhiteSpace(detectedEvent.Value))
                            {
                                ObjectiveDetected?.Invoke(
                                    this,
                                    new ObjectiveDetectedEventArgs(
                                        detectedEvent.Type,
                                        detectedEvent.Value));
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Le fichier peut être momentanément incomplet pendant son écriture.
                    }
                    catch (IOException)
                    {
                        // Le fichier peut être verrouillé momentanément par un autre processus.
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500), _stopSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Arrêt normal du détecteur.
        }
    }

    public Task StopAsync()
    {
        _stopSource?.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _stopSource?.Cancel();
        _stopSource?.Dispose();
    }

    private sealed class DetectedEvent
    {
        public string Type { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
