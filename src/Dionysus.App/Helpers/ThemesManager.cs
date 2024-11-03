namespace Dionysus.App.Helpers;

public class ThemesManager
{
    public static List<string> GetThemes(string _path = "Web/wwwroot/pagesCSS") => Directory.GetDirectories(_path).ToList();
}