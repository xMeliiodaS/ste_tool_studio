# ste_tool_studio (WPF .NET 8)

Desktop WPF application for two QA workflows:

1. **STD Baseline Verifier**
2. **STD Template Normalizer**

The UI is written in C#/.NET and executes packaged backend tools (`.exe`) that are produced from Python automation.

---

## What this app does

## 1) STD Baseline Verifier
- Lets users choose an Excel file (`.xls` / `.xlsx`).
- Runs two backend executables:
  - `test_bugs_std_validation.exe`
  - `test_excel_violations.exe`
- Displays status/progress and opens generated reports.

## 2) STD Template Normalizer
- Lets users choose a DOCX file.
- Sends form inputs (STD name, doc/project/test plan/prepared by/footer, mode) to:
  - `test_document_normalization.exe`
- Supports **Cycle** dropdown values from config (`cycle_1`, `cycle_2`, ...).
- Current UX behavior:
  - Default dropdown value is `Default`.
  - No cycle autofill is applied until the user selects a real cycle.

---

## Tech stack
- .NET 8 (`net8.0-windows`)
- WPF (XAML + MVVM-style ViewModels)
- Newtonsoft.Json
- MaterialDesignThemes / MaterialDesignColors

---

## Repo structure (important files)

- `MainMenuWindow.xaml` — main tool launcher window.
- `BaselineVerifierWindow.xaml` — baseline verifier UI.
- `STDTemplateNormalizer.xaml` — template normalizer UI.
- `src/ViewModels/` — ViewModels for each tool.
- `src/Configuration/AppConfiguration.cs` — config load/save + APPDATA path management.
- `src/Services/ValidationService.cs` — executes backend tools.
- `Scripts/` — packaged backend executables and script-side resources.

---

## Python-backed executable files (important)

These files are expected by the app and are included/copy-managed by the project file:

- `Scripts/test_bugs_std_validation.exe`
- `Scripts/test_excel_violations.exe`
- `Scripts/test_document_normalization.exe`

If any of these are missing from the output, the related feature will fail.

---

## Configuration (`config.json`)

The app uses a **user-specific config** in:

`%APPDATA%\ste_tool_studio\config.json`

At first run, if this file does not exist, it is copied from the default `config.json` next to the app executable.

### Common keys
- Baseline verifier:
  - `url`, `excel_path`, `std_name`, `current_version`, `iteration_path`
- Template normalizer:
  - `doc_type`, `doc_number`, `project_number`, `test_plan`, `prepared_by`, `footer`, `Exported_STD`

### Cycle autofill keys
Add top-level objects like:

```json
"cycle_1": {
  "doc_number": "DOC-001",
  "project_number": "PRJ-001",
  "test_plan": "TP-001",
  "footer": "Footer 1"
}
```

You can add `cycle_2`, `cycle_3`, etc.

---

## Build and run

### Prerequisites
- .NET SDK 8.0+
- Windows environment (WPF)

### Build
```bash
dotnet build ste_tool_studio.sln
```

### Run
```bash
dotnet run --project ste_tool_studio.csproj
```

---

## Notes
- Logs are written under `%APPDATA%\ste_tool_studio`.
- Reports are generated/opened through the app services.
- If dropdown cycles appear empty, verify you edited the APPDATA config (not only repo copies), then restart the app.
