using Game;
using Xunit;

namespace Tests;

public sealed class RandomAiPlayerTests
{
    public static IEnumerable<object[]> Presets =>
        GameConfiguration.Presets.Select(preset => new object[] { preset });

    public static IEnumerable<object[]> PresetsAndSeeds()
    {
        foreach (var preset in GameConfiguration.Presets)
        {
            for (var seed = 0; seed < 25; seed++)
            {
                yield return new object[] { preset, seed };
            }
        }
    }

    [Theory]
    [MemberData(nameof(PresetsAndSeeds))]
    public void GetMove_ReturnsOnlyLegalMove_ForEveryPresetAndSeed(
        GameConfiguration configuration,
        int seed)
    {
        var brain = new GameBrain(configuration);
        var player = new RandomAiPlayer(EGameCellState.Player1, new Random(seed));
        var legalMoves = brain.GetEmptyCellCoordinates();

        var move = player.GetMove(brain);

        Assert.Contains(move, legalMoves);
    }

    [Fact]
    public void GetMove_IsDeterministicForSameSeed()
    {
        var brain = new GameBrain(7, 6, 4);
        var first = new RandomAiPlayer(EGameCellState.Player1, new Random(42));
        var second = new RandomAiPlayer(EGameCellState.Player1, new Random(42));

        Assert.Equal(first.GetMove(brain), second.GetMove(brain));
    }

    [Theory]
    [MemberData(nameof(Presets))]
    public void TwoRandomAiPlayers_ReachTerminalState_UnderMoveCap(GameConfiguration configuration)
    {
        var brain = new GameBrain(configuration);
        var players = new IMoveProvider[]
        {
            new RandomAiPlayer(EGameCellState.Player1, new Random(1)),
            new RandomAiPlayer(EGameCellState.Player2, new Random(2))
        };
        var playerIndex = 0;
        var maxMoves = configuration.BoardWidth * configuration.BoardHeight + 1;
        var terminal = false;

        for (var moveNumber = 0; moveNumber < maxMoves; moveNumber++)
        {
            var player = players[playerIndex];
            var move = player.GetMove(brain);
            brain.MakeMove(move.X, move.Y, player.PlayerSymbol);

            if (brain.IsWinningMove(move.X, move.Y) || brain.IsBoardFull())
            {
                terminal = true;
                break;
            }

            brain.SwitchPlayer();
            playerIndex = (playerIndex + 1) % players.Length;
        }

        Assert.True(terminal, "AI vs AI did not reach a terminal state within the move cap.");
    }

    [Fact]
    public void GetMove_ThrowsWhenCancellationRequested()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var player = new RandomAiPlayer(EGameCellState.Player1, new Random(1));

        Assert.Throws<OperationCanceledException>(
            () => player.GetMove(new GameBrain(), cancellation.Token));
    }
}
