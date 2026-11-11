namespace Game;

public class GameBrain
{
    private readonly int _width;
    private readonly int _height;
    private readonly GameState _gameState;
    private readonly bool _isCylinder;

    // Current player property, delegating the request to GameState
    public EGameCellState CurrentPlayer => _gameState.CurrentPlayer;


    // Constructor by default / by dimensions
    public GameBrain(int width = 7, int height = 6, int winningLength = 4)
    {
        _width = width;
        _height = height;
        _gameState = new GameState(width, height, winningLength);
        _isCylinder = false;
    }

    // Constructor for loading from a configuration
    public GameBrain(GameConfiguration config)
        : this(config.BoardWidth, config.BoardHeight, config.WinCondition)
    {
        config.Validate();
        _gameState.Configuration = config;
        _isCylinder = config.IsCylinder;
    }

    // Constructor for loading from a saved game state
    public GameBrain(GameState gameState)
    {
        _gameState = gameState;
        var (width, height) = gameState.GetBoardSize();
        _width = width;
        _height = height;
        _isCylinder = gameState.Configuration?.IsCylinder == true;
    }

    // Method for returning the current state for the repository
    public GameState GetState() => _gameState;

    // Switching the current player
    public void SwitchPlayer()
    {
        _gameState.CurrentPlayer = _gameState.CurrentPlayer == EGameCellState.Player1
            ? EGameCellState.Player2
            : EGameCellState.Player1;
    }

    public EGameCellState[][] GetBoard() =>
        _gameState.GetBoard();

    public void MakeMove(int x, int y, EGameCellState player)
    {
        if (player != _gameState.CurrentPlayer)
        {
            throw new InvalidOperationException(
                $"Invalid move: it is {_gameState.CurrentPlayer}'s turn, not {player}'s.");
        }

        // validate move: only the lowest empty cell of a column is allowed
        if (!GetEmptyCellCoordinates().Contains((x, y)))
        {
            throw new InvalidOperationException($"Invalid move: ({x}, {y})");
        }

        _gameState.MakeMove(x, y, player);
        _gameState.MoveHistory.Add(new GameMove(x, y, player));
    }

    public HashSet<(int x, int y)> GetEmptyCellCoordinates() =>
        _gameState.GetEmptyCellCoordinates();

    public bool IsBoardFull() =>
        _gameState.IsBoardFull();

    public bool IsWinningMove(int x, int y)
    {
        var moveBy = _gameState.GetCellState(x, y);
        if (moveBy == EGameCellState.Empty) return false;

        var winningLength = _gameState.GetWinningLength();

        // horizontal, vertical, diagonal \, diagonal /
        var directions = new (int dx, int dy)[] { (1, 0), (0, 1), (1, 1), (1, -1) };

        foreach (var (dx, dy) in directions)
        {
            if (CountLine(x, y, dx, dy, moveBy) >= winningLength)
            {
                return true;
            }
        }

        return false;
    }

    // Counts distinct consecutive cells through (x, y) in both directions. The visited set
    // prevents double counting when a cylinder line wraps around the board edge.
    private int CountLine(int x, int y, int dx, int dy, EGameCellState player)
    {
        var visited = new HashSet<(int X, int Y)> { (x, y) };
        AddDirection(x, y, dx, dy, player, visited);
        AddDirection(x, y, -dx, -dy, player, visited);
        return visited.Count;
    }

    private void AddDirection(
        int x,
        int y,
        int dx,
        int dy,
        EGameCellState player,
        HashSet<(int X, int Y)> visited)
    {
        var curX = x;
        var curY = y;

        while (true)
        {
            curX += dx;
            curY += dy;

            if (curY < 0 || curY >= _height)
            {
                return;
            }

            if (_isCylinder && dx != 0)
            {
                curX = (curX + _width) % _width;
            }
            else if (curX < 0 || curX >= _width)
            {
                return;
            }

            var cell = (X: curX, Y: curY);
            if (!visited.Add(cell))
            {
                return;
            }

            if (_gameState.GetCellState(curX, curY) != player)
            {
                visited.Remove(cell);
                return;
            }
        }
    }
}