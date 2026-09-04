namespace BCellResearchApp.Models;

public sealed class BCellClone
{
    public string CloneId { get; set; } = "";
    public string AntibodySequence { get; set; } = "";
    public string Antigen { get; set; } = "";
    public bool MemoryCell { get; set; }
    public double Confidence { get; set; }
}
