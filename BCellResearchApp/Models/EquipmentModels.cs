namespace BCellResearchApp.Models;

public enum EquipmentConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Faulted,
    Interlocked
}

public sealed class EquipmentProfile
{
    public string EquipmentId { get; set; } = "SIM-LAB-001";
    public string DisplayName { get; set; } = "Simulated Cell-Processing Controller";
    public string Vendor { get; set; } = "Research Simulator";
    public string Model { get; set; } = "Digital Twin v1";
    public string Endpoint { get; set; } = "sim://localhost/cell-controller";
    public List<string> Capabilities { get; set; } = new()
    {
        "candidate-receipt",
        "dry-run-job-queue",
        "telemetry",
        "interlock-status",
        "acknowledgement",
        "audit-export"
    };
}

public sealed class EquipmentTelemetry
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public EquipmentConnectionState State { get; set; }
    public bool InterlockHealthy { get; set; }
    public int QueueDepth { get; set; }
    public string ActiveJobId { get; set; } = "none";
    public string StatusMessage { get; set; } = "Idle";
}

public sealed class DryRunJob
{
    public string JobId { get; set; } = $"DRY-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
    public string CandidateId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Mode { get; set; } = "NON_EXECUTABLE_SIMULATION";
    public string RequestedAction { get; set; } = "Simulate downstream equipment workflow";
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string Status { get; set; } = "Queued";
    public List<string> Events { get; set; } = new();
}
