using STD_baseline_verifier.Configuration;
using STD_baseline_verifier.Constants;
using System;
using System.Diagnostics;
using System.IO;
using IOPath = System.IO.Path;

namespace STD_baseline_verifier.Services
{
    /// <summary>
    /// Service for managing and opening report files
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly AppConfiguration _config;

        public ReportService(AppConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public bool OpenReport(string reportFileName)
        {
            string reportPath = GetReportPath(reportFileName);

            if (!File.Exists(reportPath))
            {
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = reportPath,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ReportExists(string reportFileName)
        {
            return File.Exists(GetReportPath(reportFileName));
        }

        public string GetReportPath(string reportFileName)
        {
            return _config.GetReportPath(reportFileName);
        }
    }
}

