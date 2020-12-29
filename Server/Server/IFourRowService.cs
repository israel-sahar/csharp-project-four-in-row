using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Server
{
    /*IMPORTANT - dont use the same name for service method and client method*/
    
    [ServiceContract(CallbackContract =typeof(IFourRowServiceCallback))]
    public interface IFourRowService
    {   
        [FaultContract(typeof(UserExistsFault))]
        [FaultContract(typeof(UserNotExistsFault))]
        [FaultContract(typeof(WrongPasswordFault))]
        [OperationContract]
        void Connect(string userName, string pass, bool register);
        [OperationContract]
        void Disconnect(string userName);

        [OperationContract]
        IEnumerable<string>  GetActivePlayres();

        [OperationContract]
        bool PingServer();

        [OperationContract]
        void FinishGame(int gameId, string winnerName, string losserName, bool draw, int winnerScore, int losserScore, bool isUnFinishedGame);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromClient"></param>
        /// <param name="toClient"></param>
        /// <returns>game number. or -1</returns>

        [FaultContract(typeof(UserNoLongerConnectedFault))]
        [OperationContract]
        int SendGameRequest(string fromClient, string toClient);
        
        [FaultContract(typeof(IllegalMoveFault))]
        [OperationContract]
        void AddRecord(int gameNumber, string userName, int column);

        /// <summary>
        ///  return all user data
        /// </summary>
        /// <param name="name">if you want specific user.If not,null</param>
        /// <returns>
        /// string - name (key)
        /// 
        /// Item1 - GameLoses
        /// Item2 - GameWins
        /// Item3 - GameDraws
        /// Item4 - Score
        /// Item5 - Games.Count
        /// 
        /// all ints
        /// </returns>
        [OperationContract]
        (string[], Dictionary<string, (int, int, int, int, int)>) GetDataAllUsers(string name);

        [OperationContract]
        Dictionary<int, (DateTime, DateTime?, string, int, string, int, bool?)> GetAllPlayedGames(string nameOne, string nameTwo);

        [OperationContract]
        Dictionary<int, (DateTime, string, string)> GetRunningGames();

        [OperationContract]
        void NOTUSE(MoveResult t, GameRecord tx);

    }

    public interface IFourRowServiceCallback
    {
        /// <summary>
        /// This function implement at client side.
        /// occurs when server send invite to user
        /// 
        /// </summary>
        /// <param name="fromClient"></param>
        /// <param name="toClient"></param>
        /// <returns>true if accept the invitation</returns>
        [OperationContract]
        bool SendRequestToClient(string fromClient);

        [OperationContract]
        bool Ping();
        
        [OperationContract(IsOneWay = true)]
        void SendGameNumber(string fromUser,int gameNum);

        [OperationContract(IsOneWay = true)]
        void UpdateClients(string[] users);
        [OperationContract(IsOneWay = true)]
        void OtherPlayerMoved(string moveResult, int column);
    }
}
