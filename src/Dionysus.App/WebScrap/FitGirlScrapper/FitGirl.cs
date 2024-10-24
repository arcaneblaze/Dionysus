using System.Collections;
using System.Text.RegularExpressions;
using Dionysus.App.Data;

namespace Dionysus.WebScrap.FitGirlScrapper;

public class FitGirl
{
    public static async Task<bool> GetStatus()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://fitgirl-repacks.site/");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Website https://fitgirl-repacks.site/ is available. Status: {response.StatusCode}");
                    return true; 
                }
                else
                {
                    Console.WriteLine($"Website https://fitgirl-repacks.site/ is unavailable. Status: {response.StatusCode}");
                    return false; 
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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

                    var _rephrasedName = _name.InnerText.Trim()
                        .Replace("&#8211;", "-")
                        .Replace("&#038;", "&")
                        .Replace("&#8217;", "`")
                        .Replace(":", "")
                        .Replace("-", " ");
                    var _rephrasedRequest = _gameName.Replace(":", "").Replace("-", "");
    
                    var (downloadLink, size, version) = await GetDataFromLink(_link);

                    if (_rephrasedName.ToLower().Contains(_rephrasedRequest.ToLower()))
                    {
                        _list.Add(new SearchGameInfoStruct()
                        {
                            Cover = await SteamGridDB.GetGridUri(_rephrasedName),
                            Name = _rephrasedName,
                            Link = _link,
                            Size = size,
                            Version = version,
                            DownloadLink = downloadLink
                        });
                    }
                });

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
}