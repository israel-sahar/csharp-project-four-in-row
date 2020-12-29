using Client.CFGameServiceReference;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Client
{
    public partial class GameBoard : Window
    {
        public string UserName { get; internal set; }
        public string Password { get; internal set; }

        public FourRowServiceClient Client { get; internal set; }
        public ClientCallback Callback { get; internal set; }
        public string OpponentName { get; internal set; }
        public bool Myturn { get; internal set; }
        public int GameNum { get; internal set; }
        private GameChecker LiveGame { get; set; }

        //colors
        private SolidColorBrush myColor;
        private SolidColorBrush opponentColor;
        private SolidColorBrush backgroundColor = new SolidColorBrush(Colors.Aquamarine);


        //board properties
        private static readonly int ROW = 6;
        private static readonly int COLUMN = 7;

        //boards
        private Ellipse[,] liveBoard= new Ellipse[ROW, COLUMN];
        private Ellipse[,] fullBoard = new Ellipse[ROW, COLUMN];

        //userHelperGUI
        private Image[] arrows  = new Image[COLUMN];
        private Ellipse[] balls = new Ellipse[COLUMN];

        //timers
        DispatcherTimer animationTimer;

        //useful vars
        private string res = MoveResult.GameOn.ToString();
        private int reqCol, reqRow;
        private bool StartAnimation = true;

        public GameBoard(string userName, string pass, FourRowServiceClient client, ClientCallback callback, bool turn, string fromUser, int numGame)
        {
            InitializeComponent();
            this.Password = pass;            this.UserName = userName;
            this.Client = client;            this.Callback = callback;
            this.OpponentName = fromUser;    this.Myturn = turn;
            this.GameNum = numGame;

            this.Title += $" - UserName:{UserName}";
            BeforeGame();

            for (int i = 0; i < ROW; i++)
                for (int j = 0; j < COLUMN; j++)
                    liveBoard[i,j] = null;

            animationTimer = new DispatcherTimer();
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            animationTimer.Start();

            LiveGame = new GameChecker();
            this.Callback.OpponentMove = React;
        }

        /// <summary>
        ///Initilaize the colors 
        /// </summary>
        public void BeforeGame()
        {
            if (!Myturn)
            {
                myColor = new SolidColorBrush(Color.FromArgb(150, (byte)255, (byte)0, (byte)0));
                opponentColor = new SolidColorBrush(Color.FromArgb(150, (byte)0, (byte)0, (byte)255));

                Turn.Text = OpponentName + "'s Turn";
                VisibleGrid.Visibility = Visibility.Hidden;
                ellipse.Fill = opponentColor;
            }
            else
            {
                myColor = new SolidColorBrush(Color.FromArgb(150, (byte)0, (byte)0, (byte)255));
                opponentColor = new SolidColorBrush(Color.FromArgb(150, (byte)255, (byte)0, (byte)0));

                Turn.Text = "This is your Turn";
                ellipse.Fill = myColor;
            }

            VisibleGrid_Initialized();

        }
        /// <summary>
        /// Initilaize the arrows and the ball that display to help the user choose a column
        /// </summary>
        private void VisibleGrid_Initialized()
        {
            for (int i = 0; i < COLUMN; i++)
            {
                //make arrows
                arrows[i] = new Image();
                arrows[i].Source = new BitmapImage(new Uri("./images/arrowDown.png", UriKind.Relative));
                arrows[i].Tag = i + 1;
                arrows[i].MouseEnter += VisibleEllipse;
                arrows[i].MouseDown += playMove;
                arrows[i].MouseLeave += InVisibleEllipse;
                arrows[i].HorizontalAlignment = HorizontalAlignment.Center;

                Grid.SetColumn(arrows[i], i);
                Grid.SetRow(arrows[i], 0);
                arrows[i].Visibility = Visibility.Visible;
                Panel.SetZIndex(arrows[i], 3);

                VisibleGrid.Children.Add(arrows[i]);

                //make ellipse
                balls[i] = new Ellipse
                {
                    Tag = i + 1,
                    Fill = backgroundColor,
                    Height = 22,
                    Width = 22
                };
                Panel.SetZIndex(balls[i], 2);
                balls[i].Visibility = Visibility.Visible;
                Grid.SetColumn(balls[i], i);
                Grid.SetRow(balls[i], 1);

                VisibleGrid.Children.Add(balls[i]);
            }
        }

        /// <summary>
        /// this method called from service after the other player played.
        /// play the animation, check the move, act according to result
        /// </summary>
        /// <param name="moveResult"></param>
        /// <param name="cln"></param>
        private void React(string moveResult, int cln)
        {
            if (cln != -1)
                makeAnimation(cln, false);
            else
            {
                MessageBox.Show($"Well Done! You defeat {OpponentName}");
                if (moveResult == MoveResult.OtherUserDisconnected.ToString()) {
                    Thread updateThread = new Thread(() => Client.FinishGame(GameNum,
                  UserName,
                  OpponentName,
                  false,
                  LiveGame.CountingScores(SYM.Me, true),
                  LiveGame.CountingScores(SYM.Opponent, false),
                  false));
                    updateThread.Start();
                }
                CloseWindowAndReturnMenu();
                return;
            }
            MoveResult result = LiveGame.verifyMove(reqRow, reqCol, SYM.Opponent);
            res = result.ToString();

            if (res == MoveResult.GameOn.ToString())
                return;

            if (res == MoveResult.Draw.ToString())
            {
                Thread updateThread = new Thread(() => Client.FinishGame(GameNum,
                              UserName,
                              OpponentName,
                              true,
                              LiveGame.CountingScores(SYM.Me, false),
                              LiveGame.CountingScores(SYM.Opponent, false),
                              false));
                updateThread.Start();

                MessageBox.Show($"This is a Draw!");

            }
            if (res == MoveResult.Winning.ToString() && cln != -1)
            {
                Thread updateThread = new Thread(() => Client.FinishGame(GameNum,
                                  OpponentName,
                                  UserName,
                                  false,
                                  LiveGame.CountingScores(SYM.Opponent, true),
                                  LiveGame.CountingScores(SYM.Me, false),
                                  false));
                updateThread.Start();
                
                MessageBox.Show($"You lost, try next time");

            }

            CloseWindowAndReturnMenu();
            return;
        }

        private void playMove(object sender, MouseButtonEventArgs e)
        {
            MakeMove((int)((Image)sender).Tag);
        }


        /// <summary>
        /// hide the ball. according to the arrow above him
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InVisibleEllipse(object sender, MouseEventArgs e)
        {
            balls[(int)((Image)sender).Tag-1].Fill = backgroundColor;
        }

        /// <summary>
        /// show the ball. according to the arrow above him
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisibleEllipse(object sender, MouseEventArgs e)
        {
            balls[(int)((Image)sender).Tag - 1].Fill = myColor;
        }

        /// <summary>
        /// occurs after player choose col. make animation,verify the move, and send the record to the service
        /// </summary>
        /// <param name="clm"></param>
        private void MakeMove(int clm)
        {
            try
            {
                makeAnimation(clm, true);
                var result = LiveGame.verifyMove(reqRow, reqCol, SYM.Me);
                res = result.ToString();
                Client.AddRecord(GameNum, UserName, clm);
                if (res == MoveResult.GameOn.ToString())
                    return;

                if (res == MoveResult.Winning.ToString())
                    MessageBox.Show($"Well Done! You defeat {OpponentName}");

                if (res == MoveResult.Draw.ToString())
                    MessageBox.Show($"This is a Draw!");

                CloseWindowAndReturnMenu();
                return;
            }
            catch (FaultException<IllegalMoveFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        private void makeAnimation(int cln, bool myMove)
        {
            reqRow = ROW - 1;
            reqCol = cln - 1;

            while (liveBoard[reqRow, reqCol] != null) reqRow--;

            Ellipse disk = new Ellipse()
            {
                Fill = myMove ? myColor : opponentColor,
                Height = 43,
                Width = 43
            };
            Panel.SetZIndex(disk, 2);
            disk.Visibility = Visibility.Visible;


            //place the disk on the first space of the wanted column
            var a = fullBoard[0, reqCol].TranslatePoint(new Point(0, 0), FourInARowBord);
            Canvas.SetTop(disk, a.Y);
            Canvas.SetLeft(disk, a.X);

            FourInARowBord.Children.Add(disk);

            liveBoard[reqRow, reqCol] = disk;
            animationTimer.Tick += DropCircleAnimation;

            //mean that the col full.disappear the arrow
            if (liveBoard[0, reqCol] != null) { 
                 arrows[reqCol].Visibility = Visibility.Hidden;
                 VisibleGrid.Children.Remove(arrows[reqCol]);
                balls[reqCol].Visibility = Visibility.Hidden;
                VisibleGrid.Children.Remove(balls[reqCol]);
            }
        }

        
        /// <summary>
        /// drop the disk to his space
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropCircleAnimation(object sender, EventArgs e)
        {
            if(StartAnimation && liveBoard[reqRow, reqCol].Fill.Equals(myColor)&& res == MoveResult.GameOn.ToString())
            {
                SwitchTurns();
                StartAnimation = false;
            }

            var toTop = fullBoard[reqRow, reqCol].TranslatePoint(new Point(0, 0), FourInARowBord).Y;

            var current = Canvas.GetTop(liveBoard[reqRow, reqCol]);
            if (current < toTop)
            {
                Canvas.SetTop(liveBoard[reqRow, reqCol], ((current + 10 > toTop) ? toTop : current + 5));
            }
            else
            {
                StartAnimation = true;
                animationTimer.Tick -= DropCircleAnimation;
                if (res == MoveResult.GameOn.ToString() && liveBoard[reqRow, reqCol].Fill.Equals(opponentColor))
                    SwitchTurns();
            }
        }

        /// <summary>
        /// Initialize unseen full board game for the animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FourInARowBord_Initialized(object sender, EventArgs e)
        {
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COLUMN; j++)
                {
                    fullBoard[i,j] = new Ellipse
                    {
                        Fill = backgroundColor,
                        Height = 43,
                        Width = 43
                    };
                    Panel.SetZIndex(fullBoard[i, j], 3);

                    Grid.SetColumn(fullBoard[i, j], j);
                    Grid.SetRow(fullBoard[i, j], i);

                    GameBoardGrid.Children.Add(fullBoard[i, j]);
                }
            }

        }

        private void SwitchTurns() {
            if (!Myturn) {
                Turn.Text = "This is your Turn";
                VisibleGrid.Visibility = Visibility.Visible;
            }
            else
            {
                VisibleGrid.Visibility = Visibility.Hidden;
                Turn.Text = OpponentName + "'s Turn";
            }

            ellipse.Fill = Myturn?opponentColor:myColor;
            Myturn = !Myturn;
        }

        /// <summary>
        /// reconnect and back to main menu
        /// </summary>
        private void CloseWindowAndReturnMenu()
        {
            Thread updateThread = new Thread(() => Client.Connect(UserName, Password, false));
            updateThread.Start();
           
            MainWindow newMain = new MainWindow(UserName,Password, Client, Callback);
            this.Close();
            newMain.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (res == MoveResult.GameOn.ToString())
            {
                if (MessageBox.Show("Are you sure you want leaving the game?Yes, for exit", "Question",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Thread updateThread = new Thread(() => Client.FinishGame(GameNum,
                        OpponentName, 
                        UserName, 
                        false,
                        LiveGame.CountingScores(SYM.Opponent, true),
                        LiveGame.CountingScores(SYM.Me, false), true));
                    updateThread.Start();
                    Environment.Exit(Environment.ExitCode);
                }
            }
            
        }
    }
}
