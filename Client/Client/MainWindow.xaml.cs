using Client.CFGameServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string UserName { get; internal set; }
        public string HashedPassword { get; internal set; }
        public FourRowServiceClient Client { get; internal set; }
        public ClientCallback Callback { get; internal set; }

        public MainWindow(string userName, string pass, FourRowServiceClient client, ClientCallback callback)
        {
            InitializeComponent();
            this.HashedPassword = pass;
            this.UserName = userName;
            this.Client = client;
            this.Callback = callback;
            
            Thread updateThread = new Thread(() => UpdateDataWindow(Client.GetDataAllUsers(UserName).Item2));
            updateThread.Start();

            updateThread = new Thread(() => InitActiveUsers());
            updateThread.Start();

            lblName.Content = "Welcome," + UserName;
        }

        private void InitActiveUsers()
        {
            Thread.Sleep(3000);
            this.Dispatcher.Invoke(() =>
            {
                lbUsers.ItemsSource = Client.GetDataAllUsers(UserName).Item1.Except(new string[] { UserName });
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
                Callback.OpenNewGame = startGame;
                Callback.UpdateConnectedClients = UpdateConnectedClients;
        }

        /// <summary>
        /// update the info according to the user
        /// </summary>
        /// <param name="userInfo"></param>
        public void UpdateDataWindow(Dictionary<string, (int, int, int, int, int)> userInfo)
        {
            this.Dispatcher.Invoke(() =>
        {
            var name = userInfo.Keys.First();
            titleLbl.Content = name.Equals(UserName) ? "Your Record:" : name + "'s Record:";
            tbGames.Text = userInfo[name].Item5.ToString();
            tbWins.Text = userInfo[name].Item2.ToString();
            tbLoses.Text = userInfo[name].Item1.ToString();
            tbPoints.Text = userInfo[name].Item4.ToString();
            if (userInfo[name].Item5 == 0)
                tbWinsPercent.Text = "0%";
            else
            {
                double res = (userInfo[name].Item2 * 1.0 / userInfo[name].Item5 * 1.0) * 100;
                string resText = String.Format("{0:0.00}", res) + "%";
                tbWinsPercent.Text = resText;
            }
        });
        }

        /// <summary>
        /// call by the service to update online players
        /// </summary>
        /// <param name="users"></param>
        public void UpdateConnectedClients(string[] users)
        {
            lbUsers.SelectionChanged -= lbUsers_SelectionChanged;
            try
            {
                var selectedUser = lbUsers.SelectedItem;

                lbUsers.ItemsSource = null;
                clearBtn.IsEnabled = false;

                lbUsers.ItemsSource = users.Except(new string[] { UserName });

                if (selectedUser != null && users.Contains(selectedUser.ToString()))
                {
                    lbUsers.SelectedItem = selectedUser;
                    clearBtn.IsEnabled = true;
                }
                else
                    if (selectedUser != null)
                    UpdateDataWindow(Client.GetDataAllUsers(UserName).Item2);
            }
            catch (Exception) {
                MessageBox.Show("The service off.");
                LoginWindow lgnWindow = new LoginWindow();
                this.Closing -= Window_Closing;
                this.Close();
                lgnWindow.Show();
            }
            lbUsers.SelectionChanged += lbUsers_SelectionChanged;

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Client.Disconnect(UserName);
            }
            catch (Exception) { }
            Environment.Exit(Environment.ExitCode);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (lbUsers.SelectedItem == null)
            {
                MessageBox.Show("You need to choose a player");
                return;
            }
            int answer = -1;
            try
            {
                answer = Client.SendGameRequest(UserName, lbUsers.SelectedItem.ToString());
                if (answer == -1)
                {
                    MessageBox.Show($"{lbUsers.SelectedItem} refuse your offer, You need to choose another player");
                    return;
                }

                startGame(lbUsers.SelectedItem.ToString(), answer, true);

            }
            catch (FaultException<UserNoLongerConnectedFault> ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        GameBoard newGameWindow = null;

        /// <summary>
        /// start game for both sides
        /// </summary>
        /// <param name="fromUser"></param>
        /// <param name="numGame"></param>
        /// <param name="turn"></param>
        public void startGame(string fromUser, int numGame, bool turn)
        {
            newGameWindow = new GameBoard(UserName, HashedPassword, Client, Callback, turn, fromUser, numGame);

            newGameWindow.Show();

            foreach (Window win in this.OwnedWindows)
                win.Close();
            this.Closing -= Window_Closing;
            Client.Disconnect(UserName);
            this.Close();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Client.Disconnect(UserName);
            }
            catch (Exception) { }
            if (newGameWindow == null) Environment.Exit(Environment.ExitCode);
        }


        /// <summary>
        /// open search window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow srchWindow = new SearchWindow(UserName, Callback, Client);

            srchWindow.Main = this;
            srchWindow.Owner = this;
            srchWindow.Show();
        }

        /// <summary>
        /// show which game user play if you press the Label Games
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GamesDisplayWindow gamesWindow = new GamesDisplayWindow(((lbUsers.SelectedItem != null) ? lbUsers.SelectedItem.ToString() : UserName),null);
            gamesWindow.Client = Client;
            gamesWindow.Owner = this;
            gamesWindow.Show();
        }

        /// <summary>
        /// empty the user that chosen and disply the main user info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            lbUsers.SelectedItem = null;
            ((Button)sender).IsEnabled = false;

            UpdateDataWindow(Client.GetDataAllUsers(UserName).Item2);
        }

        /// <summary>
        /// if selected 1 : change games data according to user you choose. allow send game request.
        ///             2 : open window that represent the games between the players
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbUsers.SelectedItems.Count == 1)
            {
                clearBtn.IsEnabled = true;
                var nameSearch = lbUsers.SelectedItem.ToString();
                UpdateDataWindow(Client.GetDataAllUsers(nameSearch).Item2);
            }
            else {
                if(lbUsers.SelectedItems.Count == 2) {
                GamesDisplayWindow gamesWindow = new GamesDisplayWindow(lbUsers.SelectedItems[0].ToString(), lbUsers.SelectedItems[1].ToString());
                gamesWindow.Client = Client;
                gamesWindow.Owner = this;
                gamesWindow.Show();

                lbUsers.SelectedIndex = -1;

                    lbUsers.SelectedItem = null;
                    clearBtn.IsEnabled = false;
                    UpdateDataWindow(Client.GetDataAllUsers(UserName).Item2);
                }
            }
        }
    }
}
