using BCellResearchApp.Models;

namespace BCellResearchApp.Services;

public sealed class CandidateAnalyzer
{
    private readonly IedbClient _iedb;
    public CandidateAnalyzer(IedbClient iedb) => _iedb = iedb;

    public CloneAssessment Local(BCellClone clone)
    {
        var score = NativeScorer.Score(clone, out var native);
        return new CloneAssessment
        {
            Clone = clone,
            LocalScore = score,
            Classification = Classify(score),
            Rationale = $"{(native ? "C++ native" : "managed fallback")} transparent metadata score. Public evidence not yet queried."
        };
    }

    public async Task EnrichAsync(CloneAssessment a, CancellationToken ct = default)
    {
        try
        {
            // Antibody variable-region sequences are not epitope sequences. We therefore do NOT submit the
            // antibody sequence as if it were an antigen epitope. Instead, use antigen annotation as contextual evidence.
            var evidence = await _iedb.SearchAntigensByTextAsync(a.Clone.Antigen, 10, ct);
            a.IedbMatches = evidence.Count;
            a.EvidenceBoost = evidence.Count > 0 ? Math.Min(0.08, 0.02 + 0.01 * evidence.Count) : 0;
            a.Classification = Classify(a.CombinedScore);
            a.EvidenceStatus = evidence.Count > 0 ? $"IEDB contextual hits: {evidence.Count}" : "No contextual IEDB hit in sampled page";
            a.Rationale = $"Local score plus a small, capped IEDB context boost. API hits are supporting metadata only, not proof of clone specificity.";
        }
        catch (Exception ex)
        {
            a.EvidenceStatus = "IEDB query failed: " + ex.Message;
        }
    }

    private static string Classify(double score) => score >= 0.75 ? "candidate" : score >= 0.45 ? "review" : "not_candidate";
}
