using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ste_tool_studio
{
    /// <summary>
    /// Interaction logic for STDTemplateNormalizer.xaml
    /// </summary>
    public partial class STDTemplateNormalizer : Window
    {
        private string _selectedFilePath = string.Empty;
        private static MainMenuWindow _mainMenuWindow;

        public STDTemplateNormalizer()
        {
            InitializeComponent();
            this.Closing += STDTemplateNormalizer_Closing;
        }

        private void STDTemplateNormalizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                Title = "Select DOCX File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                SelectedFileLabel.Text = Path.GetFileName(_selectedFilePath);
                StatusText.Text = $"File selected: {Path.GetFileName(_selectedFilePath)}";
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    if (Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase))
                    {
                        _selectedFilePath = filePath;
                        SelectedFileLabel.Text = Path.GetFileName(_selectedFilePath);
                        StatusText.Text = $"File selected: {Path.GetFileName(_selectedFilePath)}";
                    }
                    else
                    {
                        StatusText.Text = "Please select a DOCX file";
                    }
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void STDNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            STDNamePlaceholder.Visibility = string.IsNullOrWhiteSpace(STDNameInput.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void DOCNumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            DOCNumberPlaceholder.Visibility = string.IsNullOrWhiteSpace(DOCNumberInput.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void PreparedByInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            PreparedByPlaceholder.Visibility = string.IsNullOrWhiteSpace(PreparedByInput.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void PlanInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ProjectNumberPlaceholder.Visibility = string.IsNullOrWhiteSpace(ProjectNumberInput.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void TestPlanInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TestPlanPlaceholder.Visibility = string.IsNullOrWhiteSpace(TestPlanInput.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void FooterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            FooterPlaceholder.Visibility = string.IsNullOrWhiteSpace(FooterInput.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement generation logic
            StatusText.Text = "Generation functionality will be implemented here";
        }
    }
}
