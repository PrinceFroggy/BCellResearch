using BCellResearchApp.Models;

namespace BCellResearchApp.Services;

// Intentionally non-operational until an official BD/Biosero interface contract is supplied.
// This prevents the application from inventing undocumented commands for a real sorter/analyzer.
public sealed class BdVendorAutomationAdapter
{
    public BdIntegrationProfile Profile { get; } = new();

    public string Endpoint { get; private set; } = "";
    public bool Configured => !string.IsNullOrWhiteSpace(Endpoint);

    public void ConfigureVendorEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
            throw new ArgumentException("Enter an endpoint from official BD/Biosero documentation.");
        Endpoint = endpoint;
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "BD FACSDiscover S8 does not have a verified public control API in this project. " +
            "Supply the official BD/Biosero SDK/API contract before implementing vendor connectivity.");
    }
}
