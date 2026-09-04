using BCellResearchApp.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BCellResearchApp.Services;

public sealed class IedbClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri("https://query-api.iedb.org/") };

    public IedbClient()
    {
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("BCellResearchAvalonia/1.0 research-prototype");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // IEDB documents exact and substring matching against epitope_search.linear_sequence.
    public async Task<IedbResult> FindEpitopeSequenceAsync(string sequence, int limit = 25, CancellationToken ct = default)
    {
        var clean = new string(sequence.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        if (clean.Length < 5) throw new ArgumentException("Enter at least 5 amino-acid letters.");
        var q = $"epitope_search?linear_sequence=like.*{Uri.EscapeDataString(clean)}*&limit={Math.Clamp(limit, 1, 100)}";
        return await GetAsync(q, ct);
    }

    // Generic read-only receptor search. The returned JSON is intentionally retained verbatim because
    // IEDB can evolve individual BCR fields while keeping the PostgREST endpoint stable.
    public async Task<IedbResult> GetRecentBcrRecordsAsync(int limit = 20, CancellationToken ct = default)
    {
        var q = $"bcr_search?limit={Math.Clamp(limit, 1, 100)}";
        return await GetAsync(q, ct);
    }

    public async Task<IedbResult> SearchAntigensByTextAsync(string text, int limit = 25, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Antigen text is required.");
        // Query a limited page then inspect text locally. This avoids depending on an undocumented field name.
        var q = $"antigen_search?limit={Math.Clamp(limit * 4, 10, 100)}";
        var raw = await GetAsync(q, ct);
        var needle = text.Trim();
        var matches = new List<JsonElement>();
        if (raw.Data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in raw.Data.EnumerateArray())
            {
                if (item.GetRawText().Contains(needle, StringComparison.OrdinalIgnoreCase))
                    matches.Add(item.Clone());
                if (matches.Count >= limit) break;
            }
        }
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(matches));
        return new IedbResult { Count = matches.Count, Endpoint = "antigen_search", Query = needle, Data = doc.RootElement.Clone() };
    }

    private async Task<IedbResult> GetAsync(string relative, CancellationToken ct)
    {
        using var response = await _http.GetAsync(relative, ct);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement.Clone();
        var count = root.ValueKind == JsonValueKind.Array ? root.GetArrayLength() : 1;
        return new IedbResult { Count = count, Endpoint = relative.Split('?')[0], Query = relative, Data = root };
    }
}
