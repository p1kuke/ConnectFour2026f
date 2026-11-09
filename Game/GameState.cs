namespace Game;

public class GameState
{
    // jagged array. array of game board columns
    // every column is array of game board cells
    // _board[x][y]: x - column, y - row (y = 0 is the top row, y = height - 1 is the bottom row)
    private readonly EGameCellState[][] _board;
    private readonly int _winningLength;

    public GameState(int width = 7, int height = 6, int winningLength = 4)
    {
        _board = new EGameCellState[width][];
        for (int i = 0; i < width; i++)
        {
            _board[i] = new EGameCellState[height];
        }
        _winningLength = winningLength;
    }

    public int GetWinningLength() => _winningLength;

    public void MakeMove(int x, int y, EGameCellState player)
    {
        _board[x][y] = player;
    }

    public EGameCellState GetCellState(int x, int y)
    {
        return _board[x][y];
    }

    public EGameCellState[][] GetBoard()
    {
        var result = new EGameCellState[_board.Length][];
        for (int i = 0; i < _board.Length; i++)
        {
            result[i] = new EGameCellState[_board[i].Length];
            Array.Copy(_board[i], result[i], _board[i].Length);
        }
        return result;
    }

    public (int width, int height) GetBoardSize()
    {
        return (_board.Length, _board[0].Length);
    }

    public bool IsBoardFull()
    {
        for (int x = 0; x < _board.Length; x++)
        {
            for (int y = 0; y < _board[x].Length; y++)
            {
                if (_board[x][y] == EGameCellState.Empty)
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
        for (int x = 0; x < _board.Length; x++)
        {
            // search from the bottom row up
            for (int y = _board[x].Length - 1; y >= 0; y--)
            {
                if (_board[x][y] == EGameCellState.Empty)
                {
                    allowedMoves.Add((x, y));
                    break;
                }
            }
        }
        return allowedMoves;
    }
}