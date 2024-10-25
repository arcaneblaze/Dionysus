using System.Net;
using System.Text.RegularExpressions;
using craftersmine.SteamGridDBNet;
using Dionysus.App.Data;
using Dionysus.App.Logger;
using Dionysus.Web;

namespace Dionysus.WebScrap.XatabScrapper;

public class Xatab
{
    private static string _baseLink = "https://byxatab.com";
    private static readonly HttpClientHandler _handler = new HttpClientHandler()
    {
        UseCookies = true,
        CookieContainer = new CookieContainer()
    };
    private static readonly HttpClient _httpClient = new HttpClient(_handler);
    private static Logger _logger = new();
    
    public static async Task<bool> GetStatus()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_baseLink);
            if (response.IsSuccessStatusCode)
            {
                _logger.Log(Logger.LogType.DEBUG,$"Website {_baseLink} is available. Status: {response.StatusCode}");
                return true;
            }
            else
            {
                _logger.Log(Logger.LogType.DEBUG,$"Website {_baseLink} is unavailable. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LogType.ERROR,ex.Message);
            return false;
        }
    }

    public static async Task<IEnumerable<SearchGameInfoStruct>> GetSearchResponse(string _request)
    {
        var _siteLink = $"{_baseLink}/search/{_request}";
        var _responseList = new List<SearchGameInfoStruct>();

        try
        {
            var _html = await _httpClient.GetStringAsync(_siteLink);
            var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.LoadHtml(_html);

            var _responseDivs =
                _htmlDocument.DocumentNode.SelectNodes("//div[@class='entry']");

            foreach (var _div in _responseDivs)
            {
                var _titleNode = _div.SelectSingleNode(".//div[@class='entry__title h2']/a");
                var _gameLink = _titleNode.Attributes["href"].Value;
                var _title = _titleNode.InnerText.Trim();
                if(_title.Contains("Decepticon") || _title == null) continue;

                var (_downloadLink, _size, _version) = await GetDataFromLink(_gameLink);
                
                var rephrasedName = NormalizeName(_title);
                
                if (IsGameMatch(rephrasedName, _request))
                {
                    _responseList.Add(new SearchGameInfoStruct()
                    {
                        Cover = await SteamGridDB.GetGridUri(rephrasedName),
                        Name = _title.Replace("&#039;","'"),
                        Link = _gameLink,
                        Size = _size.Replace("Гб", "GB").Replace("гб","GB"),
                        DownloadLink = _downloadLink,
                        Version = _version
                    });   
                }
            }
            
            async Task<(string _downloadLink, string _size, string _version)> GetDataFromLink(string _link)
            {
                var _html = await _httpClient.GetStringAsync(_link);
                var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
                _htmlDocument.LoadHtml(_html);
                
                
                string _downloadLink = String.Empty;
                try
                {
                    
                    var downloadLinkNode = _htmlDocument.DocumentNode.SelectSingleNode("//a[contains(@class, 'download-torrent')]");
                    if (downloadLinkNode != null)
                    {
                        _downloadLink = downloadLinkNode.GetAttributeValue("href", "");
                    }
                    else
                    {
                        _logger.Log(Logger.LogType.ERROR,"Download link not founded.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(Logger.LogType.ERROR,ex.Message);
                }

                var versionNode = _htmlDocument.DocumentNode.SelectSingleNode("//b[contains(text(), 'Версия игры')]");
                string _version = string.Empty;
                if (versionNode != null)
                {
                    _version = versionNode.InnerText.Replace("Версия игры: ", "").Replace("&nbsp;", " ").Trim();
                }
                else
                {
                    versionNode = _htmlDocument.DocumentNode.SelectSingleNode("//span[contains(text(), 'Версия игры')]");
    
                    if (versionNode != null)
                    {
                        _version = versionNode.InnerText.Replace("Версия игры: ", "").Replace("&nbsp;", " ").Trim();
                    }
                    else
                    {
                        versionNode = _htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'inner-entry__content-text')]/span");
                        if (versionNode != null)
                        {
                            _version = versionNode.InnerText.Replace("- Версия игры: ", "").Replace("&nbsp;", " ").Trim();
                        }
                        else
                        {
                            _logger.Log(Logger.LogType.ERROR,"Version not founded.");
                        }
                    }
                }
                var _size = _htmlDocument.DocumentNode.SelectSingleNode("//span[@class='entry__info-size']").InnerText
                    .Trim();

                string versionPattern = @"(\d+\.\d+|\d+)";
                Match match = Regex.Match(_version, @"\d+(\.\d+)+");
                
                return (_downloadLink, _size, match.Value);
            }
        }
        catch (Exception e)
        {
            _logger.Log(Logger.LogType.ERROR,e.Message);
            throw;
        }

        return _responseList;
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
            .Replace("#038;", " & ");
        
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized;
    }
}