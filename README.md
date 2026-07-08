# ste_tool_studio (WPF .NET 8)

Desktop WPF application that standardizes two internal QA workflows for STD artifacts exported from DOORS:

1. **STD Baseline Verifier**
2. **STD Template Normalizer**

The app is a **workflow orchestrator** (UI + validation + logging + configuration). Core domain processing is executed by packaged backend executables built from Python automation.

---

## Why this tool exists

Before this tool, engineers had to manually run validation/normalization steps and manage multiple scripts and inputs. `ste_tool_studio` was built to:

- reduce user error in repetitive QA operations,
- centralize workflow steps in one Windows UI,
- preserve configuration defaults per user,
- provide consistent execution logging/report access,
- lower onboarding time for new users by exposing guided actions.

---

## High-level concept

- **Frontend:** WPF desktop app (.NET 8).
- **Application layer:** MVVM ViewModels + services for process execution, reporting, and logging.
- **Execution layer:** Python-packaged executables (`.exe`) invoked by C#.
- **Persistence:** User-scoped config + logs under `%APPDATA%\ste_tool_studio`.

This means feature logic is split between:
- **C# app responsibilities:** user input capture/validation, state/progress handling, config persistence, invoking executables.
- **Python executable responsibilities:** domain-specific validation/normalization.

---

## Functional overview

## 1) STD Baseline Verifier

### Purpose
Validate exported STD Excel files for:
- bug/VSTS consistency checks,
- STD rule violations.

### User inputs
- Excel file (`.xls` / `.xlsx`)
- STD Name
- Iteration Path
- Current V&V Version

### Actions
- **Check STD Bugs in VSTS** → runs `test_bugs_std_validation.exe`
- **Validate STD Rules** → runs `test_excel_violations.exe`
- **Last Report** actions open generated reports.

### Notes
- App attempts to auto-fill `STD Name` from selected filename (unless user already entered one).
- File should be a DOORS-exported Excel and must be closed before execution.

---

## 2) STD Template Normalizer

### Purpose
Normalize an exported STD Excel file (`.xls` / `.xlsx`) to required output template conventions.

### User inputs
- Source Excel file (`.xls` / `.xlsx`)
- STD Name
- Document mode (**Protocol** or **Report**)
- Doc Number (required)
- Test Plan (required)
- STx Number (required; enforced prefix)
- Prepared By (required)
- Report Number (required only in Report mode)

### Optional behavior
- **Cycle dropdown** (`Default`, then configured cycles like `1`, `2`, ...).
- Selecting a real cycle can auto-fill fields from config.
- `Default` intentionally keeps manual entry behavior.

### Execution
- Runs `test_document_normalization.exe`
- Passes selected mode (`Protocol`/`Report`) and user fields.

### Important UX rule
- STx prefix is mode-driven and normalized by UI:
  - Protocol mode → `STD...`
  - Report mode → `STR...`

---

## Input source constraints (DOORS export)

For both workflows, source files should be produced from DOORS export with:
- **Object Heading and Text** selected,
- **Preserve rich text formatting** enabled,
- Excel file saved and **closed** before running this tool.

---

## Architecture and key code areas

- `MainMenuWindow.xaml` — entry/launcher window.
- `BaselineVerifierWindow.xaml` — Baseline Verifier UI.
- `STDTemplateNormalizer.xaml` — Template Normalizer UI.
- `src/ViewModels/` — state + command logic (MVVM).
- `src/Services/ValidationService.cs` — executable invocation wrappers.
- `src/Services/ProcessExecutionService.cs` — process execution plumbing.
- `src/Services/ReportService.cs` — report open/find behavior.
- `src/Configuration/AppConfiguration.cs` — config lifecycle + APPDATA management.
- `Scripts/` — packaged backend executables/resources.

---

## Backend executables (required runtime dependencies)

The following files must be present at runtime:

- `Scripts/test_bugs_std_validation.exe`
- `Scripts/test_excel_violations.exe`
- `Scripts/test_document_normalization.exe`

If any are missing, related features fail. `Scripts/Template.docx` is also required at build/publish time and is copied to `%APPDATA%\ste_tool_studio\Template.docx` on startup.

---

## Configuration model (`config.json`)

The application reads/writes a user-specific config at:

`%APPDATA%\ste_tool_studio\config.json`

On first run, if absent, it is copied from the default config near the app binaries.

### Typical keys
- Baseline verifier:
  - `url`, `excel_path`, `std_name`, `current_version`, `iteration_path`
- Template normalizer:
  - `doc_type`, `protocol_number`, `report_number`, `test_plan`, `stx_number`, `prepared_by`, `Exported_STD`

### Cycle defaults
Add top-level keys like `cycle_1`, `cycle_2`, ...:

```json
"cycle_1": {
  "protocol_number": "DOC-001",
  "test_plan": "TP-001"
}
```

Cycle IDs are discovered from keys prefixed with `cycle_`.

---

## Reports and logging

- Logs: `%APPDATA%\ste_tool_studio\ste_tool_studio.log`
- Reports: opened/generated through report services and backend tools.
- Shared logging path allows both C# app and Python automation to contribute to one log file.

Logging extension guidelines:
- Use DI `ILoggingService` (`LogInfo`, `LogWarning`, `LogError`, `LogDebug`).
- Avoid direct `Console.WriteLine` / ad-hoc file writes.
- Log key workflow boundaries (inputs, process start/end, non-zero exits, exceptions).
- Keep logs concise and avoid sensitive data.

---

## Build and run

### Prerequisites
- .NET SDK 8.0+
- Windows (WPF target)

### Build/publish
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

---

## FRS-oriented summary (quick reference)

- **Product goal:** provide controlled UI orchestration for STD verification and normalization workflows.
- **Primary actors:** QA/Test engineers processing DOORS-exported STD files.
- **Core capabilities:** input collection, validation, mode/cycle behavior, backend process execution, status/progress, report/log accessibility, config persistence.
- **Key dependency risk:** missing backend `.exe` files or invalid input export format.

