using ste_tool_studio.Constants;
using ste_tool_studio.Services;
using ste_tool_studio.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ste_tool_studio
{
    /// <summary>
    /// Interaction logic for STDTemplateNormalizer.xaml
    /// </summary>
    public partial class STDTemplateNormalizer : BaseToolWindow
    {
        private readonly MainViewModel _viewModel;

        public STDTemplateNormalizer()
        {
            InitializeComponent();
            
            // Initialize StatusTextBlock reference after InitializeComponent
            StatusTextBlock = this.StatusText;

            // Create and configure ViewModel for DOCX files
            _viewModel = ServiceFactory.CreateMainViewModel();
            _viewModel.FileFilter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*";
            _viewModel.FileDialogTitle = "Select DOCX File";
            _viewModel.AllowedExtensions = new[] { ".docx" };
            
            DataContext = _viewModel;

            // Subscribe to ViewModel property changes for UI updates
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle UI-specific updates based on ViewModel property changes
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.StatusMessage):
                case nameof(MainViewModel.IsError):
                    UpdateStatusDisplay();
                    break;
            }
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

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectFileCommand.Execute(null);
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

        protected override void HandleFileDrop(string filePath)
        {
            _viewModel.HandleFileSelection(filePath);
        }

        private void STDNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(STDNameInput, STDNamePlaceholder);
        }

        private void DOCNumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(DOCNumberInput, DOCNumberPlaceholder);
        }

        private void PreparedByInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(PreparedByInput, PreparedByPlaceholder);
        }

        private void PlanInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(ProjectNumberInput, ProjectNumberPlaceholder);
        }

        private void TestPlanInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(TestPlanInput, TestPlanPlaceholder);
        }

        private void FooterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(FooterInput, FooterPlaceholder);
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement generation logic
            
        }
    }
}
