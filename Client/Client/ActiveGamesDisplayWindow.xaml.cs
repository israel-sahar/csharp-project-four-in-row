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
    /// Interaction logic for ActiveGamesDisplayWindow.xaml
    /// </summary>
    public partial class ActiveGamesDisplayWindow : Window
    {
        public FourRowServiceClient Client { get; internal set; }

        public ActiveGamesDisplayWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// ask from server the running game and display them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var games = Client.GetRunningGames();
            if (games.Count == 0)
            {
                NoGamesLabel.Visibility = Visibility.Visible;
            }
            else
            {
                List<ActiveGameShow> gamesView = new List<ActiveGameShow>();
                foreach (var game in games)
                {
                    gamesView.Add(new ActiveGameShow()
                    {
                        ID = game.Key,
                        Start = game.Value.Item1,
                        Player1 = game.Value.Item2,
                        Player2 = game.Value.Item3
                    });
                }
                GamesGrid.ItemsSource = gamesView;
                GamesGrid.Visibility = Visibility.Visible;
            }
        }
    }

    /// <summary>
    /// this class help me to represent the data more clearly
    /// </summary>
    class ActiveGameShow
    {
        public int ID { get; set; }
        public DateTime Start { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
    }
}
