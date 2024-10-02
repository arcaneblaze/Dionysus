using Dionysus.App.Models;
using Newtonsoft.Json;

namespace Dionysus.App.Data;

public class GameData
{
    public class GamesData
    {
        public static IEnumerable<GameModel> ParseGamesFromJSON()
        {
            var path = "Data/games.json";
            if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
            if (!File.Exists(path))
            {
                File.Create("Data/games.json");
                File.WriteAllText(path, "[]");
            }
            var jsonData = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<GameModel>>(jsonData);
        
        }

        public static void SaveToJSON(List<GameModel> gamesList)
        {
            var path = "Data/games.json";
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);

            }
            var json = JsonConvert.SerializeObject(gamesList, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}