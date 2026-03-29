using ste_tool_studio.Services;
using ste_tool_studio.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

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

        private void DOCNumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(DOCNumberInput, DOCNumberPlaceholder);
        }

        private void PreparedByInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(PreparedByInput, PreparedByPlaceholder);
        }

        private void TestPlanInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(TestPlanInput, TestPlanPlaceholder);
        }

        private void ReportNumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(ReportNumberInput, ReportNumberPlaceholder);
        }

        private void StxNumberSuffixInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandlePlaceholderVisibility(StxNumberSuffixInput, StxNumberPlaceholder);
        }

        // Simple click handlers - just call ViewModel methods directly
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectFile();
        }

        private void ProtocolButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPressAnimation(ProtocolScale);
            _viewModel.IsReportMode = false;
            UpdateToggleButtons();
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPressAnimation(ReportScale);
            _viewModel.IsReportMode = true;
            UpdateToggleButtons();
        }

        private void ProtocolButton_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlayPressAnimation(ProtocolScale);
        }

        private void ReportButton_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlayPressAnimation(ReportScale);
        }


        private void ProtocolButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AnimateHoverState(ProtocolButton, ProtocolScale, ProtocolShadow, true, !_viewModel.IsReportMode);
        }

        private void ProtocolButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateToggleButtons();
        }

        private void ReportButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AnimateHoverState(ReportButton, ReportScale, ReportShadow, true, _viewModel.IsReportMode);
        }

        private void ReportButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateToggleButtons();
        }

        private void PlayPressAnimation(ScaleTransform scaleTransform)
        {
            var pressAnimation = new DoubleAnimation
            {
                To = 0.88,
                Duration = TimeSpan.FromMilliseconds(110),
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pressAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pressAnimation.Clone());
        }

        private static SolidColorBrush EnsureAnimatableBackground(Button button, Color fallbackColor)
        {
            if (button.Background is SolidColorBrush existingBrush)
            {
                if (existingBrush.IsFrozen)
                {
                    var clone = existingBrush.Clone();
                    button.Background = clone;
                    return clone;
                }

                return existingBrush;
            }

            var brush = new SolidColorBrush(fallbackColor);
            button.Background = brush;
            return brush;
        }

        private static void AnimateToggleButton(
            Button button,
            ScaleTransform scale,
            DropShadowEffect shadow,
            bool isSelected)
        {
            var targetBackgroundColor = isSelected
                ? Color.FromRgb(76, 175, 80) // #4CAF50
                : Color.FromRgb(30, 30, 30); // #1E1E1E
            var targetForeground = isSelected ? Brushes.White : Brushes.LightGray;
            var targetScale = isSelected ? 1.04 : 1.0;
            var targetBlur = isSelected ? 14.0 : 0.0;
            var targetOpacity = isSelected ? 0.55 : 0.0;
            var targetBorderColor = isSelected
                ? Color.FromRgb(129, 199, 132)
                : Color.FromRgb(30, 30, 30);

            button.Foreground = targetForeground;

            var backgroundBrush = EnsureAnimatableBackground(button, targetBackgroundColor);
            backgroundBrush.BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation
                {
                    To = targetBackgroundColor,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            var borderBrush = EnsureAnimatableBorderBrush(button, targetBorderColor);
            borderBrush.BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation
                {
                    To = targetBorderColor,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            scale.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                new DoubleAnimation
                {
                    To = targetScale,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            scale.BeginAnimation(
                ScaleTransform.ScaleYProperty,
                new DoubleAnimation
                {
                    To = targetScale,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            shadow.BeginAnimation(
                DropShadowEffect.BlurRadiusProperty,
                new DoubleAnimation
                {
                    To = targetBlur,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            shadow.BeginAnimation(
                DropShadowEffect.OpacityProperty,
                new DoubleAnimation
                {
                    To = targetOpacity,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
        }


        private static void AnimateHoverState(
            Button button,
            ScaleTransform scale,
            DropShadowEffect shadow,
            bool isHovering,
            bool isSelected)
        {
            var targetScale = isHovering
                ? (isSelected ? 1.1 : 1.06)
                : (isSelected ? 1.04 : 1.0);
            var targetBlur = isHovering
                ? (isSelected ? 18.0 : 10.0)
                : (isSelected ? 14.0 : 0.0);
            var targetOpacity = isHovering
                ? (isSelected ? 0.75 : 0.35)
                : (isSelected ? 0.55 : 0.0);
            var targetBorderColor = isHovering && !isSelected
                ? Color.FromRgb(76, 175, 80)
                : isSelected
                    ? Color.FromRgb(129, 199, 132)
                    : Color.FromRgb(30, 30, 30);

            var borderBrush = EnsureAnimatableBorderBrush(button, targetBorderColor);
            borderBrush.BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation
                {
                    To = targetBorderColor,
                    Duration = TimeSpan.FromMilliseconds(140),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            scale.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                new DoubleAnimation
                {
                    To = targetScale,
                    Duration = TimeSpan.FromMilliseconds(140),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            scale.BeginAnimation(
                ScaleTransform.ScaleYProperty,
                new DoubleAnimation
                {
                    To = targetScale,
                    Duration = TimeSpan.FromMilliseconds(140),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            shadow.BeginAnimation(
                DropShadowEffect.BlurRadiusProperty,
                new DoubleAnimation
                {
                    To = targetBlur,
                    Duration = TimeSpan.FromMilliseconds(140),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            shadow.BeginAnimation(
                DropShadowEffect.OpacityProperty,
                new DoubleAnimation
                {
                    To = targetOpacity,
                    Duration = TimeSpan.FromMilliseconds(140),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
        }

        private static SolidColorBrush EnsureAnimatableBorderBrush(Button button, Color fallbackColor)
        {
            if (button.BorderBrush is SolidColorBrush existingBrush)
            {
                if (existingBrush.IsFrozen)
                {
                    var clone = existingBrush.Clone();
                    button.BorderBrush = clone;
                    return clone;
                }

                return existingBrush;
            }

            var brush = new SolidColorBrush(fallbackColor);
            button.BorderBrush = brush;
            return brush;
        }

        private void UpdateToggleButtons()
        {
            AnimateToggleButton(ProtocolButton, ProtocolScale, ProtocolShadow, !_viewModel.IsReportMode);
            AnimateToggleButton(ReportButton, ReportScale, ReportShadow, _viewModel.IsReportMode);
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
