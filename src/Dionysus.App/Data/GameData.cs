using Dionysus.App.Models;
using Dionysus.Web;
using Newtonsoft.Json;

namespace Dionysus.App.Data;

public class GameData
{
    public class GamesData
    {
        public static string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json");
        public static string _backJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json.bak");
        private static Logger.Logger _logger = new ();
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
                _logger.Log(Logger.Logger.LogType.DEBUG,$"{_jsonPath} Parsed");
                return gamesList ?? new List<GameModel>();
            }
            catch (FileNotFoundException ex)
            {
                _logger.Log(Logger.Logger.LogType.ERROR,$"File not found: {ex.FileName}");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.Logger.LogType.ERROR,$"Error occurred while parsing the JSON file: {ex.Message}");
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
                _logger.Log(Logger.Logger.LogType.DEBUG,$"{_backJsonPath} Parsed");
                return gamesList ?? new List<GameModel>();
            }
            catch (FileNotFoundException ex)
            {
                _logger.Log(Logger.Logger.LogType.ERROR,$"File not found: {ex.FileName}");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.Logger.LogType.ERROR,$"Error occurred while parsing the JSON file: {ex.Message}");
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

        public static string GetElapsedTime(DateTime earlyDateTime)
        {
            var difference = DateTime.Now - earlyDateTime;
            if (difference.TotalSeconds < 60)
            {
                return $"{Math.Floor(difference.TotalSeconds)} {Localization.strings.LibraryPage_ElapsedTimeSeconds}";
            }
            else if (difference.TotalMinutes < 60)
            {
                return $"{Math.Floor(difference.TotalMinutes)} {Localization.strings.LibraryPage_ElapsedTimeMinutes}";
            }
            else if (difference.TotalHours < 24)
            {
                int hours = difference.Hours;
                return $"{hours} {Localization.strings.LibraryPage_ElapsedTimeHours}";
            }
            else
            {
                int days = difference.Days;
                return $"{days} {Localization.strings.LibraryPage_ElapsedTimeDays}";
            }
        }
    }
}