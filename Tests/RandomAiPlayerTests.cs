using Game;
using Xunit;

namespace Tests;

public sealed class RandomAiPlayerTests
{
    [Fact]
    public void GetMove_ReturnsOnlyLegalMove()
    {
        var legalMoves = new HashSet<(int x, int y)>
        {
            (0, 5),
            (1, 5),
            (2, 5)
        };
        var player = new RandomAiPlayer(EGameCellState.Player1, new Random(42));

        var move = player.GetMove(legalMoves);

        Assert.Contains(move, legalMoves);
    }

    [Fact]
    public void GetMove_IsDeterministicForSameSeed()
    {
        var legalMoves = new HashSet<(int x, int y)>
        {
            (0, 5),
            (1, 5),
            (2, 5)
        };
        var first = new RandomAiPlayer(EGameCellState.Player1, new Random(42));
        var second = new RandomAiPlayer(EGameCellState.Player1, new Random(42));

        Assert.Equal(first.GetMove(legalMoves), second.GetMove(legalMoves));
    }

    [Fact]
    public void TwoRandomAiPlayers_ReachTerminalState()
    {
        var brain = new GameBrain(7, 6, 4);
        var players = new IPlayer[]
        {
            new RandomAiPlayer(EGameCellState.Player1, new Random(1)),
            new RandomAiPlayer(EGameCellState.Player2, new Random(2))
        };
        var playerIndex = 0;
        var terminal = false;

        while (!terminal)
        {
            var player = players[playerIndex];
            var move = player.GetMove(brain.GetEmptyCellCoordinates());
            brain.MakeMove(move.x, move.y, player.PlayerSymbol);
            terminal = brain.IsWinningMove(move.x, move.y) || brain.IsBoardFull();
            if (!terminal)
            {
                brain.SwitchPlayer();
                playerIndex = (playerIndex + 1) % players.Length;
            }
        }

        Assert.True(brain.IsBoardFull() || brain.GetState().MoveHistory.Any(
            move => brain.IsWinningMove(move.X, move.Y)));
    }
}
