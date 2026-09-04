namespace BCellResearchApp.Models;

public sealed class CloneAssessment
{
    public BCellClone Clone { get; set; } = new();
    public double LocalScore { get; set; }
    public double EvidenceBoost { get; set; }
    public double CombinedScore => Math.Clamp(LocalScore + EvidenceBoost, 0, 1);
    public string Classification { get; set; } = "not_candidate";
    public string Rationale { get; set; } = "";
    public int IedbMatches { get; set; }
    public string EvidenceStatus { get; set; } = "Not queried";

    // Research workflow state. These fields never actuate treatment or laboratory hardware.
    public bool EvidenceValidated { get; set; }
    public bool RiskGatePassed { get; set; }
    public bool HumanApproved { get; set; }
    public bool SimulationCompleted { get; set; }
    public string ValidationNotes { get; set; } = "Not validated";
    public string RiskNotes { get; set; } = "Not evaluated";
    public string ApprovalNotes { get; set; } = "Not approved";
    public string SimulationSummary { get; set; } = "Not simulated";
    public DateTimeOffset? LastUpdatedUtc { get; set; }

    public string WorkflowStage => SimulationCompleted ? "simulated"
        : HumanApproved ? "approved"
        : RiskGatePassed ? "risk-cleared"
        : EvidenceValidated ? "validated"
        : IedbMatches > 0 ? "evidence-found"
        : "screened";
}
