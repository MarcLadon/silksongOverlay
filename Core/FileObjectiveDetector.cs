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

        string? lastContent = null;

        while (!_stopSource.Token.IsCancellationRequested)
        {
            if (File.Exists(_path))
            {
                var content = await File.ReadAllTextAsync(_path, _stopSource.Token);

                if (!string.Equals(content, lastContent))
                {
                    lastContent = content;

                    var detectedEvent = JsonSerializer.Deserialize<DetectedEvent>(content);

                    if (detectedEvent is not null)
                    {
                        ObjectiveDetected?.Invoke(
                            this,
                            new ObjectiveDetectedEventArgs(
                                detectedEvent.Type,
                                detectedEvent.Value));
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), _stopSource.Token);
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
