namespace BCellResearchApp.Models;

public sealed class BdFlowFileSummary
{
    public string FileName { get; set; } = "";
    public string FcsVersion { get; set; } = "";
    public long TextStart { get; set; }
    public long TextEnd { get; set; }
    public long DataStart { get; set; }
    public long DataEnd { get; set; }
    public int ParameterCount { get; set; }
    public long EventCount { get; set; }
    public string Cytometer { get; set; } = "";
    public string SampleId { get; set; } = "";
    public string Date { get; set; } = "";
    public Dictionary<string, string> Keywords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<BdFlowParameter> Parameters { get; set; } = new();
}

public sealed class BdFlowParameter
{
    public int Index { get; set; }
    public string Name { get; set; } = "";
    public string Stain { get; set; } = "";
    public int Bits { get; set; }
    public double Range { get; set; }
}

public sealed class BdIntegrationProfile
{
    public string Vendor { get; set; } = "BD Biosciences";
    public string Family { get; set; } = "Flow Cytometry / Cell Sorting";
    public string IntegrationMode { get; set; } = "FCS 3.x file ingestion";
    public bool PublicControlApiVerified { get; set; }
    public string ControlApiStatus { get; set; } = "No public FACSDiscover S8 control API verified.";
    public string AutomationNote { get; set; } = "BD has announced Biosero Green Button Go integration for selected BD flow cytometers; vendor access is required.";
}
