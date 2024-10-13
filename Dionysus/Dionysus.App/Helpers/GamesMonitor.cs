using System.Diagnostics;
using Dionysus.App.Data;
using Dionysus.App.Models;

namespace Dionysus.App.Helpers;

public class GamesMonitor
{
    /// <summary>
    /// Tracking functions
    /// </summary>
    private static bool GameIsRunning(string gameName) => Process.GetProcessesByName(gameName).Length > 0;

    public static (bool isRunning, GameModel runningGame) GameFromListIsRunning(List<GameModel> gamesList)
    {
        if (gamesList == null || gamesList.Count == 0)
            return (false, null);

        foreach (var game in gamesList)
        {
            if (GameIsRunning(Path.GetFileNameWithoutExtension(game.Location)))
            {
                return (true, game);
            }
        }

        return (false, null);
    }

    public static void RemoveDeletedGames(List<GameModel> gamesList)
    {
        if (gamesList == null) return;
        gamesList.RemoveAll(game => !File.Exists(game.Location));
        GameData.GamesData.SaveToJSON(gamesList);
    }


    /// <summary>
    /// Time counter functions
    /// </summary>

    #region variables

    private static int hours = 0;

    private static int minutes = 0;
    private static int seconds = 0;
    public static bool isCounting = false;

    #endregion

    private static readonly object lockObject = new object();

    public static async Task CountPlayTime(List<GameModel> gamesList, string gameLocation)
    {
        bool isAlreadyCounting;

        lock (lockObject)
        {
            isAlreadyCounting = isCounting;
            if (!isAlreadyCounting)
            {
                isCounting = true;
            }
        }

        if (isAlreadyCounting)
        {
            Console.WriteLine("CountPlayTime already running, skipping.");
            return;
        }

        var game = gamesList.FirstOrDefault(g => g.Location == gameLocation);
        if (game != null)
        {
            Console.WriteLine($"[DEBUG] Starting to count time for game: {game.Title} at {DateTime.Now}");

            ResetTime();
            ParseTimeInfo(game.TimeInfo);

            while (isCounting)
            {
                await Task.Delay(1000);
                IncrementTime();

                game.TimeInfo = $"{hours}h {minutes}m";

                Console.WriteLine($"[DEBUG] {game.Title} - Time: {hours}h {minutes}m {seconds}s");
            }

            Console.WriteLine($"[DEBUG] Game closed, saving final time for {game.Title} at {DateTime.Now}");
            await GameData.GamesData.SaveToJSON(gamesList);
        }
        else
        {
            Console.WriteLine($"Game with location {gameLocation} not found in the list.");
        }

        lock (lockObject)
        {
            isCounting = false;
        }
    }

    private static void ResetTime()
    {
        Console.WriteLine("[DEBUG] Resetting time to 0.");
        hours = 0;
        minutes = 0;
        seconds = 0;
    }

    private static void ParseTimeInfo(string timeInfo)
    {
        Console.WriteLine($"[DEBUG] Parsing time info: {timeInfo}");

        if (!string.IsNullOrEmpty(timeInfo))
        {
            var timeComponents = timeInfo.Split(' ');
            if (timeComponents.Length == 2)
            {
                if (timeComponents[0].EndsWith("h"))
                {
                    int.TryParse(timeComponents[0].Replace("h", ""), out hours);
                    Console.WriteLine($"[DEBUG] Parsed hours: {hours}");
                }

                if (timeComponents[1].EndsWith("m"))
                {
                    int.TryParse(timeComponents[1].Replace("m", ""), out minutes);
                    Console.WriteLine($"[DEBUG] Parsed minutes: {minutes}");
                }
            }
        }
        else
        {
            Console.WriteLine("[DEBUG] No time info available, resetting time.");
            ResetTime();
        }
    }

    private static void IncrementTime()
    {
        seconds++;
        if (seconds == 60)
        {
            seconds = 0;
            minutes++;
            Console.WriteLine($"[DEBUG] Incremented minutes: {minutes}");
        }

        if (minutes == 60)
        {
            minutes = 0;
            hours++;
            Console.WriteLine($"[DEBUG] Incremented hours: {hours}");
        }
    }
}