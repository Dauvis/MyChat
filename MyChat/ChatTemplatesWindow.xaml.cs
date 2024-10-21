using CommunityToolkit.Mvvm.Messaging;
using MyChat.Messages;
using MyChat.Util;
using MyChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for ChatTemplatesWindow.xaml
    /// </summary>
    public partial class ChatTemplatesWindow : Window
    {
        private readonly ChatTemplatesViewModel _viewModel;
        private readonly SystemMessageUtil _systemMessageUtil;

        public ChatTemplatesWindow(ChatTemplatesViewModel viewModel, SystemMessageUtil systemMessageUtil)
        {
            _viewModel = viewModel;
            _systemMessageUtil = systemMessageUtil;
            DataContext = _viewModel;

            InitializeComponent();
            InitToneCombo();

            WeakReferenceMessenger.Default.Register<WindowEventMessage>(this, (r, m) => OnWindowEventMessage(m));
        }

        private void OnWindowEventMessage(WindowEventMessage m)
        {
            if (m.Type == WindowType.ChatTemplate && m.State == WindowEventType.DoClose)
            {
                Close();
            }
        }

        private void InitToneCombo()
        {
            ToneCombo.Items.Add("");

            foreach (string tone in _systemMessageUtil.AvailableTones())
            {
                ToneCombo.Items.Add(tone);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Loaded, WindowType.ChatTemplate));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var message = new WindowEventMessage(WindowEventType.Closing, WindowType.ChatTemplate);
            WeakReferenceMessenger.Default.Send(message);

            if (message.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var parameters = new { NewItem = e.NewValue, OldItem = e.OldValue };
            WindowEventMessage message = new(WindowEventType.Selection, WindowType.ChatTemplate)
            {
                ObjectParameter = parameters
            };

            WeakReferenceMessenger.Default.Send(message);
        }
    }
}
