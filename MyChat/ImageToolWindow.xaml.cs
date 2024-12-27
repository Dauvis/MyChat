using CommunityToolkit.Mvvm.Messaging;
using MyChat.Common.Interfaces;
using MyChat.Common.Messages;
using MyChat.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for ImageToolWindow.xaml
    /// </summary>
    public partial class ImageToolWindow : Window
    {
        private readonly ImageToolViewModel _viewModel;
        private readonly ISettingsService _settingsService;

        public ImageToolWindow(ImageToolViewModel viewModel, ISettingsService settingsService)
        {
            _viewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
            InitQualityCombo();
            InitSizeCombo();
            InitStyleCombo();

            _viewModel.Quality = "Standard";
            _viewModel.Size = "Standard";
            _viewModel.Style = "Vivid";

            _settingsService = settingsService;
        }

        private void InitQualityCombo()
        {
            QualityCombo.Items.Clear();
            QualityCombo.Items.Add("Standard");
            QualityCombo.Items.Add("High");
        }

        private void InitSizeCombo()
        {
            SizeCombo.Items.Clear();
            SizeCombo.Items.Add("Standard");
            SizeCombo.Items.Add("Portrait");
            SizeCombo.Items.Add("Landscape");
        }

        private void InitStyleCombo()
        {
            styleCombo.Items.Clear();
            styleCombo.Items.Add("Natural");
            styleCombo.Items.Add("Vivid");
        }

        private void HandleWindowLoaded()
        {
            var settings = _settingsService.GetUserSettings();
            Grid content = (Grid)Content;
            var columns = content.ColumnDefinitions;

            if (settings.ImageToolWindow.Rectangle.Width > 0)
            {
                Left = settings.ImageToolWindow.Rectangle.Left;
                Top = settings.ImageToolWindow.Rectangle.Top;
                Width = settings.ImageToolWindow.Rectangle.Width;
                Height = settings.ImageToolWindow.Rectangle.Height;
                columns[2].Width = new(settings.ImageToolWindow.PromptColumnWidth);
            }
        }

        private void HandleWindowClosed()
        {
            var settings = _settingsService.GetUserSettings();
            Grid content = (Grid)Content;
            var columns = content.ColumnDefinitions;

            settings.ImageToolWindow.Rectangle = new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
            settings.ImageToolWindow.PromptColumnWidth = columns[2].Width.Value;

            _settingsService.SetUserSettings(settings);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Closing, WindowType.ImageTool));
            HandleWindowClosed();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HandleWindowLoaded();
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Loaded, WindowType.ImageTool));
        }
    }
}
