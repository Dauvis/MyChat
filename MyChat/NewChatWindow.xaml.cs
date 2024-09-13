using MyChat.Util;
using MyChat.ViewModel;
using System.Windows;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for NewChatWindow.xaml
    /// </summary>
    public partial class NewChatWindow : Window
    {
        private NewChatViewModel _viewModel;

        public NewChatWindow(NewChatViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;
            _viewModel.LoadUserSettings();

            InitToneCombo();
        }

        private void InitToneCombo()
        {
            ToneCombo.Items.Add("");

            foreach (string tone in SystemPrompts.AvailableTones())
            {
                ToneCombo.Items.Add(tone);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult= false;
            Close();
        }
    }
}
