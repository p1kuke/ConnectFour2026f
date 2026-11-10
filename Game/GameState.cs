namespace Game
{
    public class GameState : BaseEntity
    {
        public EGameCellState[][] Board { get; set; }
        public int WinningLength { get; set; }
        public EGameCellState CurrentPlayer { get; set; } = EGameCellState.Player1;
        public DateTime SavedAt { get; set; }

        public GameState()
        {
            Board = Array.Empty<EGameCellState[]>();
        }

        public GameState(int width = 7, int height = 6, int winningLength = 4)
        {
            Board = new EGameCellState[width][];
            for (int i = 0; i < width; i++)
            {
                Board[i] = new EGameCellState[height];
            }
            WinningLength = winningLength;
        }

        public int GetWinningLength() => WinningLength;

        public void MakeMove(int x, int y, EGameCellState player)
        {
            Board[x][y] = player;
        }

        public EGameCellState GetCellState(int x, int y)
        {
            return Board[x][y];
        }

        public EGameCellState[][] GetBoard()
        {
            var result = new EGameCellState[Board.Length][];
            for (int i = 0; i < Board.Length; i++)
            {
                result[i] = new EGameCellState[Board[i].Length];
                Array.Copy(Board[i], result[i], Board[i].Length);
            }
            return result;
        }

        public (int width, int height) GetBoardSize()
        {
            return (Board.Length, Board[0].Length);
        }

        public bool IsBoardFull()
        {
            for (int x = 0; x < Board.Length; x++)
            {
                for (int y = 0; y < Board[x].Length; y++)
                {
                    if (Board[x][y] == EGameCellState.Empty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    
        // ConnectFour: returns only the cells where a piece can actually land
        // (the lowest empty cell of each column)
        public HashSet<(int x, int y)> GetEmptyCellCoordinates()
        {
            var allowedMoves = new HashSet<(int x, int y)>();
        
            for (int x = 0; x < Board.Length; x++)
            {
                for (int y = Board[x].Length - 1; y >= 0; y--)
                {
                    if (Board[x][y] == EGameCellState.Empty)
                    {
                        allowedMoves.Add((x, y));
                        break;
                    }
                }
            }
            return allowedMoves;
        }
    }
}