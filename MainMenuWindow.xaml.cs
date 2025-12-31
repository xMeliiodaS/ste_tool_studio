using System.Windows;

namespace AT_baseline_verifier
{
    /// <summary>
    /// Main menu window that serves as a launcher for all tools
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
        }

        private void BaselineVerifierButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the Baseline Verifier tool window
            var baselineVerifierWindow = new BaselineVerifierWindow();
            baselineVerifierWindow.Show();
            
            // Close this window
            this.Close();
        }
    }
}

