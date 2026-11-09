using ConsoleApp;
using Game;
using MenuSystem;

Console.WriteLine("Hello, TIC-TAC-TOE!");

var mainMenu = new Menu("Main Menu");
mainMenu.Items.Add(new MenuItem
{
    Title = "Start Game", 
    Shortcut = "S",
    MethodToRun = () =>
    {
        
        var gameBrain = new GameBrain(5);
        
        var gameController = new GameController();
        return gameController.RunGame(gameBrain);
    }
});

mainMenu.Run();