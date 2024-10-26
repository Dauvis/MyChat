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
        private readonly SystemMessageUtil _systemMessageUtil;

        public NewChatWindow(NewChatViewModel viewModel, SystemMessageUtil systemMessageUtil)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;
            _systemMessageUtil = systemMessageUtil;

            InitToneCombo();
        }

        private void InitToneCombo()
        {
            ToneCombo.Items.Add("");

            foreach (string tone in _systemMessageUtil.AvailableTones())
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
