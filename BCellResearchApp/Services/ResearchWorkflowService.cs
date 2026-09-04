using BCellResearchApp.Models;
using System.Text.Json;

namespace BCellResearchApp.Services;

public sealed class ResearchWorkflowService
{
    public void ValidateEvidence(CloneAssessment a)
    {
        if (a.IedbMatches <= 0)
            throw new InvalidOperationException("Public evidence must be queried before validation.");

        // Conservative research-only validation gate: contextual evidence + candidate-level score.
        a.EvidenceValidated = a.CombinedScore >= 0.75 && a.IedbMatches > 0;
        a.ValidationNotes = a.EvidenceValidated
            ? "Contextual public evidence present and candidate threshold met. Experimental binding confirmation is still required."
            : "Validation gate not met. Additional evidence or experimental confirmation is required.";
        Touch(a);
    }

    public void EvaluateRisk(CloneAssessment a)
    {
        if (!a.EvidenceValidated)
            throw new InvalidOperationException("Evidence validation must pass before the risk gate.");

        // This gate checks whether the record is suitable for further *research review* only.
        // It intentionally does not calculate a treatment, target-selection recipe, dose, or device command.
        var sequencePresent = !string.IsNullOrWhiteSpace(a.Clone.AntibodySequence);
        var annotated = !string.IsNullOrWhiteSpace(a.Clone.Antigen);
        var confidenceOkay = a.Clone.Confidence >= 0.75;

        a.RiskGatePassed = sequencePresent && annotated && confidenceOkay;
        a.RiskNotes = a.RiskGatePassed
            ? "Research-data quality gate passed. No clinical safety or efficacy conclusion is implied."
            : "Research-data quality gate failed: sequence, annotation, or source confidence is insufficient.";
        Touch(a);
    }

    public void RecordHumanApproval(CloneAssessment a, string reviewer)
    {
        if (!a.RiskGatePassed)
            throw new InvalidOperationException("Risk/data-quality gate must pass before human approval.");

        reviewer = string.IsNullOrWhiteSpace(reviewer) ? "unnamed reviewer" : reviewer.Trim();
        a.HumanApproved = true;
        a.ApprovalNotes = $"Research workflow approved by {reviewer}. Approval authorizes simulation/reporting only; not patient treatment.";
        Touch(a);
    }

    public void RunSimulation(CloneAssessment a)
    {
        if (!a.HumanApproved)
            throw new InvalidOperationException("Human approval is required before simulation.");

        // Non-operational sandbox simulation. It does not model molecular editing instructions or treatment parameters.
        var persistence = a.Clone.MemoryCell ? "persistent-memory-associated" : "non-memory-associated";
        var evidence = a.IedbMatches > 5 ? "multiple public-context records" : "limited public-context records";
        a.SimulationCompleted = true;
        a.SimulationSummary = $"Sandbox result: {persistence} candidate with {evidence}. " +
            "Next real-world research step would require independent laboratory validation and institutional oversight. " +
            "No intervention plan, dose, molecular construct, or device command was generated.";
        Touch(a);
    }

    public string BuildReport(CloneAssessment a)
    {
        var report = new
        {
            generatedUtc = DateTimeOffset.UtcNow,
            mode = "research-only / non-operational",
            clone = a.Clone,
            scores = new { a.LocalScore, a.EvidenceBoost, a.CombinedScore, a.Classification },
            publicEvidence = new { a.IedbMatches, a.EvidenceStatus },
            workflow = new
            {
                a.WorkflowStage,
                a.EvidenceValidated,
                a.ValidationNotes,
                a.RiskGatePassed,
                a.RiskNotes,
                a.HumanApproved,
                a.ApprovalNotes,
                a.SimulationCompleted,
                a.SimulationSummary,
                a.LastUpdatedUtc
            },
            restrictions = new[]
            {
                "Not for diagnosis or treatment",
                "Does not generate biological intervention instructions",
                "Does not control laboratory or medical equipment",
                "Public API evidence is contextual and requires independent validation"
            }
        };
        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    private static void Touch(CloneAssessment a) => a.LastUpdatedUtc = DateTimeOffset.UtcNow;
}
