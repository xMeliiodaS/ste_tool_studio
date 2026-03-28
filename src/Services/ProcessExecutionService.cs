using ste_tool_studio.Constants;
using System.Diagnostics;
using System.IO;
using System.Text;
using IOPath = System.IO.Path;

namespace ste_tool_studio.Services
{
    /// <summary>
    /// Service for executing external processes with progress reporting
    /// </summary>
    public class ProcessExecutionService : IProcessExecutionService
    {
        public async Task<ProcessExecutionResult> ExecuteAsync(
            string exeName,
            string arguments,
            System.Action<int, int, string> progressCallback = null)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidatePaths =
            {
                IOPath.Combine(baseDirectory, "Scripts", exeName),
                IOPath.Combine(baseDirectory, exeName)
            };

            string exePath = candidatePaths.FirstOrDefault(File.Exists);

            if (string.IsNullOrWhiteSpace(exePath))
            {
                throw new FileNotFoundException(
                    string.Format(AppConstants.ErrorExeNotFound, exeName),
                    IOPath.Combine(baseDirectory, "Scripts", exeName));
            }

            var result = new ProcessExecutionResult();
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            await Task.Run(() =>
            {
                using (var process = new Process())
                {
                    process.StartInfo = psi;

                    process.OutputDataReceived += (s, ea) =>
                    {
                        if (string.IsNullOrEmpty(ea.Data)) return;

                        outputBuilder.AppendLine(ea.Data);

                        // Parse progress markers
                        if (ea.Data.StartsWith(AppConstants.ProgressTotalMarker))
                        {
                            if (int.TryParse(ea.Data.Substring(AppConstants.ProgressTotalMarker.Length).Trim(), out int total))
                            {
                                progressCallback?.Invoke(0, total, $"Processing 0/{total} bugs...");
                            }
                        }
                        else if (ea.Data.StartsWith(AppConstants.ProgressMarker))
                        {
                            var parts = ea.Data.Substring(AppConstants.ProgressMarker.Length).Trim().Split('/');
                            if (parts.Length == 2
                                && int.TryParse(parts[0], out int current)
                                && int.TryParse(parts[1], out int total))
                            {
                                progressCallback?.Invoke(current, total, $"Processing {current}/{total} bugs...");
                            }
                        }
                        else if (ea.Data.Trim() == AppConstants.ProcessFinishedMarker)
                        {
                            progressCallback?.Invoke(-1, -1, AppConstants.DefaultStatusAllBugsProcessed);
                        }
                        else
                        {
                            progressCallback?.Invoke(-1, -1, ea.Data);
                        }
                    };

                    process.ErrorDataReceived += (s, ea) =>
                    {
                        if (!string.IsNullOrEmpty(ea.Data))
                            errorBuilder.AppendLine(ea.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    result.ExitCode = process.ExitCode;
                }
            });

            result.Output = outputBuilder.ToString();
            result.Error = errorBuilder.ToString();

            return result;
        }
    }
}
