using craftersmine.SteamGridDBNet;
using Dionysus.App.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dionysus.App.Data;

public class SteamGridDB
{
    private static string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "env.json");
    private static JObject configJson = (JObject)JsonConvert.DeserializeObject(System.IO.File.ReadAllText(_jsonPath));
    private static string steamGDBAPI = configJson["steamGDBAPI"].Value<string>();
    static SteamGridDb _steamGridDb = new SteamGridDb(steamGDBAPI);
    
    public static async Task<string> GetGridUri(string _gameName)
    {
        var game = await _steamGridDb.SearchForGamesAsync(_gameName);
        var icons = await _steamGridDb.GetGridsForGameAsync(game[0], dimensions: SteamGridDbDimensions.W920H430); 
        return icons[0].FullImageUrl;
    }
    
}