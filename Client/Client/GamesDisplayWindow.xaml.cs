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
    /// Interaction logic for GamesDisplayWindow.xaml
    /// </summary>
    public partial class GamesDisplayWindow : Window
    {
        public string UserName_1 { get; set; }
        public string UserName_2 { get; set; }

        public GamesDisplayWindow(string UserName_1, string UserName_2)
        {
            this.UserName_1 = UserName_1;
            this.UserName_2 = UserName_2;
            InitializeComponent();
        }

        public FourRowServiceClient Client { get; internal set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var games = Client.GetAllPlayedGames(UserName_1, UserName_2);
            if (games.Count == 0)
            {
                NoGamesLabel.Visibility = Visibility.Visible;
            }
            else
            {
                List<GameShow> gamesView = new List<GameShow>();
                foreach (var game in games)
                {
                    gamesView.Add(new GameShow()
                    {
                        ID = game.Key,
                        Start = game.Value.Item1,
                        End =   game.Value.Item2,
                        Winner = game.Value.Item3,
                        WScore = game.Value.Item4,
                        Losser = game.Value.Item5,
                        LScore = game.Value.Item6,
                        Draw = game.Value.Item7
                    });
                }
                GamesGrid.ItemsSource = gamesView;
                GamesGrid.Visibility = Visibility.Visible;
            }

            if (UserName_1 == null || UserName_2 == null)
            {
                Grid.SetColumn(GamesGrid, 0);
                Grid.SetRow(GamesGrid, 0);
                Grid.SetRowSpan(GamesGrid, 3);
                Grid.SetColumn(NoGamesLabel, 0);
                Grid.SetRow(NoGamesLabel, 0);
                Grid.SetRowSpan(NoGamesLabel, 3);

            }
            else
            if(UserName_2 != null)
            {
                int winsOne = 0, winsTwo = 0;
                double prcntOne = 0, prcntTwo = 0;
                if (games.Count != 0) {
                foreach (var game in games)
                {
                    if (game.Value.Item3.Equals(UserName_1) && game.Value.Item7 == false) winsOne++;
                    if (game.Value.Item3.Equals(UserName_2) && game.Value.Item7 == false) winsTwo++;
                }

                    prcntOne = (winsOne*1.0 / games.Count*1.0)*100;
                    prcntTwo = winsTwo*1.0 / (games.Count*1.0)*100;
                }

                lblName1.Content = $"{UserName_1} :";
                lblName2.Content = $"{UserName_2} :";

                tbScore1.Text = String.Format("{0:0.00}", prcntOne) + "%";
                tbScore2.Text = String.Format("{0:0.00}", prcntTwo) + "%";
                lblName1.Visibility = Visibility.Visible;
                lblName2.Visibility = Visibility.Visible;
                tbScore1.Visibility = Visibility.Visible;
                tbScore2.Visibility = Visibility.Visible;
            }
        }
    }

    /// <summary>
    /// help to represent more clearly
    /// </summary>
    class GameShow
    {
        public int ID { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string Winner { get; set; }
        public int WScore { get; set; }
        public string Losser { get; set; }
        public int LScore { get; set; }
        public bool? Draw { get; set; }

    }
}
