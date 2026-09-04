using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using BCellResearchApp.Models;
using BCellResearchApp.Services;
using System.Text.Json;

namespace BCellResearchApp;

public partial class MainWindow : Window
{
    private readonly IedbClient _iedb = new();
    private readonly CandidateAnalyzer _analyzer;
    private readonly ResearchWorkflowService _workflow = new();
    private readonly IEquipmentAdapter _equipment = new SimulatedEquipmentAdapter();
    private readonly BdFcsService _bdFcs = new();
    private readonly BdVendorAutomationAdapter _bdVendor = new();
    private readonly List<CloneAssessment> _items = new();

    public MainWindow()
    {
        InitializeComponent();
        _analyzer = new CandidateAnalyzer(_iedb);
        Opened += (_, _) => { RefreshEquipment(); RefreshBdProfile(); };
    }

    private void Load(string path)
    {
        _items.Clear();
        foreach (var clone in CsvService.Load(path)) _items.Add(_analyzer.Local(clone));
        RefreshList();
        StatusText.Text = $"Loaded {_items.Count} clone records.";
    }

    private void RefreshList()
    {
        CloneList.ItemsSource = null;
        CloneList.ItemsSource = _items.Select(a => $"{a.Clone.CloneId,-12}  {a.Classification,-14}  {a.CombinedScore:0.000}  {a.Clone.Antigen}").ToList();
        if (_items.Count > 0) CloneList.SelectedIndex = 0;
    }

    private void ShowSelected()
    {
        var i = CloneList.SelectedIndex;
        if (i < 0 || i >= _items.Count) return;
        var a = _items[i];
        DetailClone.Text = $"Clone: {a.Clone.CloneId}    Memory cell: {a.Clone.MemoryCell}";
        DetailAntigen.Text = $"Antigen annotation: {a.Clone.Antigen}";
        DetailScore.Text = $"Local: {a.LocalScore:0.000}   Public evidence boost: {a.EvidenceBoost:0.000}   Combined: {a.CombinedScore:0.000}";
        DetailClass.Text = $"Classification: {a.Classification}";
        DetailEvidence.Text = $"Evidence: {a.EvidenceStatus}";
        DetailRationale.Text = a.Rationale;
        WorkflowStageText.Text = $"Stage: {a.WorkflowStage}   |   Validated: {a.EvidenceValidated}   Risk gate: {a.RiskGatePassed}   Approved: {a.HumanApproved}   Simulated: {a.SimulationCompleted}";
        WorkflowNotesText.Text =
            $"Validation: {a.ValidationNotes}" +
            $"Risk: {a.RiskNotes}" +
            $"Approval: {a.ApprovalNotes}" +
            $"Simulation: {a.SimulationSummary}";
        DetailSequence.Text = a.Clone.AntibodySequence;
    }

    private void LoadDemo_Click(object? sender, RoutedEventArgs e)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "clones.csv");
        Load(path);

        // Deterministic demo evidence so the full workflow can be tested
        // without depending on live IEDB API results.
        if (_items.Count > 0)
        {
            var demo = _items[0];

            demo.IedbMatches = 3;
            demo.EvidenceBoost = 0.05;
            demo.EvidenceStatus = "DEMO evidence: simulated contextual hits: 3";
            demo.Classification = demo.CombinedScore >= 0.75
                ? "candidate"
                : demo.CombinedScore >= 0.45
                    ? "review"
                    : "not_candidate";

            demo.Rationale =
                "DEMO MODE: simulated public-context evidence added for UI/workflow testing only. " +
                "This is not a real IEDB result or evidence of biological specificity.";

            RefreshList();
            CloneList.SelectedIndex = 0;
            ShowSelected();

            StatusText.Text =
                "Demo loaded. clone_001 has simulated evidence for full workflow testing.";
        }
    }

    private async void OpenCsv_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open B-cell clone CSV",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
        });
        var f = files.FirstOrDefault();
        if (f?.TryGetLocalPath() is string path) Load(path);
    }

    private async void Enrich_Click(object? sender, RoutedEventArgs e)
    {
        if (_items.Count == 0) return;
        EnrichButton.IsEnabled = false;
        try
        {
            for (var i = 0; i < _items.Count; i++)
            {
                StatusText.Text = $"IEDB context {i + 1}/{_items.Count}…";
                await _analyzer.EnrichAsync(_items[i]);
            }
            RefreshList();
            StatusText.Text = "IEDB evidence lookup finished.";
        }
        finally { EnrichButton.IsEnabled = true; }
    }

    private void CloneList_SelectionChanged(object? sender, SelectionChangedEventArgs e) => ShowSelected();

    private CloneAssessment? SelectedAssessment()
    {
        var i = CloneList.SelectedIndex;
        return i >= 0 && i < _items.Count ? _items[i] : null;
    }

    private void ValidateEvidence_Click(object? sender, RoutedEventArgs e)
    {
        try { var a = SelectedAssessment(); if (a is null) return; _workflow.ValidateEvidence(a); ShowSelected(); RefreshList(); StatusText.Text = "Evidence validation recorded."; }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void RiskGate_Click(object? sender, RoutedEventArgs e)
    {
        try { var a = SelectedAssessment(); if (a is null) return; _workflow.EvaluateRisk(a); ShowSelected(); RefreshList(); StatusText.Text = "Research data-quality/risk gate evaluated."; }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void ApproveResearch_Click(object? sender, RoutedEventArgs e)
    {
        try { var a = SelectedAssessment(); if (a is null) return; _workflow.RecordHumanApproval(a, ReviewerName.Text ?? ""); ShowSelected(); RefreshList(); StatusText.Text = "Research-only approval recorded."; }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void RunSimulation_Click(object? sender, RoutedEventArgs e)
    {
        try { var a = SelectedAssessment(); if (a is null) return; _workflow.RunSimulation(a); ShowSelected(); RefreshList(); StatusText.Text = "Sandbox simulation completed."; }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private async void ExportReport_Click(object? sender, RoutedEventArgs e)
    {
        var a = SelectedAssessment();
        if (a is null) return;
        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export research candidate report",
                SuggestedFileName = $"{a.Clone.CloneId}-research-report.json",
                FileTypeChoices = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } }
            });
            if (file?.TryGetLocalPath() is string path)
            {
                await File.WriteAllTextAsync(path, _workflow.BuildReport(a));
                StatusText.Text = $"Report exported: {path}";
            }
        }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void RefreshEquipment()
    {
        var t = _equipment.GetTelemetry();
        EquipmentStateText.Text = $"State: {t.State}   Interlock: {(t.InterlockHealthy ? "HEALTHY" : "OPEN")}   Queue: {t.QueueDepth}";
        EquipmentProfileText.Text = $"{_equipment.Profile.DisplayName} | {_equipment.Profile.Vendor} {_equipment.Profile.Model} | {_equipment.Profile.Endpoint}";
        EquipmentTelemetryText.Text = $"Active job: {t.ActiveJobId}\nStatus: {t.StatusMessage}\nCapabilities: {string.Join(", ", _equipment.Profile.Capabilities)}";
        EquipmentJobs.ItemsSource = null;
        EquipmentJobs.ItemsSource = _equipment.GetJobs().Select(j => $"{j.JobId}  {j.Status,-18}  candidate={j.CandidateId}").ToList();
    }

    private async void EquipmentConnect_Click(object? sender, RoutedEventArgs e)
    {
        try { await _equipment.ConnectAsync(); EquipmentStatus.Text = "Connected to simulated equipment adapter."; RefreshEquipment(); }
        catch (Exception ex) { EquipmentStatus.Text = ex.Message; }
    }

    private async void EquipmentDisconnect_Click(object? sender, RoutedEventArgs e)
    {
        await _equipment.DisconnectAsync();
        EquipmentStatus.Text = "Disconnected.";
        RefreshEquipment();
    }

    private void EquipmentInterlock_Click(object? sender, RoutedEventArgs e)
    {
        var healthy = !_equipment.GetTelemetry().InterlockHealthy;
        _equipment.SetInterlock(healthy);
        EquipmentStatus.Text = healthy ? "Interlock restored." : "Interlock opened; submissions are blocked.";
        RefreshEquipment();
    }

    private async void EquipmentSubmit_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var a = SelectedAssessment();
            if (a is null) { EquipmentStatus.Text = "Select a candidate in Clone Analysis first."; return; }
            var job = await _equipment.SubmitDryRunAsync(a);
            EquipmentStatus.Text = $"Dry-run envelope queued: {job.JobId}. No executable treatment/device parameters were transmitted.";
            RefreshEquipment();
        }
        catch (Exception ex) { EquipmentStatus.Text = ex.Message; }
    }

    private async void EquipmentAdvance_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var i = EquipmentJobs.SelectedIndex;
            var jobs = _equipment.GetJobs();
            if (i < 0 || i >= jobs.Count) { EquipmentStatus.Text = "Select a dry-run job."; return; }
            var job = await _equipment.AdvanceDryRunAsync(jobs[i].JobId);
            EquipmentStatus.Text = job is null ? "Job not found." : $"{job.JobId} => {job.Status}";
            RefreshEquipment();
        }
        catch (Exception ex) { EquipmentStatus.Text = ex.Message; }
    }

    private async void EquipmentExport_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export equipment simulation audit",
                SuggestedFileName = "equipment-simulation-audit.json",
                FileTypeChoices = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } }
            });
            if (file?.TryGetLocalPath() is string path)
            {
                await File.WriteAllTextAsync(path, _equipment.ExportAuditJson());
                EquipmentStatus.Text = $"Audit exported: {path}";
            }
        }
        catch (Exception ex) { EquipmentStatus.Text = ex.Message; }
    }

    private async void IedbSequence_Click(object? sender, RoutedEventArgs e)
    {
        ApiStatus.Text = "Querying IEDB epitope_search…";
        try
        {
            var r = await _iedb.FindEpitopeSequenceAsync(ApiSequence.Text ?? "");
            ApiStatus.Text = $"IEDB returned {r.Count} record(s).";
            ApiOutput.Text = JsonSerializer.Serialize(r.Data, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) { ApiStatus.Text = ex.Message; }
    }

    private async void IedbBcr_Click(object? sender, RoutedEventArgs e)
    {
        ApiStatus.Text = "Querying IEDB bcr_search…";
        try
        {
            var r = await _iedb.GetRecentBcrRecordsAsync();
            ApiStatus.Text = $"IEDB returned {r.Count} receptor record(s).";
            ApiOutput.Text = JsonSerializer.Serialize(r.Data, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) { ApiStatus.Text = ex.Message; }
    }

    private void RefreshBdProfile()
    {
        var p = _bdFcs.Profile;
        BdProfileText.Text = $"{p.Vendor} | {p.Family} | Mode: {p.IntegrationMode} | Public control API verified: {p.PublicControlApiVerified}";
    }

    private async void BdOpenFcs_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open flow-cytometry FCS file",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("Flow Cytometry Standard") { Patterns = new[] { "*.fcs", "*.FCS" } } }
            });
            if (files.Count == 0 || files[0].TryGetLocalPath() is not string path) return;
            var summary = _bdFcs.ReadSummary(path);
            BdOutput.Text = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) { BdOutput.Text = ex.ToString(); }
    }

    private void BdStatus_Click(object? sender, RoutedEventArgs e)
    {
        BdOutput.Text = JsonSerializer.Serialize(_bdFcs.Profile, new JsonSerializerOptions { WriteIndented = true });
    }

    private void BdValidateEndpoint_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _bdVendor.ConfigureVendorEndpoint(BdVendorEndpoint.Text ?? "");
            BdOutput.Text = JsonSerializer.Serialize(new
            {
                endpoint = _bdVendor.Endpoint,
                configured = _bdVendor.Configured,
                status = "Endpoint syntax accepted. No commands are sent because an official BD/Biosero API contract has not been supplied."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) { BdOutput.Text = ex.Message; }
    }

}
