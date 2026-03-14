using ste_tool_studio.Services;
using ste_tool_studio.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ste_tool_studio
{
    /// <summary>
    /// Interaction logic for STDTemplateNormalizer.xaml
    /// </summary>
    public partial class STDTemplateNormalizer : BaseToolWindow
    {
        private readonly STDTemplateNormalizerViewModel _viewModel;

        public STDTemplateNormalizer()
        {
            InitializeComponent();
            
            // Initialize StatusTextBlock reference after InitializeComponent
            StatusTextBlock = this.StatusText;

            // Create ViewModel for DOCX files (already configured in ViewModel)
            _viewModel = ServiceFactory.CreateSTDTemplateNormalizerViewModel();
            
            DataContext = _viewModel;

            // Subscribe to ViewModel property changes for UI updates
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Initialize toggle buttons
            UpdateToggleButtons();
        }

        private void UpdateStatusDisplay()
        {
            // The StatusText is bound to StatusMessage, but we need to update the color based on IsError
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    _viewModel.IsError
                        ? System.Windows.Media.Color.FromRgb(255, 102, 102)
                        : System.Windows.Media.Color.FromRgb(255, 255, 255));
            }
        }

        // Implement abstract methods from BaseToolWindow
        protected override bool IsValidFileType(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            return Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase);
        }

        protected override string GetExpectedFileType()
        {
            return "DOCX files (.docx)";
        }

        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RunSTDNormalizer();
        }

        protected override void HandleFileDrop(string filePath)
        {
            _viewModel.HandleFileSelection(filePath);
        }

        private void STDNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(STDNameInput, STDNamePlaceholder);
        }

        private void ProtocolNumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(ProtocolNumberInput, ProtocolNumberPlaceholder);
        }

        private void ReportNumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(ReportNumberInput, ReportNumberPlaceholder);
        }

        private void PreparedByInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(PreparedByInput, PreparedByPlaceholder);
        }


        private void TestPlanInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(TestPlanInput, TestPlanPlaceholder);
        }

        private void STxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(STxInput, STxPlaceholder);
        }

        // Simple click handlers - just call ViewModel methods directly
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectFile();
        }

        private void ProtocolButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsReportMode = false;
            UpdateToggleButtons();
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsReportMode = true;
            UpdateToggleButtons();
        }

        private void UpdateToggleButtons()
        {
            if (_viewModel.IsReportMode)
            {
                // Report mode - green (selected)
                ReportButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(76, 175, 80)); // #4CAF50
                ReportButton.Foreground = System.Windows.Media.Brushes.White;
                
                // Protocol mode - grey/black (unselected)
                ProtocolButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 30, 30)); // #1E1E1E
                ProtocolButton.Foreground = System.Windows.Media.Brushes.LightGray;
            }
            else
            {
                // Protocol mode - green (selected)
                ProtocolButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(76, 175, 80)); // #4CAF50
                ProtocolButton.Foreground = System.Windows.Media.Brushes.White;
                
                // Report mode - grey/black (unselected)
                ReportButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 30, 30)); // #1E1E1E
                ReportButton.Foreground = System.Windows.Media.Brushes.LightGray;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle UI-specific updates based on ViewModel property changes
            switch (e.PropertyName)
            {
                case nameof(STDTemplateNormalizerViewModel.StatusMessage):
                case nameof(STDTemplateNormalizerViewModel.IsError):
                    UpdateStatusDisplay();
                    break;
                case nameof(STDTemplateNormalizerViewModel.IsReportMode):
                    UpdateToggleButtons();
                    break;
            }
        }
    }
}
