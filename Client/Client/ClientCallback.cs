using Client.CFGameServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ClientCallback : IFourRowServiceCallback
    {
        internal Action<string,int,bool> OpenNewGame;
        internal Action<string[]> UpdateConnectedClients;
        internal Action<string, int> OpponentMove;

        public bool Ping()
        {
            return true;
        }

        public void OtherPlayerMoved(string moveResult, int column)
        {
            OpponentMove(moveResult,column);
        }

        public void UpdateClients(string[] users)
        {
            UpdateConnectedClients?.Invoke(users);
        }

        public void SendGameNumber(string fromUser,int gameNum)
        {
            OpenNewGame(fromUser, gameNum,false);
        }

        public bool SendRequestToClient(string fromClient)
        {
            return MessageBox.Show(fromClient + " wants to play with you", "Question",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
