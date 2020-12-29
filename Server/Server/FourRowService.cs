using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FourRowService : IFourRowService
    {
        /// <summary>
        /// activeClients     - list of availables players
        /// playingClients    - list of non avialable players (Callback, gameId)
        /// activeGamesBoards - list of running games         (nameOne,nameTwo)
        /// </summary>
        Dictionary<string, IFourRowServiceCallback> activeClients = new Dictionary<string, IFourRowServiceCallback>();
        Dictionary<string, (IFourRowServiceCallback,int)> playingClients = new Dictionary<string,(IFourRowServiceCallback,int)>();
        Dictionary<int, (string,string)> activeGamesBoards = new Dictionary<int, (string, string)>();

        DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// the constructor delete unfinished game from database.
        /// </summary>
        public FourRowService()
        {
            timer.Interval = TimeSpan.FromSeconds(30);

            IEnumerable<Game> games;
                using (var ctx = new CFDatabaseContext())
                {
                    games = (from game in ctx.Games
                             where game.EndTime == null
                             select game).ToArray();
                }

                foreach (Game game in games)
                    DeleteGameFromDB(game.GameId);
        }

        /// <summary>
        /// connect or register user to database
        /// </summary>
        /// <param name="userName">which username want to login</param>
        /// <param name="pass">his hashed password</param>
        /// <param name="register"> the user want to register or login?true if register.otherwise, false</param>
        public void Connect(string userName, string pass, bool register)
        {
            using (var ctx = new CFDatabaseContext())
            {
                if (register)
                    RegisterUser(userName, pass, ctx);
                else
                    ConnectUser(userName, pass, ctx);
            }

            Thread updateThread = new Thread(() => SendConnectedClients(new string[] { userName }));
            updateThread.Start();
        }

        /// <summary>
        /// register user to database and connect the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pass"></param>
        /// <param name="ctx">instance of database context</param>
        private void RegisterUser(string userName, string pass, CFDatabaseContext ctx)
        {
            var result = from u in ctx.Users
                         where u.UserName.Equals(userName)
                         select u;
            if (result.Count() > 0)
                throw new FaultException<UserExistsFault>(new UserExistsFault { Message = "User in database.Please choose another username" },
                    new FaultReason("User in database.Please choose another username"));

            User newUser = new User()
            {
                UserName = userName,
                HashedPassword = pass,
                GameDraws = 0,
                GameLoses = 0,
                GameWins = 0,
                Score = 0
            };

            ctx.Users.Add(newUser);
            ctx.SaveChanges();
            ConnectUser(userName, pass, ctx);
        }

        /// <summary>
        ///  disconnect user from service
        /// </summary>
        /// <param name="userName"></param>
        public void Disconnect(string userName)
        {
                activeClients.Remove(userName);
                          
                Thread updateThread = new Thread(() => SendConnectedClients(new string[] { userName }));
                updateThread.Start();
        }

        /// <summary>
        /// Connect user to the service
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pass"></param>
        /// <param name="ctx">instance of database context</param>
        private void ConnectUser(string userName, string pass, CFDatabaseContext ctx)
        {
            if (activeClients.ContainsKey(userName) || playingClients.ContainsKey(userName))
                throw new FaultException<UserExistsFault>(new UserExistsFault { Message = "User already log in" },
                        new FaultReason("User already log in"));

            var result = from u in ctx.Users
                         where u.UserName.Equals(userName)
                         select u;
            if (result.Count() == 0)
                throw new FaultException<UserNotExistsFault>(new UserNotExistsFault { Message = "User not in database.Please Register" },
                    new FaultReason("User not in database.Please Register"));
            if (result.First().HashedPassword != pass)
                throw new FaultException<WrongPasswordFault>(new WrongPasswordFault { Message = "Wrong Password." },
                    new FaultReason("Wrong Password."));

            IFourRowServiceCallback callback = OperationContext.Current.GetCallbackChannel<IFourRowServiceCallback>();
            activeClients.Add(userName, callback);
        }
        public IEnumerable<string> GetActivePlayres()
        {
            return activeClients.Keys;
        }

        /// <summary>
        /// For Playing users.this how server know player alive
        /// </summary>
        /// <returns></returns>
        public bool PingServer()
        {
            return true;
        }

        /// <summary>
        /// deletes specifiec game from the database.also for the users.
        /// </summary>
        /// <param name="game">which game you want to delete</param>
        private void DeleteGameFromDB(int gameId)
        {
            using (var ctx = new CFDatabaseContext())
            {

                ctx.Games.Include(_ => _.Users);
                ctx.Users.Include(_ => _.Games);

                Game game = (from g in ctx.Games
                        where gameId == g.GameId
                        select g).First();
                if (game != null) {
                    //deleting game records
                    var grs = ctx.GameRecords.Where(_ => _.GameId == gameId).ToList();
                    if (grs.Count() > 0)
                    {
                        foreach (var gr in grs)
                        {
                            ctx.GameRecords.Remove(gr);
                        }
                        ctx.SaveChanges();
                    }

                    var players = (from user in ctx.Users
                               where user.UserName.Equals(game.LoserName) ||
                                     user.UserName.Equals(game.WinnerName)
                               select user).ToArray();

                //update users and delete game
                foreach (User player in players)
                {
                    game.Users.Remove(player);
                    player.Games.Remove(game);
                    ctx.Users.AddOrUpdate(player);
                }
                ctx.Games.AddOrUpdate(game);
                ctx.SaveChanges();
                ctx.Games.Remove(game);
                ctx.SaveChanges();
                }
            }
        }

        /// <summary>
        /// get user or all users data - Personal Information: num of winning, lossses
        /// </summary>
        /// <param name="name">if you want specific user.If not,null</param>
        /// <returns>
        /// string - name (key)
        /// Item1 - GameLoses
        /// Item2 - GameWins
        /// Item3 - GameDraws
        /// Item4 - Score
        /// Item5 - Games.Count
        /// 
        /// all ints
        /// </returns>
        public (string[],Dictionary<string, (int, int, int, int, int)>) GetDataAllUsers(string name)
        {
            Dictionary<string, (int, int, int, int, int)> data = new Dictionary<string, (int, int, int, int, int)>();
            using (var ctx = new CFDatabaseContext())
            {
                IEnumerable<User> usrs;
                if (name == null)
                    usrs = (from u in ctx.Users
                            select u).ToArray();
                else
                    usrs = (from u in ctx.Users
                            where u.UserName.Equals(name)
                            select u).ToArray();

                foreach (var usr in usrs)
                {
                    data.Add(usr.UserName,
                        (usr.GameLoses
                        , usr.GameWins
                        , usr.GameDraws
                        , usr.Score
                        , usr.Games.Count));
                }
            }

            return (GetActivePlayres().ToArray(),data);
        }
        
        /// <summary>
        /// add game record to database.
        /// </summary>
        /// <param name="gameNumber"></param>
        /// <param name="userName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public void AddRecord(int gameNumber, string userName, int column)
        {
            using (var ctx = new CFDatabaseContext())
            {
               Game game = (from g in ctx.Games
                             where g.GameId == gameNumber
                             select g).First();
                User user = (from g in ctx.Users
                             where g.UserName.Equals(userName)
                             select g).First();
                GameRecord newRecord = new GameRecord
                {
                    Column = column,
                    Game = game,
                    User = user,
                    RecordTime = DateTime.Now
                };

                game.GameRecords.Add(newRecord);
                ctx.Games.AddOrUpdate(game);
                ctx.SaveChanges();

                var userTwo = activeGamesBoards[gameNumber].Item1.Equals(userName) ? activeGamesBoards[gameNumber].Item2 : activeGamesBoards[gameNumber].Item1;
                playingClients[userTwo].Item1.OtherPlayerMoved(MoveResult.GameOn.ToString(), column);
            }
        }

        /// <summary>
        /// first, check if the client is online, if is online make a new game and add the game to database for see the available game
        /// </summary>
        /// <param name="fromClient"></param>
        /// <param name="toClient"></param>
        /// <returns></returns>
        public int SendGameRequest(string fromClient, string toClient)
        {
            try { 
            if (activeClients[toClient].SendRequestToClient(fromClient))
            {
                using (var ctx = new CFDatabaseContext())
                {
                    Game newGame = new Game();
                    newGame.WinnerScore = 0;
                    newGame.LoserScore = 0;

                    newGame.WinnerName = fromClient;
                    newGame.LoserName = toClient;
                    newGame.StartTime = DateTime.Now;
                    User userOne = (from u in ctx.Users
                                    where u.UserName.Equals(fromClient)
                                    select u).FirstOrDefault();
                    newGame.Users.Add(userOne);
                    User userTwo = (from u in ctx.Users
                                    where u.UserName.Equals(toClient)
                                    select u).FirstOrDefault();
                    newGame.Users.Add(userTwo);
                    ctx.Games.Add(newGame);
                    ctx.SaveChanges();

                    newGame = (from g in ctx.Games
                               where g.EndTime == null &&
                               g.WinnerName.Equals(fromClient) &&
                                g.LoserName.Equals(toClient)
                               select g).FirstOrDefault();

                    userOne.Games.Add(newGame);
                    userTwo.Games.Add(newGame);
                    ctx.Users.AddOrUpdate(userOne);
                    ctx.Users.AddOrUpdate(userTwo);
                    ctx.SaveChanges();

                    activeGamesBoards.Add(newGame.GameId, (userOne.UserName,userTwo.UserName));

                    activeClients[toClient].SendGameNumber(fromClient, newGame.GameId);
                    playingClients.Add(fromClient, (activeClients[fromClient], newGame.GameId));
                    activeClients.Remove(fromClient);
                    playingClients.Add(toClient, (activeClients[toClient], newGame.GameId));
                    activeClients.Remove(toClient);

                    Thread updateThread = new Thread(() => SendConnectedClients(new string[] { fromClient, toClient }));
                    updateThread.Start();
                    if (activeGamesBoards.Count == 1)
                    {
                        timer.Tick += PingPlayingClients;
                        timer.Start();
                    }

                    return newGame.GameId;
                }
            }
            }
            catch (Exception)
            {
                Disconnect(toClient);
                throw new FaultException<UserNoLongerConnectedFault>(new UserNoLongerConnectedFault { Message = "User No Longer Connected.Please choose another player" },
                    new FaultReason("User No Longer Connected.Please choose another player"));

            }
            return -1;
        }
        public Dictionary<int, (DateTime, DateTime?, string, int, string, int, bool?)> GetAllPlayedGames(string nameOne, string nameTwo)
        {
            Dictionary<int, (DateTime, DateTime?, string, int, string, int, bool?)> data = new Dictionary<int, (DateTime, DateTime?, string, int, string, int, bool?)>();
            using (var ctx = new CFDatabaseContext())
            {
                IEnumerable<Game> games;
                if (nameOne != null && nameTwo != null)
                    games = (from g in ctx.Games
                             where (g.LoserName.Equals(nameOne) && g.WinnerName.Equals(nameTwo)) ||
                                   (g.LoserName.Equals(nameTwo) && g.WinnerName.Equals(nameOne)) &&
                                   g.EndTime != null
                             select g).ToArray();
                else if (nameOne == null && nameTwo == null)
                    games = (from g in ctx.Games
                             where g.EndTime != null
                             select g).ToArray();
                else
                    games = (from g in ctx.Games
                             where (g.LoserName.Equals(nameOne) || g.WinnerName.Equals(nameOne)) &&
                                   g.EndTime != null
                             select g).ToArray();
                foreach (var game in games)
                {
                    data.Add(game.GameId,
                        (game.StartTime
                        , game.EndTime
                        , game.WinnerName
                        , game.WinnerScore
                        , game.LoserName
                        , game.LoserScore
                        , game.IsDraw));
                }
            }
            return data;
        }

        public Dictionary<int, (DateTime, string, string)> GetRunningGames()
        {
            Dictionary<int, (DateTime, string, string)> data = new Dictionary<int, (DateTime, string, string)>();
            using (var ctx = new CFDatabaseContext())
            {
                IEnumerable<Game> games = null;
                games = (from g in ctx.Games
                         where g.EndTime == null
                         select g).ToArray();
                foreach (var game in games)
                {
                    data.Add(game.GameId,
                        (game.StartTime
                        , game.WinnerName
                        , game.LoserName));
                }
            }

            return data;
        }

        /// <summary>
        /// for client to know MoveResult and GameRecord
        /// </summary>
        /// <param name="t"></param>
        /// <param name="tx"></param>
        public void NOTUSE(MoveResult t, GameRecord tx)
        {
            return;
        }

        /// <summary>
        /// The server sends users the list of connected and avoid deadlock by user who disconnect or disconnected for play .
        /// </summary>
        /// <param name="users"></param>
        private void SendConnectedClients(string[] users)
        {
            List<string> removes = new List<string>();
            foreach (var client in activeClients)
            {
                try
                {
                    if (users.Contains(client.Key) == false)
                        client.Value.UpdateClients(activeClients.Keys.ToArray());
                }
                catch
                {
                    removes.Add(client.Key);
                }
            }
            if (removes.Count > 0)
                foreach (var remove in removes)
                {
                    activeClients.Remove(remove);
                }
        }

        private void PingPlayingClients(object sender, EventArgs e)
        {
            if (activeGamesBoards.Count == 0)
            {
                timer.Tick -= PingPlayingClients;
                timer.Stop();
                return;
            }

            var actives = activeGamesBoards.Keys.ToList();
            foreach (var gameId in actives)
            {
                Thread updateThread = new Thread(() => CheckUsers(gameId));
                updateThread.Start();
            }

        }

        /// <summary>
        /// checking if the 2 players are connection if not , the game will delete from DB
        /// </summary>
        /// <param name="gameId"></param>
        private void CheckUsers(int gameId)
        {
            try
            {
                playingClients[activeGamesBoards[gameId].Item1].Item1.Ping();
            }
            catch (Exception)
            {
                try
                {
                    playingClients[activeGamesBoards[gameId].Item2].Item1.Ping();
                }
                catch (Exception)
                {
                    DeleteGameFromDB(gameId);
                    Disconnect(activeGamesBoards[gameId].Item1);
                    Disconnect(activeGamesBoards[gameId].Item2);
                    activeGamesBoards.Remove(gameId);
                    return;
                }
                playingClients[activeGamesBoards[gameId].Item2].Item1.OtherPlayerMoved(MoveResult.OtherUserDisconnected.ToString(), -1);
                return;
            }

            try
            {
                playingClients[activeGamesBoards[gameId].Item2].Item1.Ping();
            }
            catch (Exception)
            {
                try
                {
                    playingClients[activeGamesBoards[gameId].Item1].Item1.Ping();
                }
                catch (Exception)
                {
                    DeleteGameFromDB(gameId);
                    Disconnect(activeGamesBoards[gameId].Item1);
                    Disconnect(activeGamesBoards[gameId].Item2);
                    activeGamesBoards.Remove(gameId);
                    return;
                }
                playingClients[activeGamesBoards[gameId].Item1].Item1.OtherPlayerMoved(MoveResult.OtherUserDisconnected.ToString(), -1);
                return;
            }
        }

        /// <summary>
        /// update users amd games. remove active game and send the opponent if it unfinished game
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="winnerName"></param>
        /// <param name="losserName"></param>
        /// <param name="draw"></param>
        /// <param name="winnerScore"></param>
        /// <param name="losserScore"></param>
        /// <param name="isUnFinishedGame"></param>
        public void FinishGame(int gameId, string winnerName, string losserName, bool draw, int winnerScore, int losserScore, bool isUnFinishedGame)
        {
            timer.Tick -= PingPlayingClients;
            Game game;
            using (var ctx = new CFDatabaseContext())
            {
                game = (from g in ctx.Games
                        where gameId == g.GameId
                        select g).First();
                game.EndTime = DateTime.Now;
                game.IsDraw = draw;
                game.LoserName = losserName;
                game.WinnerName = winnerName;

                User losser = (from u in ctx.Users
                               where u.UserName.Equals(losserName)
                               select u).First();
                User winner = (from u in ctx.Users
                               where u.UserName.Equals(winnerName)
                               select u).First();

                game.LoserScore = losserScore;
                losser.Score += game.LoserScore;

                game.WinnerScore = winnerScore;
                winner.Score += game.WinnerScore;

                if (draw)
                {
                    winner.GameDraws++; losser.GameDraws++;

                }
                else
                {
                    winner.GameWins++; losser.GameLoses++;
                }

                ctx.Games.AddOrUpdate(game);
                ctx.Users.AddOrUpdate(winner);
                ctx.Users.AddOrUpdate(losser);
                ctx.SaveChanges();

            }
            if (isUnFinishedGame) playingClients[winnerName].Item1.OtherPlayerMoved(MoveResult.Winning.ToString(), -1);
            if (playingClients.ContainsKey(winnerName)) playingClients.Remove(winnerName);
            if (playingClients.ContainsKey(losserName)) playingClients.Remove(losserName);

            activeGamesBoards.Remove(game.GameId);
            if (activeGamesBoards.Count == 0)
            {
                timer.Tick -= PingPlayingClients;
                timer.Stop();
            }
        }
    }
}
