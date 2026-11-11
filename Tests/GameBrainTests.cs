using Game;
using Xunit;

namespace Tests;

public sealed class GameBrainTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(1, -1)]
    public void IsWinningMove_DetectsEveryDirection(int dx, int dy)
    {
        var state = new GameState(7, 6, 4);
        var startY = dy < 0 ? 3 : 0;
        for (var index = 0; index < 4; index++)
        {
            var x = 1 + index * dx;
            var y = startY + index * dy;
            state.Board[x][y] = EGameCellState.Player1;
        }

        Assert.True(new GameBrain(state).IsWinningMove(1 + 3 * dx, startY + 3 * dy));
    }

    [Fact]
    public void Cylinder_WrapsHorizontalEdge()
    {
        var configuration = new GameConfiguration("Cylinder", 7, 6, 4, true);
        var state = new GameState(7, 6, 4) { Configuration = configuration };
        state.Board[6][0] = EGameCellState.Player1;
        state.Board[0][0] = EGameCellState.Player1;
        state.Board[1][0] = EGameCellState.Player1;
        state.Board[2][0] = EGameCellState.Player1;

        Assert.True(new GameBrain(state).IsWinningMove(2, 0));
    }

    [Fact]
    public void Cylinder_WrapsWhenCheckingFromRightEdge()
    {
        var configuration = new GameConfiguration("Cylinder", 7, 6, 4, true);
        var state = new GameState(7, 6, 4) { Configuration = configuration };
        state.Board[5][0] = EGameCellState.Player1;
        state.Board[6][0] = EGameCellState.Player1;
        state.Board[0][0] = EGameCellState.Player1;
        state.Board[1][0] = EGameCellState.Player1;

        Assert.True(new GameBrain(state).IsWinningMove(6, 0));
    }

    [Fact]
    public void Cylinder_NarrowBoard_ThreeInARow_IsNotFour()
    {
        var configuration = new GameConfiguration("Cylinder narrow", 3, 7, 4, true);
        var state = new GameState(3, 7, 4) { Configuration = configuration };
        state.Board[0][6] = EGameCellState.Player1;
        state.Board[1][6] = EGameCellState.Player1;
        state.Board[2][6] = EGameCellState.Player1;

        Assert.False(new GameBrain(state).IsWinningMove(1, 6));
    }

    [Fact]
    public void Cylinder_NarrowBoard_VerticalFour_IsAWin()
    {
        var configuration = new GameConfiguration("Cylinder narrow", 3, 7, 4, true);
        var state = new GameState(3, 7, 4) { Configuration = configuration };
        state.Board[0][3] = EGameCellState.Player1;
        state.Board[0][4] = EGameCellState.Player1;
        state.Board[0][5] = EGameCellState.Player1;
        state.Board[0][6] = EGameCellState.Player1;

        Assert.True(new GameBrain(state).IsWinningMove(0, 4));
    }

    [Fact]
    public void MakeMove_RejectsNonLandingCell()
    {
        var brain = new GameBrain(7, 6, 4);

        Assert.Throws<InvalidOperationException>(() =>
            brain.MakeMove(0, 1, EGameCellState.Player1));
    }

    [Fact]
    public void MakeMove_RejectsMoveByWrongPlayer()
    {
        var brain = new GameBrain(7, 6, 4);

        Assert.Throws<InvalidOperationException>(() =>
            brain.MakeMove(0, 5, EGameCellState.Player2));
    }

    [Fact]
    public void MakeMove_RecordsHistoryAndSwitchesPlayer()
    {
        var brain = new GameBrain();

        brain.MakeMove(0, 5, EGameCellState.Player1);
        brain.SwitchPlayer();

        Assert.Single(brain.GetState().MoveHistory);
        Assert.Equal(EGameCellState.Player2, brain.CurrentPlayer);
    }

    [Fact]
    public void FullBoard_IsDrawWhenThereIsNoWinningLine()
    {
        var brain = new GameBrain(2, 2, 3);
        brain.MakeMove(0, 1, EGameCellState.Player1);
        brain.SwitchPlayer();
        brain.MakeMove(1, 1, EGameCellState.Player2);
        brain.SwitchPlayer();
        brain.MakeMove(0, 0, EGameCellState.Player1);
        brain.SwitchPlayer();
        brain.MakeMove(1, 0, EGameCellState.Player2);

        Assert.True(brain.IsBoardFull());
        Assert.False(brain.IsWinningMove(1, 1));
    }

    [Fact]
    public void Validate_RejectsWinConditionLargerThanBoard()
    {
        var configuration = new GameConfiguration("Impossible", 2, 2, 5);

        Assert.Throws<ArgumentOutOfRangeException>(configuration.Validate);
    }

    [Fact]
    public void Presets_AreAllValid()
    {
        foreach (var preset in GameConfiguration.Presets)
        {
            preset.Validate();
        }
    }
}
