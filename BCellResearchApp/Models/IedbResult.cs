using System.Text.Json;

namespace BCellResearchApp.Models;

public sealed class IedbResult
{
    public int Count { get; init; }
    public string Endpoint { get; init; } = "";
    public string Query { get; init; } = "";
    public JsonElement Data { get; init; }
}
