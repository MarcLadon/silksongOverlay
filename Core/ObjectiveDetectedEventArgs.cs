namespace SilksongOverlay.Core;

public sealed class ObjectiveDetectedEventArgs : EventArgs
{
    public ObjectiveDetectedEventArgs(string eventType, string value)
    {
        EventType = eventType;
        Value = value;
    }

    public string EventType { get; }

    public string Value { get; }
}
