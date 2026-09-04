using BCellResearchApp.Models;
using System.Text.Json;

namespace BCellResearchApp.Services;

public sealed class SimulatedEquipmentAdapter : IEquipmentAdapter
{
    private readonly List<DryRunJob> _jobs = new();
    private readonly List<object> _audit = new();
    private bool _interlockHealthy = true;

    public EquipmentProfile Profile { get; } = new();
    public EquipmentConnectionState State { get; private set; } = EquipmentConnectionState.Disconnected;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        State = EquipmentConnectionState.Connecting;
        Audit("connect_requested", "Simulator connection requested");
        await Task.Delay(350, cancellationToken);
        State = _interlockHealthy ? EquipmentConnectionState.Connected : EquipmentConnectionState.Interlocked;
        Audit("connected", $"Connected to {Profile.Endpoint}");
    }

    public Task DisconnectAsync()
    {
        State = EquipmentConnectionState.Disconnected;
        Audit("disconnected", "Simulator disconnected");
        return Task.CompletedTask;
    }

    public EquipmentTelemetry GetTelemetry()
    {
        var active = _jobs.LastOrDefault(j => j.Status is "Queued" or "Accepted" or "Simulating");
        return new EquipmentTelemetry
        {
            State = State,
            InterlockHealthy = _interlockHealthy,
            QueueDepth = _jobs.Count(j => j.Status is "Queued" or "Accepted" or "Simulating"),
            ActiveJobId = active?.JobId ?? "none",
            StatusMessage = !_interlockHealthy ? "Interlock open — submissions blocked" : State == EquipmentConnectionState.Connected ? "Ready (dry-run only)" : State.ToString()
        };
    }

    public IReadOnlyList<DryRunJob> GetJobs() => _jobs.AsReadOnly();

    public Task<DryRunJob> SubmitDryRunAsync(CloneAssessment assessment, CancellationToken cancellationToken = default)
    {
        if (State != EquipmentConnectionState.Connected)
            throw new InvalidOperationException("Equipment simulator is not connected.");
        if (!_interlockHealthy)
            throw new InvalidOperationException("Equipment simulator interlock is open.");
        if (!assessment.HumanApproved || !assessment.RiskGatePassed || !assessment.EvidenceValidated)
            throw new InvalidOperationException("Candidate must pass evidence validation, risk gate, and human approval before dry-run submission.");

        var job = new DryRunJob
        {
            CandidateId = assessment.Clone.CloneId,
            Metadata = new Dictionary<string, string>
            {
                ["classification"] = assessment.Classification,
                ["combined_score"] = assessment.CombinedScore.ToString("0.000"),
                ["antigen_annotation"] = assessment.Clone.Antigen,
                ["safety_boundary"] = "No executable biological, dosing, editing, or device-control parameters included"
            }
        };
        job.Events.Add($"{DateTimeOffset.UtcNow:o} queued as non-executable simulation envelope");
        _jobs.Add(job);
        Audit("job_queued", new { job.JobId, job.CandidateId, job.Mode });
        return Task.FromResult(job);
    }

    public async Task<DryRunJob?> AdvanceDryRunAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var job = _jobs.FirstOrDefault(j => j.JobId == jobId);
        if (job is null) return null;
        if (!_interlockHealthy)
        {
            job.Status = "Held by interlock";
            job.Events.Add($"{DateTimeOffset.UtcNow:o} held because interlock opened");
            Audit("job_interlocked", job.JobId);
            return job;
        }

        string next = job.Status switch
        {
            "Queued" => "Accepted",
            "Accepted" => "Simulating",
            "Simulating" => "Completed",
            _ => job.Status
        };
        await Task.Delay(250, cancellationToken);
        job.Status = next;
        job.Events.Add($"{DateTimeOffset.UtcNow:o} state => {next}");
        Audit("job_state", new { job.JobId, job.Status });
        return job;
    }

    public void SetInterlock(bool healthy)
    {
        _interlockHealthy = healthy;
        if (!healthy && State == EquipmentConnectionState.Connected)
            State = EquipmentConnectionState.Interlocked;
        else if (healthy && State == EquipmentConnectionState.Interlocked)
            State = EquipmentConnectionState.Connected;
        Audit("interlock", healthy ? "healthy" : "open");
    }

    public string ExportAuditJson() => JsonSerializer.Serialize(new
    {
        profile = Profile,
        generated_at = DateTimeOffset.UtcNow,
        safety_mode = "NON_EXECUTABLE_SIMULATION_ONLY",
        audit = _audit,
        jobs = _jobs
    }, new JsonSerializerOptions { WriteIndented = true });

    private void Audit(string type, object detail) => _audit.Add(new
    {
        at = DateTimeOffset.UtcNow,
        type,
        detail
    });
}
