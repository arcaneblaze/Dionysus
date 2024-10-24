using Dionysus.Web;

namespace Dionysus.App.Data;

public class Backup
{
    public static async Task MakeBackupAsync()
    {
        var _fileData = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json"));

        var _backupPath = Path.GetDirectoryName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json"));
        var _backupName = Path.GetFileName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json"));
        File.WriteAllTextAsync(_backupPath + $"/{_backupName}.bak", _fileData);
        Console.WriteLine($"Backup for {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/games.json")} maked");
    }

    public static async Task ReadBackupAsync()
    {
        var _backData = GameData.GamesData.ParseBackFromJSON();
        GameData.GamesData.SaveToJSON(_backData.ToList());
        new Thread(() =>
        {
            MainPage._gamesList = GameData.GamesData.ParseGamesFromJSON().ToList();
        }).Start();
        Console.WriteLine("Backup Restored");
    }
}