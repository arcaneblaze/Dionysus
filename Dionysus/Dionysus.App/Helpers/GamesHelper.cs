using System.Diagnostics;
using Dionysus.App.Data;
using Dionysus.App.Models;

namespace Dionysus.App.Helpers;

public class GamesHelper
{
    static bool GameIsRunning(string gameName) => Process.GetProcessesByName(gameName).Length > 0;
    
    public static (bool isRunning, string gameLocation, string gameId ,string gameName) GameFromListIsRunning(List<GameModel> gamesList)
    {
        if (gamesList != null)
        {
            foreach (var _game in gamesList)
            {
                if (GameIsRunning(Path.GetFileNameWithoutExtension(_game.Location)))
                {
                    var _gameName = _game.Location;
                    return (true, _gameName, _game.Id.ToString(), _game.Title);
                }
            }
        }
        return (false, null, null, null);
    }

    public static void IfGameFromListDeleted(List<GameModel> gamesList)
    {
        gamesList.RemoveAll(_game => !File.Exists(_game.Location));
        GameData.GamesData.SaveToJSON(gamesList);
    }
}