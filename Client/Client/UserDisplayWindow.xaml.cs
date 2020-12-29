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
    /// Interaction logic for UserDisplayWindow.xaml
    /// </summary>
    public partial class UserDisplayWindow : Window
    {
        public UserDisplayWindow()
        {
            InitializeComponent();
        }

        public FourRowServiceClient Client { get; internal set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var x = Client.GetDataAllUsers(null).Item2;
            List<UserShow> users = new List<UserShow>();
            foreach (var usr in x) {
                users.Add(new UserShow()
                {
                    Name = usr.Key,
                    Loses = usr.Value.Item1,
                    Wins = usr.Value.Item2,
                    Draws = usr.Value.Item3,
                    Score = usr.Value.Item4,
                    Games = usr.Value.Item5
                });
            }
            UsersGrid.ItemsSource = users;
        }

    }

    class UserShow {
        public string Name { get; set; }
        public int Loses { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Score { get; set; }
        public int Games { get; set; }
    }
}
