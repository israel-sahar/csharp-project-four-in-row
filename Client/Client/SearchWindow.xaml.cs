using Client.CFGameServiceReference;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public FourRowServiceClient Client { get; internal set; }
        public ClientCallback Callback { get; internal set; }
        public string UserName { get; internal set; }
        public MainWindow Main { get; internal set; }

        public SearchWindow(string userName, ClientCallback callback, FourRowServiceClient client)
        {
            InitializeComponent();
            UserName = userName;
            Callback = callback;
            Client = client;
        }

        private void btnSPlayerClick(object sender, RoutedEventArgs e)
        {
            UserDisplayWindow usrWindow = new UserDisplayWindow();
            usrWindow.Client = Client;
            usrWindow.Owner = Main;

            usrWindow.Show();
        }

        private void btnSGameActiveClick(object sender, RoutedEventArgs e)
        {
            ActiveGamesDisplayWindow gamesWindow = new ActiveGamesDisplayWindow();
            gamesWindow.Client = Client;
            gamesWindow.Owner = Main;
            gamesWindow.Show();
        }

        private void btnSGameFinishedClick(object sender, RoutedEventArgs e)
        {
            GamesDisplayWindow gamesWindow = new GamesDisplayWindow(null,null);
            gamesWindow.Client = Client;
            gamesWindow.Owner = Main;
            gamesWindow.Show();
        }
    }
}
