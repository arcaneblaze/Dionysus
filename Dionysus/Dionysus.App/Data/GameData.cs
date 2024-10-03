using Dionysus.App.Models;
using Newtonsoft.Json;

namespace Dionysus.App.Data;

public class GameData
{
    public class GamesData
    {
        public static IEnumerable<GameModel> ParseGamesFromJSON()
        {
            try
            {
                var path = "Data/games.json";
                if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
                
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "[]"); 
                }
                
                var jsonData = File.ReadAllText(path);
                
                var gamesList = JsonConvert.DeserializeObject<List<GameModel>>(jsonData);
                return gamesList ?? new List<GameModel>();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.FileName}");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while parsing the JSON file: {ex.Message}");
            }
            
            return new List<GameModel>();
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