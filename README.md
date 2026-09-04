# B-Cell Memory Research Workbench

Cross-platform Avalonia (.NET 8/C#) desktop research prototype for macOS and Windows, with an optional C++17 native scoring library and read-only public IEDB API integration.

## Safety / scope
This app is **not a medical device**. It does not diagnose infection or immunity, prescribe treatment, identify cells for destruction, erase immune memory, control laboratory equipment, or trigger biological intervention. Its candidate labels are research triage labels based on transparent toy metadata scoring plus limited public-data context.

## What is new vs. the original C++ demo
- Avalonia desktop UI for macOS + Windows
- C++ scorer exposed through a stable C ABI and called by C# using P/Invoke
- Managed fallback if the native library is absent
- CSV import and clone review
- IEDB IQ-API read-only integration
  - `epitope_search` substring lookup for antigen/epitope sequences
  - `bcr_search` receptor-record explorer
  - `antigen_search` contextual evidence lookup
- Candidate score keeps public evidence as a small capped context boost, not a confirmation

## Prerequisites
- .NET 8 SDK
- CMake 3.16+
- C++17 compiler
- macOS: Xcode Command Line Tools
- Windows: Visual Studio Build Tools with Desktop development with C++

## macOS
```bash
./scripts/build-macos.sh
cd BCellResearchApp
dotnet run
```

## Windows PowerShell
```powershell
.\scripts\build-windows.ps1
cd BCellResearchApp
dotnet run
```

You can also run without building C++; the C# implementation automatically falls back to the same transparent local scoring rule.

## Public API notes
IEDB IQ-API is PostgREST-based and provides epitope, antigen, B-cell assay, BCR and other endpoints. The app makes read-only HTTPS requests and does not upload names, medical records, or other patient identifiers. Do not put patient-identifying data in the demo CSV.

The epitope explorer uses the documented IEDB substring pattern against `epitope_search.linear_sequence`. An antibody variable-region sequence is **not** treated as an antigen epitope; the app does not make that biological category error.

## Suggested next research-grade upgrades
For legitimate repertoire research, use AIRR-formatted data, V(D)J annotation, clonotype/lineage clustering, validated SARS-CoV-2 antibody reference datasets, experimental antigen-binding results, provenance and audit logs, and independent validation. Keep any intervention system physically and logically separated from exploratory software.


## Extended research workflow

The desktop app now continues beyond candidate review:

`identify → compare → score → public evidence → validate → research risk gate → human approval → sandbox simulation → audit/report export`

The later stages are intentionally non-operational. They do not generate treatment parameters, molecular constructs, cell-destruction instructions, doses, or commands for laboratory/medical equipment.

## Equipment Digital Twin integration

The Avalonia app now includes an `IEquipmentAdapter` abstraction plus a `SimulatedEquipmentAdapter` that behaves like a connected laboratory/medical controller without generating or transmitting operational biological commands.

The simulated integration supports:

- connect / disconnect lifecycle
- equipment identity and capability negotiation
- safety-interlock state
- candidate submission after evidence/risk/reviewer gates
- dry-run job queue
- acknowledgement and state transitions
- telemetry and queue depth
- immutable-style audit event export to JSON

A submitted job is intentionally restricted to a metadata envelope such as candidate ID, research classification, score and antigen annotation. It does not contain biological targeting instructions, editing constructs, dosages, treatment parameters, device actuation parameters or vendor-specific control commands.

This makes the application suitable for developing the **software integration boundary** against a digital twin or test harness. A real regulated-device integration would require the equipment vendor's documented sandbox/test API, quality-system controls, cybersecurity review, validation, institutional oversight and applicable regulatory authorization.

## BD flow-cytometry integration

The Avalonia app now includes a **BD Flow Cytometry** tab.

What works now:
- Imports and parses standard FCS 3.x files.
- Reads FCS version, TEXT/DATA offsets, `$PAR`, `$TOT`, `$CYT`, sample/date metadata, and per-parameter names/stains/bit widths/ranges.
- Works with FCS files produced by BD instruments as well as other FCS-compliant flow cytometers.
- Exposes a BD/Biosero vendor-adapter boundary for future authenticated integration.

What is intentionally not fabricated:
- No undocumented FACSDiscover S8 REST endpoint.
- No made-up FACSDiva/FACSChorus commands.
- No commands that start acquisition, define sorting gates, or actuate a sorter.

BD publicly announced collaboration with Biosero to support Green Button Go automation for selected BD flow cytometers. If your lab has the official BD/Biosero SDK/API documentation and credentials, the `BdVendorAutomationAdapter` is the place to implement that contract.

The data-import path uses the Flow Cytometry Standard (FCS), maintained by ISAC, which is designed for interoperable exchange of cytometry datasets.
