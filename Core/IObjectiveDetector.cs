namespace SilksongOverlay.Core;

public interface IObjectiveDetector : IDisposable
{
    event EventHandler<ObjectiveDetectedEventArgs>? ObjectiveDetected;

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync();
}
