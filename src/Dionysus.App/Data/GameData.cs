using Dionysus.App.Models;
using Newtonsoft.Json;

namespace Dionysus.App.Data;

public class GameData
{
    public class GamesData
    {
        public static string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json");
        public static string _backJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json.bak");
        public static IEnumerable<GameModel> ParseGamesFromJSON()
        {
            try
            {
                if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
                
                if (!File.Exists(_jsonPath))
                { 
                    File.WriteAllText(_jsonPath, "[]"); 
                }
                
                var jsonData = File.ReadAllText(_jsonPath);
                
                var gamesList = JsonConvert.DeserializeObject<List<GameModel>>(jsonData);
                Console.WriteLine($"{_jsonPath} Parsed");
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
        
        public static IEnumerable<GameModel> ParseBackFromJSON()
        {
            try
            {
                if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
                
                if (!File.Exists(_backJsonPath))
                { 
                    File.WriteAllText(_backJsonPath, "[]"); 
                }
                
                var jsonData = File.ReadAllText(_backJsonPath);
                
                var gamesList = JsonConvert.DeserializeObject<List<GameModel>>(jsonData);
                Console.WriteLine($"{_backJsonPath} Parsed");
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

        public static async Task SaveToJSON(List<GameModel> gamesList)
        {
            var directory = Path.GetDirectoryName(_jsonPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);

            }
            var json = JsonConvert.SerializeObject(gamesList, Formatting.Indented);
            await File.WriteAllTextAsync(_jsonPath, json);

            await Task.CompletedTask;
        }
    }
}