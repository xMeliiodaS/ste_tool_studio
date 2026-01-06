using System.Windows;

namespace ste_tool_studio
{
    /// <summary>
    /// Main menu window that serves as a launcher for all tools
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private static BaselineVerifierWindow _baselineVerifierWindow;
        private static STDTemplateNormalizer _stdTemplateNormalizerWindow;
        public static bool IsShuttingDown { get; internal set; } = false;

        public MainMenuWindow()
        {
            InitializeComponent();
            this.Closing += MainMenuWindow_Closing;
        }

        private void MainMenuWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsShuttingDown) return;
            IsShuttingDown = true;

            // Close all tool windows when main menu is closed
            try
            {
                if (_baselineVerifierWindow != null && _baselineVerifierWindow.IsLoaded)
                {
                    _baselineVerifierWindow.Close();
                }
            }
            catch { }

            try
            {
                if (_stdTemplateNormalizerWindow != null && _stdTemplateNormalizerWindow.IsLoaded)
                {
                    _stdTemplateNormalizerWindow.Close();
                }
            }
            catch { }
            
            // Shut down the application
            try
            {
                Application.Current.Shutdown();
            }
            catch { }
        }

        private void BaselineVerifierButton_Click(object sender, RoutedEventArgs e)
        {
            // Show existing window or create new one
            if (_baselineVerifierWindow == null || !_baselineVerifierWindow.IsLoaded)
            {
                _baselineVerifierWindow = new BaselineVerifierWindow();
                _baselineVerifierWindow.Closed += (s, args) => _baselineVerifierWindow = null;
            }
            _baselineVerifierWindow.Show();
            _baselineVerifierWindow.Activate();
            
            // Hide this window instead of closing to preserve state
            this.Hide();
        }

        private void STDTemplateNormalizer_click(object sender, RoutedEventArgs e)
        {
            // Show existing window or create new one
            if (_stdTemplateNormalizerWindow == null || !_stdTemplateNormalizerWindow.IsLoaded)
            {
                _stdTemplateNormalizerWindow = new STDTemplateNormalizer();
                _stdTemplateNormalizerWindow.Closed += (s, args) => _stdTemplateNormalizerWindow = null;
            }
            _stdTemplateNormalizerWindow.Show();
            _stdTemplateNormalizerWindow.Activate();

            // Hide this window instead of closing to preserve state
            this.Hide();
        }
    }
}

