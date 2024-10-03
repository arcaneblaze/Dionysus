using Dionysus.App.Data;
using Dionysus.App.Models;

namespace Dionysus.App.Helpers;

public class CountTimeHelper
{
    private static int hours = 0;
    private static int minutes = 0;
    private static int seconds = 0;
    
    private static bool isCounting = false;

    public static async Task Count(List<GameModel> gamesList, string gameLocation)
    {
        if (isCounting) return;

        isCounting = true;

        var _game = gamesList.FirstOrDefault(g => g.Location == gameLocation);

        if (_game != null)
        {
            var _parsedTime = _game.TimeInfo;
            if (!string.IsNullOrEmpty(_parsedTime))
            {
                var timeComponents = _parsedTime.Split(' ');

                if (timeComponents.Length == 2)
                {
                    if (timeComponents[0].EndsWith("h"))
                    {
                        if (int.TryParse(timeComponents[0].Replace("h", ""), out int parsedHours))
                        {
                            hours = parsedHours;
                        }
                    }

                    if (timeComponents[1].EndsWith("m"))
                    {
                        if (int.TryParse(timeComponents[1].Replace("m", ""), out int parsedMinutes))
                        {
                            minutes = parsedMinutes;
                        }
                    }
                }
            }
            else
            {
                hours = 0;
                minutes = 0;
            }

            while (true)
            {
                seconds++;

                if (seconds == 60)
                {
                    seconds = 0;
                    minutes++;
                }

                if (minutes == 60)
                {
                    minutes = 0;
                    hours++;
                }

                if (hours == 24)
                {
                    hours = 0;
                }

                _game.TimeInfo = $"{hours}h {minutes}m";

                GameData.GamesData.SaveToJSON(gamesList);

                await Task.Delay(1000);
            }
        }
        isCounting = false;
    }
}