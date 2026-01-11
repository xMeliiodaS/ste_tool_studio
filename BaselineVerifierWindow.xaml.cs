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
    public partial class BaselineVerifierWindow : BaseToolWindow
    {
        private readonly MainViewModel _viewModel;
        private Storyboard _automationSpinnerStoryboard;
        private Storyboard _violationSpinnerStoryboard;

        public BaselineVerifierWindow()
        {
            InitializeComponent();
            _viewModel = ServiceFactory.CreateMainViewModel();
            DataContext = _viewModel;

            // Initialize StatusTextBlock reference after InitializeComponent
            StatusTextBlock = this.StatusText;

            // Subscribe to ViewModel property changes for UI updates
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
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
            SetStatusMessage(_viewModel.StatusMessage, _viewModel.IsError);
        }

        // UI Event Handlers - Minimal code-behind for UI-specific behavior

        private void STDNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(STDNameInput, STDPlaceholder);
        }

        private void IterationPathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(IterationPathInput, IterationPathPlaceholder);
        }

        private void VVVersionInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(VVVersionInput, VVVersionPlaceholder);
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectFileCommand.Execute(null);
        }

        // Implement abstract methods from BaseToolWindow
        protected override bool IsValidFileType(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".xls" || extension == ".xlsx";
        }

        protected override string GetExpectedFileType()
        {
            return "Excel files (.xls, .xlsx)";
        }

        protected override void HandleFileDrop(string filePath)
        {
            _viewModel.HandleFileSelection(filePath);
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
            this.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
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
    }
}

