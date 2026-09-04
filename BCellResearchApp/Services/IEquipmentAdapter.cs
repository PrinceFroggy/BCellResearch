using BCellResearchApp.Models;

namespace BCellResearchApp.Services;

public interface IEquipmentAdapter
{
    EquipmentProfile Profile { get; }
    EquipmentConnectionState State { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    EquipmentTelemetry GetTelemetry();
    IReadOnlyList<DryRunJob> GetJobs();
    Task<DryRunJob> SubmitDryRunAsync(CloneAssessment assessment, CancellationToken cancellationToken = default);
    Task<DryRunJob?> AdvanceDryRunAsync(string jobId, CancellationToken cancellationToken = default);
    void SetInterlock(bool healthy);
    string ExportAuditJson();
}
