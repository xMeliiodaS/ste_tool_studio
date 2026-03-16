using Newtonsoft.Json.Linq;
using ste_tool_studio.Constants;
using System.IO;
using System.Linq;
using IOPath = System.IO.Path;

namespace ste_tool_studio.Configuration
{
    /// <summary>
    /// Manages application configuration with user-specific settings
    /// </summary>
    public class AppConfiguration
    {
        private readonly string _userConfigPath;
        private readonly string _defaultConfigPath;
        private JObject _config;

        public AppConfiguration()
        {
            string appDataFolder = IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppConstants.AppName);

            Directory.CreateDirectory(appDataFolder);

            _userConfigPath = IOPath.Combine(appDataFolder, AppConstants.ConfigFileName);
            _defaultConfigPath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);

            EnsureUserConfigExists();
            LoadConfiguration();
        }

        /// <summary>
        /// Ensures the user-specific configuration file exists by copying from default if missing
        /// </summary>
        private void EnsureUserConfigExists()
        {
            try
            {
                if (!File.Exists(_userConfigPath))
                {
                    if (File.Exists(_defaultConfigPath))
                    {
                        File.Copy(_defaultConfigPath, _userConfigPath);
                    }
                    else
                    {
                        throw new FileNotFoundException(
                            string.Format(AppConstants.ErrorConfigNotFound, AppConstants.ConfigFileName, _defaultConfigPath));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format(AppConstants.ErrorConfigCreationFailed, ex.Message), ex);
            }
        }

        /// <summary>
        /// Loads configuration from the user config file
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                string jsonContent = File.ReadAllText(_userConfigPath);
                _config = JObject.Parse(jsonContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves the current configuration to disk
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                File.WriteAllText(_userConfigPath, _config.ToString());
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        // Configuration properties with getters and setters
        public string Url
        {
            get => _config[AppConstants.ConfigKeyUrl]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyUrl] = value;
        }

        public string ExcelPath
        {
            get => _config[AppConstants.ConfigKeyExcelPath]?.ToString() ?? string.Empty;
            set
            {
                _config[AppConstants.ConfigKeyExcelPath] = value;
                SaveConfiguration();
            }
        }

        public string DocType
        {
            get => _config[AppConstants.ConfigKeyDoctype]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyDoctype] = value;
        }

        public string StdName
        {
            get => _config[AppConstants.ConfigKeyStdName]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyStdName] = value;
        }

        public string CurrentVersion
        {
            get => _config[AppConstants.ConfigKeyCurrentVersion]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyCurrentVersion] = value;
        }

        public string IterationPath
        {
            get => _config[AppConstants.ConfigKeyIterationPath]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyIterationPath] = value;
        }

        // Template Normalizer (Protocol number → config key "protocol_number" → Word ADD_DOC_STD#)
        public string DocNumber
        {
            get => _config[AppConstants.ConfigKeyProtocolNumber]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyProtocolNumber] = value;
        }

        public string ReportNumber
        {
            get => _config[AppConstants.ConfigKeyReportNumber]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyReportNumber] = value;
        }

        public string ProjectNumber
        {
            get => _config[AppConstants.ConfigKeyProjectNumber]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyProjectNumber] = value;
        }

        public string TestPlan
        {
            get => _config[AppConstants.ConfigKeyTestPlan]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyTestPlan] = value;
        }

        public string PreparedBy
        {
            get => _config[AppConstants.ConfigKeyPreparedBy]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyPreparedBy] = value;
        }

        public string Footer
        {
            get => _config[AppConstants.ConfigKeyFooter]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyFooter] = value;
        }

        public string StxNumber
        {
            get => _config[AppConstants.ConfigKeyStxNumber]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ConfigKeyStxNumber] = value;
        }

        public string SelectedFilePath
        {
            get => _config[AppConstants.ExportedSTD]?.ToString() ?? string.Empty;
            set => _config[AppConstants.ExportedSTD] = value;
        }

        /// <summary>
        /// Updates multiple configuration values at once
        /// </summary>
        public void UpdateBaselineVerifierConfig(string stdName, string iterationPath, string currentVersion)
        {
            StdName = stdName;
            IterationPath = iterationPath;
            CurrentVersion = currentVersion;

            SaveConfiguration();
        }

        /// <summary>
        /// Updates multiple configuration values at once for the STD Template Normalizer tool.
        /// </summary>
        public void UpdateTemplateNormalizerConfig(
            string docType,
            string stdName,
            string docNumber,
            string reportNumber,
            string testPlan,
            string stxNumber,
            string preparedBy,
            string selectedFilePath)
        {
            DocType = docType;
            StdName = stdName;
            DocNumber = docNumber;
            ReportNumber = reportNumber;
            TestPlan = testPlan;
            StxNumber = stxNumber;
            PreparedBy = preparedBy;
            SelectedFilePath = selectedFilePath;

            SaveConfiguration();
        }

        /// <summary>
        /// Gets the path to a report file
        /// </summary>
        public string GetReportPath(string reportFileName)
        {
            string appDataFolder = IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppConstants.AppName);
            return IOPath.Combine(appDataFolder, reportFileName);
        }

        /// <summary>
        /// Gets the path to the log file (APPDATA, shared with Python so both write to one file).
        /// </summary>
        public string GetLogFilePath()
        {
            string appDataFolder = IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppConstants.AppName);
            return IOPath.Combine(appDataFolder, AppConstants.LogFileName);
        }

        /// <summary>
        /// Gets all configured cycle identifiers (e.g. "1" for key "cycle_1").
        /// </summary>
        public IReadOnlyList<string> GetAvailableCycleIds()
        {
            var cycles = _config.Properties()
                .Where(p => p.Name.StartsWith("cycle_", StringComparison.OrdinalIgnoreCase) && p.Value.Type == JTokenType.Object)
                .Select(p => p.Name.Substring("cycle_".Length))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            return cycles
                .OrderBy(id => int.TryParse(id, out var numericId) ? numericId : int.MaxValue)
                .ThenBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Attempts to load template-normalizer defaults from a specific cycle key (cycle_{id}).
        /// </summary>
        public bool TryGetCycleTemplateDefaults(
            string cycleId,
            out string docNumber,
            out string projectNumber,
            out string testPlan,
            out string footer)
        {
            docNumber = string.Empty;
            projectNumber = string.Empty;
            testPlan = string.Empty;
            footer = string.Empty;

            if (string.IsNullOrWhiteSpace(cycleId))
            {
                return false;
            }

            string cycleKey = $"cycle_{cycleId.Trim()}";
            if (_config[cycleKey] is not JObject cycleConfig)
            {
                return false;
            }

            docNumber = cycleConfig[AppConstants.ConfigKeyProtocolNumber]?.ToString() ?? string.Empty;
            projectNumber = cycleConfig[AppConstants.ConfigKeyProjectNumber]?.ToString() ?? string.Empty;
            testPlan = cycleConfig[AppConstants.ConfigKeyTestPlan]?.ToString() ?? string.Empty;
            footer = cycleConfig[AppConstants.ConfigKeyFooter]?.ToString() ?? string.Empty;

            return true;
        }
    }
}
