using Dionysus.App.Models;
using Newtonsoft.Json;

namespace Dionysus.App.Data;

public class GameData
{
    public class GamesData
    {
        private static string _josnPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json");
        public static IEnumerable<GameModel> ParseGamesFromJSON()
        {
            try
            {
                if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
                
                if (!File.Exists(_josnPath))
                {
                    File.WriteAllText(_josnPath, "[]"); 
                }
                
                var jsonData = File.ReadAllText(_josnPath);
                
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
            var directory = Path.GetDirectoryName(_josnPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);

            }
            var json = JsonConvert.SerializeObject(gamesList, Formatting.Indented);
            File.WriteAllText(_josnPath, json);
        }
    }
}