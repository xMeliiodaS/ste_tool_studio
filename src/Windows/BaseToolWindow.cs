using System.Windows;
using System.Windows.Controls;
using System.Linq;
using ste_tool_studio.Configuration;
using ste_tool_studio.Services;

namespace ste_tool_studio
{
    /// <summary>
    /// Base class for tool windows with common functionality like navigation, file handling, and UI helpers
    /// </summary>
    public abstract class BaseToolWindow : Window
    {
        protected static MainMenuWindow _mainMenuWindow;
        private static ILoggingService _loggingService;
        protected TextBlock StatusTextBlock { get; set; }

        protected BaseToolWindow()
        {
            this.Closing += BaseToolWindow_Closing;
            _loggingService ??= new FileLoggingService(new AppConfiguration());

            // Wire drag-drop in code so it works regardless of XAML.
            // Preview (tunneling) events fire at the window BEFORE any child control
            // can swallow the drop (a common WPF drag-drop failure).
            this.AllowDrop = true;
            this.PreviewDragOver += Window_DragOver;
            this.PreviewDrop += Window_Drop;
        }

        /// <summary>
        /// Handles window closing logic - common to all tool windows
        /// </summary>
        private void BaseToolWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If already shutting down, don't do anything
            if (MainMenuWindow.IsShuttingDown) return;
            _loggingService?.LogInfo("Application shutdown initiated.");

            // If closing via X button (not hiding), shut down the application
            try
            {
                if (_mainMenuWindow != null && _mainMenuWindow.IsLoaded)
                {
                    _mainMenuWindow.Close();
                }
                else
                {
                    MainMenuWindow.IsShuttingDown = true;
                    Application.Current.Shutdown();
                }
            }
            catch { }
        }

        /// <summary>
        /// Handles back button click - returns to main menu
        /// </summary>
        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _loggingService?.LogInfo($"User clicked Back from {GetType().Name}.");

            _mainMenuWindow = Application.Current?.Windows
                .OfType<MainMenuWindow>()
                .FirstOrDefault(window => window.IsLoaded);

            // Show existing main menu or create new one
            if (_mainMenuWindow == null || !_mainMenuWindow.IsLoaded)
            {
                _mainMenuWindow = new MainMenuWindow();
                _mainMenuWindow.Closed += (s, args) => _mainMenuWindow = null;
            }

            _mainMenuWindow.Show();
            _mainMenuWindow.Activate();

            // Hide this window instead of closing to preserve state
            this.Hide();
            _loggingService?.LogInfo($"Output: Returned to main menu from {GetType().Name}.");
        }

        /// <summary>
        /// Handles drag over event - validates file type
        /// </summary>
        public void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsValidFileType(files[0]))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles drop event - processes dropped file
        /// </summary>
        public void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsValidFileType(files[0]))
                {
                    HandleFileDrop(files[0]);
                }
                else
                {
                    SetStatusMessage($"Invalid file format. Expected: {GetExpectedFileType()}", isError: true);
                }
            }
        }

        /// <summary>
        /// Helper method to handle placeholder text visibility
        /// </summary>
        protected void HandlePlaceholderVisibility(TextBox textBox, TextBlock placeholder)
        {
            if (textBox != null && placeholder != null)
            {
                placeholder.Visibility = string.IsNullOrWhiteSpace(textBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Sets the status message text
        /// </summary>
        protected void SetStatusMessage(string message, bool isError = false)
        {
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = message;
                StatusTextBlock.FontWeight = FontWeights.Bold;
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    isError
                        ? System.Windows.Media.Color.FromRgb(255, 102, 102)
                        : System.Windows.Media.Color.FromRgb(255, 255, 255));
            }
        }

        /// <summary>
        /// Abstract method to validate file type - must be implemented by derived classes
        /// </summary>
        protected abstract bool IsValidFileType(string filePath);

        /// <summary>
        /// Abstract method to get expected file type description for error messages
        /// </summary>
        protected abstract string GetExpectedFileType();

        /// <summary>
        /// Abstract method to handle file drop - must be implemented by derived classes
        /// </summary>
        protected abstract void HandleFileDrop(string filePath);
    }
}
