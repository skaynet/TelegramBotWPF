using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telegram.Bot.Types;

namespace TelegramBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TelegramBotService _botService;

        public MainWindow()
        {
            InitializeComponent();

            _botService = new TelegramBotService(receivedMessage);
            lstBoxUsers.ItemsSource = _botService.Users;
            btnSendMessage.IsEnabled = false;
        }

        private void receivedMessage()
        {
            this.Dispatcher.Invoke(() =>
            {
                lstBoxUsers.Items.Refresh();
                lstBoxMessages.Items.Refresh();
            });
        }

        private void onClickBtnSendMessage(object sender, RoutedEventArgs e)
        {
            SendMessageToUser();
        }

        private void onKeyDownTxtBoxMessage(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessageToUser();
            }
        }

        private void SendMessageToUser()
        {
            if (lstBoxUsers.SelectedItem is TelegramUser concreteUser)
            {
                string message = txtBoxMessage.Text;
                _ = _botService.SendTextMessageAsync(concreteUser, message);
                txtBoxMessage.Text = String.Empty;
            }
        }

        private void onSelectedLstBoxUsers(object sender, SelectionChangedEventArgs e)
        {
            btnSendMessage.IsEnabled = true;
        }
    }
}
