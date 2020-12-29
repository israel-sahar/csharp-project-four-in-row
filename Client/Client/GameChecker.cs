using Client.CFGameServiceReference;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    enum Direction { First, LeftUp, Left, LeftDown, Down, RightDown, Right, RightUp }
    enum SYM {None,Me,Opponent }
    //each player have board.

    class GameChecker
    {
        int[,] Board { get; set; }
        private readonly int BOARD_WI_SIZE = 7;
        private readonly int BOARD_HI_SIZE = 6;

        public GameChecker() {
            Board = new int[BOARD_HI_SIZE, BOARD_WI_SIZE];
            for (int i = 0; i < BOARD_HI_SIZE; i++)
                for (int j = 0; j < BOARD_WI_SIZE; j++)
                    Board[i, j] = (int)SYM.None;
        }

        private void AddMove(int reqRow, int reqCol, SYM SYM) {
            Board[reqRow, reqCol] = (int)SYM;
        }

        private int isWinning(int reqRow, int reqCol, Direction dir, SYM SYM)
        {
            try
            {
                if (Board[reqRow, reqCol] != (int)SYM) return 0;
            }
            catch (Exception) { return 0; }


            switch (dir)
            {
                case (Direction.First):
                    {
                        int LeftUp = isWinning(reqRow - 1, reqCol - 1, Direction.LeftUp, SYM);
                        int Left = isWinning(reqRow, reqCol - 1, Direction.Left, SYM);
                        int LeftDown = isWinning(reqRow + 1, reqCol - 1, Direction.LeftDown, SYM);
                        int Down = isWinning(reqRow + 1, reqCol, Direction.Down, SYM);
                        int RightDown = isWinning(reqRow + 1, reqCol + 1, Direction.RightDown, SYM);
                        int Right = isWinning(reqRow, reqCol + 1, Direction.Right, SYM);
                        int RightUp = isWinning(reqRow - 1, reqCol + 1, Direction.RightUp, SYM);

                        return (LeftUp + 1 + RightDown >= 4 ||
                                Left + 1 + Right >= 4 ||
                                LeftDown + 1 + RightUp >= 4 ||
                                Down + 1 >= 4) ? 1 : 0;
                    }
                case (Direction.LeftUp):
                    {
                        return 1 + isWinning(reqRow - 1, reqCol - 1, Direction.LeftUp, SYM);
                    }
                case (Direction.Left):
                    {
                        return 1 + isWinning(reqRow, reqCol - 1, Direction.Left, SYM);
                    }
                case (Direction.LeftDown):
                    {
                        return 1 + isWinning(reqRow + 1, reqCol - 1, Direction.LeftDown, SYM);
                    }
                case (Direction.Down):
                    {
                        return 1 + isWinning(reqRow + 1, reqCol, Direction.Down, SYM);
                    }
                case (Direction.RightDown):
                    {

                        return 1 + isWinning(reqRow + 1, reqCol + 1, Direction.RightDown, SYM);
                    }
                case (Direction.Right):
                    {
                        return 1 + isWinning(reqRow, reqCol + 1, Direction.Right, SYM);
                    }
                case (Direction.RightUp):
                    {
                        return 1 + isWinning(reqRow - 1, reqCol + 1, Direction.RightUp, SYM);
                    }
            }
            return 0;
        }

        private bool isDraw() {
            for (int i = 0; i < BOARD_WI_SIZE; i++)
            {
                if (Board[0, i] == (int)SYM.None) return false;
            }
            return true;
        }

        /// <summary>
        /// check move and return the result
        /// </summary>
        /// <param name="reqRow"></param>
        /// <param name="reqCol"></param>
        /// <param name="SYM"></param>
        /// <returns></returns>
        public MoveResult verifyMove(int reqRow, int reqCol,SYM SYM)
        {
            AddMove(reqRow, reqCol, SYM);

            //check winning
            if (isWinning(reqRow, reqCol, Direction.First, SYM) == 1)
                return MoveResult.Winning;

            //check draw
            return isDraw()?MoveResult.Draw:MoveResult.GameOn;
        }

        /// <summary>
        /// counting score
        /// </summary>
        /// <param name="game"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public int CountingScores(SYM SYM,bool isWinning)
        {
            int counter = 0;
            int[] columns = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < BOARD_HI_SIZE; i++)
            {
                for (int j = 0; j < BOARD_WI_SIZE; j++)
                {
                    if (Board[i, j] == (int)SYM) {
                        counter++;
                        columns[j] = 1;
                    }
                }
            }

            counter = isWinning ? 1000 : counter * 10;
            if (columns.Sum() == 7) counter += 100;

            return counter;
        }

    }
}
