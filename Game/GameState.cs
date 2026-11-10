using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game;

public class GameState : BaseEntity
{
    public EGameCellState[][] Board { get; set; } = Array.Empty<EGameCellState[]>();
    public int WinningLength { get; set; } = 4;
    public EGameCellState CurrentPlayer { get; set; } = EGameCellState.Player1;
    public GameConfiguration Configuration { get; set; } = default!;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public List<GameMove> MoveHistory { get; set; } = [];

    // EF Core persists the board through this serialized backing property.
    [JsonIgnore]
    public string BoardJson
    {
        get => JsonSerializer.Serialize(Board, GameJson.Options);
        set => Board = string.IsNullOrEmpty(value)
            ? Array.Empty<EGameCellState[]>()
            : JsonSerializer.Deserialize<EGameCellState[][]>(value, GameJson.Options) ?? Array.Empty<EGameCellState[]>();
    }

    [JsonIgnore]
    public string MoveHistoryJson
    {
        get => JsonSerializer.Serialize(MoveHistory, GameJson.Options);
        set => MoveHistory = string.IsNullOrEmpty(value)
            ? []
            : JsonSerializer.Deserialize<List<GameMove>>(value, GameJson.Options) ?? [];
    }

    public GameState() : this(7, 6, 4) { }

    public GameState(int width = 7, int height = 6, int winningLength = 4)
    {
        Board = new EGameCellState[width][];
        for (int i = 0; i < width; i++)
        {
            Board[i] = new EGameCellState[height];
        }
        WinningLength = winningLength;
    }

    public (int width, int height) GetBoardSize()
    {
        return (Board.Length, Board.Length > 0 ? Board[0].Length : 0);
    }

    public EGameCellState[][] GetBoard() => Board;

    public void MakeMove(int x, int y, EGameCellState player) => Board[x][y] = player;

    public EGameCellState GetCellState(int x, int y) => Board[x][y];

    public int GetWinningLength() => WinningLength;

    public bool IsBoardFull()
    {
        for (int x = 0; x < Board.Length; x++)
        {
            for (int y = 0; y < Board[x].Length; y++)
            {
                if (Board[x][y] == EGameCellState.Empty) return false;
            }
        }
        return true;
    }

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