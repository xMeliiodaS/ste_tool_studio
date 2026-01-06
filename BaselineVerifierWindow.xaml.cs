using ste_tool_studio.Constants;
using ste_tool_studio.Services;
using ste_tool_studio.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ste_tool_studio
{
    /// <summary>
    /// Baseline Verifier tool window with minimal code-behind, using MVVM pattern
    /// </summary>
    public partial class BaselineVerifierWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private Storyboard _automationSpinnerStoryboard;
        private Storyboard _violationSpinnerStoryboard;

        public BaselineVerifierWindow()
        {
            InitializeComponent();
            _viewModel = ServiceFactory.CreateMainViewModel();
            DataContext = _viewModel;

            // Subscribe to ViewModel property changes for UI updates
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            this.Closing += BaselineVerifierWindow_Closing;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle UI-specific updates based on ViewModel property changes
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.IsAutomationRunning):
                    if (_viewModel.IsAutomationRunning)
                        StartButtonSpinner(RunAutomationButton, RunIcon, RunButtonText, ButtonSpinner, ButtonSpinnerRotate, ref _automationSpinnerStoryboard);
                    else
                        StopButtonSpinner(RunAutomationButton, RunIcon, RunButtonText, ButtonSpinner, ref _automationSpinnerStoryboard);
                    break;

                case nameof(MainViewModel.IsViolationRunning):
                    if (_viewModel.IsViolationRunning)
                        StartButtonSpinner(RunViolationButton, ViolationIcon, RunViolationButtonText, ViolationButtonSpinner, ViolationButtonSpinnerRotate, ref _violationSpinnerStoryboard);
                    else
                        StopButtonSpinner(RunViolationButton, ViolationIcon, RunViolationButtonText, ViolationButtonSpinner, ref _violationSpinnerStoryboard);
                    break;

                case nameof(MainViewModel.IsRunning):
                    SetActionButtonsEnabled(!_viewModel.IsRunning);
                    break;

                case nameof(MainViewModel.StatusMessage):
                case nameof(MainViewModel.IsError):
                    UpdateStatusDisplay();
                    break;
            }
        }

        private void UpdateStatusDisplay()
        {
            StatusText.Text = _viewModel.StatusMessage;
            StatusText.FontWeight = FontWeights.Bold;
            StatusText.Foreground = new SolidColorBrush(
                _viewModel.IsError
                    ? Color.FromRgb(255, 102, 102)
                    : Color.FromRgb(255, 255, 255));
        }

        // UI Event Handlers - Minimal code-behind for UI-specific behavior

        private void STDNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            STDPlaceholder.Visibility = string.IsNullOrEmpty(STDNameInput.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void IterationPathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            IterationPathPlaceholder.Visibility = string.IsNullOrEmpty(IterationPathInput.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void VVVersionInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            VVVersionPlaceholder.Visibility = string.IsNullOrEmpty(VVVersionInput.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectFileCommand.Execute(null);
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsValidExcelFile(files[0]))
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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsValidExcelFile(files[0]))
                {
                    _viewModel.HandleFileSelection(files[0]);
                }
                else
                {
                    _viewModel.SetStatus(AppConstants.ErrorInvalidFileFormat, true);
                }
            }
        }

        private bool IsValidExcelFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".xls" || extension == ".xlsx";
        }

        // Spinner animation helpers
        private void StartButtonSpinner(Button targetButton, TextBlock icon, TextBlock buttonText, Ellipse spinner, RotateTransform spinnerRotate, ref Storyboard storyboard)
        {
            spinner.Visibility = Visibility.Visible;
            icon.Visibility = Visibility.Collapsed;

            storyboard = new Storyboard();
            var anim = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(anim, spinnerRotate);
            Storyboard.SetTargetProperty(anim, new PropertyPath("Angle"));
            storyboard.Children.Add(anim);
            storyboard.Begin();

            targetButton.IsEnabled = false;
            buttonText.Text = AppConstants.ButtonTextRunning;
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        private void StopButtonSpinner(Button targetButton, TextBlock icon, TextBlock buttonText, Ellipse spinner, ref Storyboard storyboard)
        {
            storyboard?.Stop();
            spinner.Visibility = Visibility.Collapsed;
            icon.Visibility = Visibility.Visible;

            if (targetButton == RunAutomationButton)
                buttonText.Text = AppConstants.ButtonTextCheckStdBugs;
            else if (targetButton == RunViolationButton)
                buttonText.Text = AppConstants.ButtonTextValidateStdRules;

            targetButton.IsEnabled = true;
        }

        private void SetActionButtonsEnabled(bool isEnabled)
        {
            RunAutomationButton.IsEnabled = isEnabled;
            RunViolationButton.IsEnabled = isEnabled;
            SelectSTDButton.IsEnabled = isEnabled;

            STDNameInput.IsEnabled = isEnabled;
            IterationPathInput.IsEnabled = isEnabled;
            VVVersionInput.IsEnabled = isEnabled;
        }

        // Button click handlers - delegate to ViewModel commands
        private void RunAutomation_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RunAutomationCommand.Execute(null);
        }

        private void RunViolationCheck_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RunViolationCheckCommand.Execute(null);
        }

        private void OpenLastBugsReport_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenLastBugsReportCommand.Execute(null);
        }

        private void OpenLastRulesReport_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenLastRulesReportCommand.Execute(null);
        }

        private static MainMenuWindow _mainMenuWindow;

        private void BaselineVerifierWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If already shutting down, don't do anything
            if (MainMenuWindow.IsShuttingDown) return;

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

        // Back button handler - return to main menu
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
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
        }
    }
}

