using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using Dionysus.App.Data;
using Dionysus.App.Logger;

namespace Dionysus.WebScrap.FitGirlScrapper;

public class FitGirl
{
    private static Logger _logger = new Logger();
    
    public static async Task<bool> GetStatus()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://fitgirl-repacks.site/");
                if (response.IsSuccessStatusCode)
                {
                    _logger.Log(Logger.LogType.DEBUG, 
                        $"Website https://fitgirl-repacks.site/ is available. Status: {response.StatusCode}");
                    return true; 
                }
                else
                {
                    _logger.Log(Logger.LogType.DEBUG, 
                        $"Website https://fitgirl-repacks.site/ is unavailable. Status: {response.StatusCode}");
                    return false; 
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LogType.ERROR, ex.Message);
            return false; 
        }
    }

    public static async Task<IEnumerable<SearchGameInfoStruct>> GetSearchResponse(string _gameName)
    {
        
        var _list = new List<SearchGameInfoStruct>();
        var _searchLink = $"https://fitgirl-repacks.site/?s={_gameName}";

        try
        {
            using var _httpClient = new HttpClient();
            var _html = await _httpClient.GetStringAsync(_searchLink);
            var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.LoadHtml(_html);

            var _responseDivs =
                _htmlDocument.DocumentNode.SelectNodes("//article[contains(@class, 'post type-post status-publish format-standard hentry category-lossless-repack')]");
            if (_responseDivs != null)
            {
                var tasks = _responseDivs.Select(async _div =>
                {
                    var _name = _div.SelectSingleNode(".//header/h1/a");
                    var _link = _name.Attributes["href"].Value;
                    
                    var rephrasedName = NormalizeName(_name.InnerText.Trim());

                    var (downloadLink, size, version) = await GetDataFromLink(_link);

                    var _date = _div.SelectSingleNode(".//header/div[2]/span[1]/a/time").InnerText.Trim();
                    
                    if (IsGameMatch(rephrasedName, _gameName))
                    {
                        var _cover = await SteamGridDB.GetGridUri(rephrasedName);
                        if (_cover != null)
                        {
                            _list.Add(new SearchGameInfoStruct()
                            {
                                Cover = _cover,
                                Name = rephrasedName,
                                Link = _link,
                                Size = size,
                                Version = version,
                                DownloadLink = downloadLink,
                                Date = _date
                            });   
                        }
                    }
                });

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception e)
        {
            _logger.Log(Logger.LogType.ERROR, e.Message);
            throw;
        }
        
        return _list;
    }
    
    private static async Task<(string downloadLink, string size, string version)> GetDataFromLink(string _link)
    {
        using var _httpClient = new HttpClient();
        var _html = await _httpClient.GetStringAsync(_link);
        var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
        _htmlDocument.LoadHtml(_html);

        var _downloadNode = _htmlDocument.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]");
        string _downloadLink = _downloadNode != null 
            ? _downloadNode.Attributes["href"].Value.Replace("&amp;", "&").Replace("&#038;", "&") 
            : null;

        var _sizeNode = _htmlDocument.DocumentNode.SelectSingleNode("//strong[contains(text(), 'from')]");
        
        var gameVersionNode = _htmlDocument.DocumentNode.SelectSingleNode("//li[contains(text(), 'Game version:')]");

        string gameVersion = null;
        if (gameVersionNode != null)
        {
            Match match = Regex.Match(gameVersionNode.InnerText, @"v([\d\.]+)");
            if (match.Success)
            {
                gameVersion = match.Groups[1].Value;
            }
        }
        
        string _size = null;
        if (_sizeNode != null)
        {
            string text = _sizeNode.InnerText;
            Match match = Regex.Match(text, @"from\s+(\d+(\.\d+)?\s*GB)");
            if (match.Success)
            {
                _size = match.Value.Trim();
            }
        }

        return (_downloadLink, _size , gameVersion);
    }
    
    private static bool IsGameMatch(string gameName, string searchQuery)
    {
        string NormalizeForComparison(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            
            input = input.ToLower().Trim();
            
            input = Regex.Replace(input, @"[^\w\s]", " ");
            
            input = Regex.Replace(input, @"\s+", " ");
            
            var commonWords = new[] { "repack", "goty", "edition", "complete", "collection" };
            foreach (var word in commonWords)
            {
                input = Regex.Replace(input, $@"\b{word}\b", "", RegexOptions.IgnoreCase);
            }
            
            return input.Trim();
        }

        var normalizedGame = NormalizeForComparison(gameName);
        var normalizedQuery = NormalizeForComparison(searchQuery);
        
        if (string.IsNullOrWhiteSpace(normalizedGame) || string.IsNullOrWhiteSpace(normalizedQuery))
            return false;
        
        var gameWords = normalizedGame.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return queryWords.All(queryWord => 
            gameWords.Any(gameWord => gameWord.Contains(queryWord) || queryWord.Contains(gameWord)));
    }
    
    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty; 
        }
        
        var normalized = name
            .Replace("#8211;", " - ")
            .Replace("&#8211;", " - ")
            .Replace("&nbsp;", " ")
            .Replace("&amp;", " & ")
            .Replace("#038;", " & ")
            .Replace("&#8217;", "'");
        
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized;
    }
}