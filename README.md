# 🧬 B-Cell Research Workbench

A cross-platform **B-cell research analysis and simulation workbench** built with **C#, .NET 8, Avalonia UI, and native C++**.

The application provides a desktop environment for exploring B-cell clone datasets, scoring and reviewing candidate records, querying public immunological context, inspecting flow-cytometry metadata, and exercising a safety-gated simulated equipment workflow.

> **Research software only:** This project is intended for software development, research-data exploration, education, and simulation. It is not a clinical diagnostic tool and does not directly control laboratory equipment.

<img width="1277" height="649" alt="Image 2026-09-04 at 12 32 AM" src="https://github.com/user-attachments/assets/8cad2c3a-cbc8-44dc-a33d-6a22cb6b5d5f" />

---

## Overview

B-Cell Research Workbench demonstrates how biological research data, public evidence sources, human review, simulation, and equipment abstractions can be brought together in a single desktop application.

The application includes:

- 🧬 B-cell clone analysis
- 📊 Candidate scoring and classification
- 🌐 IEDB public API exploration
- 🔎 Contextual public-evidence lookup
- ✅ Evidence-validation workflow
- 🛡️ Risk-gating workflow
- 👤 Human reviewer approval
- 🧪 Sandbox simulation
- 🔬 FCS / flow-cytometry metadata inspection
- ⚙️ Simulated equipment digital twin
- 🚨 Equipment interlock simulation
- 📋 Audit logging
- 📤 JSON research-report export
- 🖥️ Modern cross-platform Avalonia desktop GUI

---

# Architecture

The project combines several technologies:

```text
┌───────────────────────────────────────────────┐
│              Avalonia Desktop UI              │
│                  C# / .NET 8                  │
├───────────────────────────────────────────────┤
│                                               │
│   Clone Analysis        Public API Explorer   │
│         │                       │             │
│         ▼                       ▼             │
│   Candidate Scoring          IEDB API         │
│         │                                     │
│         ▼                                     │
│   Evidence Validation                         │
│         │                                     │
│         ▼                                     │
│      Risk Gate                                │
│         │                                     │
│         ▼                                     │
│    Human Approval                             │
│         │                                     │
│         ▼                                     │
│  Sandbox / Digital Twin                       │
│                                               │
├───────────────────────────────────────────────┤
│          Native C++ research component        │
├───────────────────────────────────────────────┤
│     CSV / FCS / JSON / simulated adapters     │
└───────────────────────────────────────────────┘
```

The workflow intentionally separates **analysis**, **public context**, **human review**, and **simulation**.

---

# 🧬 Clone Analysis

Clone Analysis loads B-cell clone records from CSV and presents them through the desktop interface.

A record can contain information such as:

```text
Clone ID
Sequence
Antigen annotation
Memory-cell status
Source confidence
Local score
Public-evidence score
Combined score
Classification
Workflow state
```

The application calculates a local candidate score from the available metadata.

Candidates can then move through a gated research workflow.

```text
Clone
  │
  ▼
Local Analysis
  │
  ▼
Public Evidence
  │
  ▼
Evidence Validation
  │
  ▼
Risk Gate
  │
  ▼
Human Approval
  │
  ▼
Sandbox Simulation
```

---

# 🌐 IEDB Integration

The application contains a **Public API Explorer** for interacting with the Immune Epitope Database (IEDB) query API.

Supported research-oriented exploration includes:

```text
Epitope search
BCR search
Antigen/context searches
```

Clone Analysis can also request contextual IEDB evidence for antigen annotations.

Public database results are treated as **contextual evidence**, not proof that a particular antibody sequence recognizes a specific target.

This distinction is intentional.

---

# 📊 Candidate Scoring

Clone records receive a local score based on their available metadata.

Public evidence can provide an additional bounded evidence contribution.

Conceptually:

```text
Local Score
     +
Public Context
     │
     ▼
Combined Score
     │
     ▼
Candidate Classification
```

The scoring system is intended as a software/research demonstration and should not be interpreted as biological or clinical validation.

---

# ✅ Evidence Validation

Candidates cannot automatically progress through the entire workflow.

Evidence validation checks whether the configured requirements have been met.

For example, the demonstration workflow can require:

```text
Combined score >= threshold
Public contextual evidence > 0
```

A failed gate leaves the candidate unvalidated.

```text
Validated: False
```

A successful validation produces:

```text
Validated: True
```

---

# 🛡️ Risk Gate

Evidence validation alone is insufficient to proceed.

A separate risk gate must also pass.

The application therefore demonstrates a layered workflow:

```text
Evidence Found
      ↓
Evidence Validated
      ↓
Risk Gate Passed
      ↓
Human Reviewed
      ↓
Simulation Allowed
```

This prevents a candidate from moving directly from an automated score into the simulation workflow.

---

# 👤 Human Approval

The application requires an explicit reviewer step before a candidate can proceed to the final sandbox workflow.

The reviewer enters their name and approves the research candidate.

The resulting state can contain:

```text
Validated: True
Risk gate: True
Approved: True
Reviewer: Demo Reviewer
```

This demonstrates a **human-in-the-loop architecture** rather than fully automated decision making.

---

# 🧪 Sandbox Simulation

Approved candidates can enter a software-only sandbox simulation.

The simulation does not represent physical execution of an experimental protocol.

Instead, it demonstrates how a research application could enforce:

```text
analysis
    ↓
validation
    ↓
safety checks
    ↓
human approval
    ↓
simulation
```

before downstream software operations become available.

---

# ⚙️ Equipment Digital Twin

The application contains a simulated equipment controller represented by an internal URI such as:

```text
sim://localhost/cell-controller
```

`sim://` is an application-specific simulator scheme.

It is **not an HTTP server** and therefore cannot be opened directly in a normal web browser.

The simulator supports software states such as:

```text
Disconnected
Connected
Interlocked
```

and simulated jobs can transition through:

```text
Queued
   ↓
Accepted
   ↓
Simulating
   ↓
Completed
```

No real laboratory device is required.

---

# 🚨 Simulated Safety Interlock

The digital twin contains an interlock system.

Normal state:

```text
Connection: CONNECTED
Interlock: HEALTHY
Mode: DRY RUN
```

Opening the simulated interlock changes the controller state and prevents new simulated jobs from being accepted.

This allows developers to test failure and safety states without physical hardware.

---

# 📋 Audit Logging

Equipment simulator operations can be recorded in an audit trail.

Example events include:

```text
connect_requested
connected
job_queued
job_state
interlock
disconnected
```

The audit can be exported as JSON for inspection.

---

# 🔬 FCS / Flow Cytometry Support

The application can inspect `.fcs` flow-cytometry files.

When an FCS file is loaded, the application can expose metadata such as:

```text
FCS version
Event count
Parameter count
Cytometer information
Sample metadata
Parameter names
Stains
Bit widths
Measurement ranges
```

This allows flow-cytometry datasets to be inspected alongside the rest of the research workflow.

---

# 🧫 BD Integration Boundary

The application also contains a software boundary for representing BD flow-cytometry integration.

The current implementation focuses on:

```text
configuration
endpoint validation
FCS data inspection
simulation
```

rather than issuing undocumented commands to physical instruments.

---

# 🧪 Demo Mode

A demonstration dataset is included with the project.

From **Clone Analysis**, click:

```text
Load Demo
```

The application loads the included clone dataset.

A recommended full demonstration is:

```text
Load Demo
    ↓
Select Candidate
    ↓
Inspect Candidate Score
    ↓
Validate Evidence
    ↓
Run Risk Gate
    ↓
Enter Reviewer
    ↓
Approve Research
    ↓
Run Sandbox Simulation
    ↓
Connect Equipment Simulator
    ↓
Submit Dry Run
    ↓
Advance Simulated Job
    ↓
Export Audit
```

Live IEDB searches can also be tested independently through **Query IEDB Evidence** or the **Public API Explorer**.

### Demo evidence vs. live evidence

A deterministic demo configuration may contain clearly labeled simulated evidence so the complete GUI workflow can be tested without relying on an external service.

Live IEDB queries can legitimately return:

```text
No contextual IEDB hit
```

This does not indicate that the application failed—it means the particular public-context query did not return a matching record under the application's search criteria.

Simulated demo evidence should always remain clearly identified as simulated and should never be represented as an actual IEDB result.

---

# 🛠 Technology Stack

### Desktop

- C#
- .NET 8
- Avalonia UI
- XAML / AXAML

### Native component

- C++
- CMake
- Apple Clang on macOS

### Data / Integration

- CSV
- JSON
- FCS
- HTTP/REST
- IEDB public query APIs
- Simulated equipment adapters

---

# 🍎 Building on macOS

The project has been tested with Apple Silicon macOS development environments.

Requirements:

```text
.NET 8 SDK
CMake
Apple Clang / Xcode Command Line Tools
```

Check .NET:

```bash
dotnet --version
```

Check CMake:

```bash
cmake --version
```

Check the compiler:

```bash
clang++ --version
```

---

## Build Release

From the repository root:

```bash
export DOTNET_ROOT=/usr/local/share/dotnet

./scripts/build-macos.sh
```

The build script compiles the native C++ component and the Avalonia application.

A successful build should finish with:

```text
Build succeeded.
```

---

# ▶️ Run

After building:

```bash
./BCellResearchApp/bin/Release/net8.0/BCellResearchApp
```

---

# 🧹 Clean Rebuild

If AXAML, C#, dependencies, or build configuration have changed, a clean rebuild can be useful:

```bash
rm -rf BCellResearchApp/bin
rm -rf BCellResearchApp/obj

export DOTNET_ROOT=/usr/local/share/dotnet

./scripts/build-macos.sh
```

Then run:

```bash
./BCellResearchApp/bin/Release/net8.0/BCellResearchApp
```

---

# 📦 Self-Contained macOS Release

For Apple Silicon:

```bash
export DOTNET_ROOT=/usr/local/share/dotnet

./scripts/publish-macos.sh osx-arm64
```

The published application is placed under:

```text
release/osx-arm64/
```

Run it with:

```bash
./release/osx-arm64/BCellResearchApp
```

---

# 📁 Project Structure

```text
BCellResearchAvalonia/
│
├── BCellResearchApp/
│   ├── App.axaml
│   ├── MainWindow.axaml
│   ├── MainWindow.axaml.cs
│   ├── data/
│   │   └── clones.csv
│   └── ...
│
├── native/
│   ├── src/
│   ├── CMakeLists.txt
│   └── build-macos/
│
├── scripts/
│   ├── build-macos.sh
│   └── publish-macos.sh
│
└── README.md
```

---

# 🔐 Design Philosophy

The project intentionally uses several boundaries between data analysis and downstream actions.

### Public evidence is contextual

A public-database match should not automatically be interpreted as experimental confirmation.

### Human review remains explicit

Automated scoring does not replace the reviewer step.

### Equipment is simulated

The included digital twin is designed for software testing and demonstration rather than direct physical control.

### Demo results are labeled

Synthetic or deterministic demo evidence should always be distinguishable from live public-database results.

---

# ⚠️ Limitations

This project is a research/software prototype.

It does **not**:

- provide medical diagnoses
- establish antibody efficacy
- establish antibody specificity
- replace experimental validation
- provide clinical recommendations
- autonomously execute laboratory protocols
- directly control a physical cell-processing instrument through the included simulator

Results produced by scoring, demonstration data, or simulation should therefore be interpreted as software/research outputs rather than validated biological conclusions.

---

# 🚧 Future Development

Potential extensions include:

- richer clone-data visualization
- score distribution charts
- expanded FCS analysis
- additional public immunology databases
- persistent research projects
- experiment/review history
- improved audit visualization
- configurable scoring models
- browser-accessible digital-twin dashboard
- automated integration tests
- deterministic offline demo mode
- Windows and Linux release packaging
- additional simulated instrument adapters

---

# License

Add the license appropriate for your project before distributing it publicly.

Common choices for open-source software include MIT, Apache-2.0, and GPL-3.0.

---

## Disclaimer

B-Cell Research Workbench is experimental research software.

It is intended for **software development, educational use, research-data exploration, and simulation**. It is not a medical device, diagnostic system, clinical decision-support system, or validated laboratory-control platform.
