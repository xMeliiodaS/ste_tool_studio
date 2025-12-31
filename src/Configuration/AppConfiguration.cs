using Newtonsoft.Json.Linq;
using System;
using System.IO;
using STD_baseline_verifier.Constants;
using IOPath = System.IO.Path;

namespace STD_baseline_verifier.Configuration
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

        /// <summary>
        /// Updates multiple configuration values at once
        /// </summary>
        public void UpdateConfig(string stdName, string iterationPath, string currentVersion)
        {
            StdName = stdName;
            IterationPath = iterationPath;
            CurrentVersion = currentVersion;
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
        /// Gets the path to the log file
        /// </summary>
        public string GetLogFilePath()
        {
            return IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.LogFileName);
        }
    }
}

