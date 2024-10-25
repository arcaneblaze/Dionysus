using craftersmine.SteamGridDBNet;
using Dionysus.App.Data;
using Dionysus.App.Logger;
using Dionysus.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dionysus.WebScrap.GOGScrapper;

public class GOG
{
    private static Logger _logger = new Logger();
    public static async Task<bool> GetStatus()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://freegogpcgames.com");
                if (response.IsSuccessStatusCode)
                {
                    _logger.Log(Logger.LogType.DEBUG, 
                        $"Website https://freegogpcgames.com is available. Status: {response.StatusCode}");
                    return true; 
                }
                else
                {
                    _logger.Log(Logger.LogType.DEBUG,
                        $"Website https://freegogpcgames.com is unavailable. Status: {response.StatusCode}");
                    Console.WriteLine();
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
    
    public static async Task<IEnumerable<SearchGameInfoStruct>> GetSearchResponse(string _request)
{
    var _siteLink = $"https://freegogpcgames.com/?s={_request}";
    var _responseList = new List<SearchGameInfoStruct>();
    try
    {
        using var _httpClient = new HttpClient();
        var _html = await _httpClient.GetStringAsync(_siteLink);
        var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
        _htmlDocument.LoadHtml(_html);

        var _responseDivs = _htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'generate-columns-container')]/article");
        if (_responseDivs != null)
        {
            var tasks = _responseDivs.Select(async _div =>
            {
                var _name = _div.SelectSingleNode(".//header/h2/a");
                var _link = _name.Attributes["href"].Value;

                var _rephrasedName = _name.InnerText.Trim()
                    .Replace("&#8211;", "-")
                    .Replace("&#038;", "&")
                    .Replace("&#8217;", "`")
                    .Replace(":", "")
                    .Replace("-", "");
                var _rephrasedRequest = _request.Replace(":", "").Replace("-", "");

                var (downloadLink, size) = await GetDataFromLink(_link);

                if (_rephrasedName.ToLower().Contains(_rephrasedRequest.ToLower()))
                {
                    var _downloadLink = BypassDownloadLink(downloadLink);
                    _responseList.Add(new SearchGameInfoStruct()
                    {
                        Cover = await SteamGridDB.GetGridUri(_rephrasedName),
                        Name = _rephrasedName,
                        Link = _link,
                        Size = size.Replace("Size: ", "").Replace("GiB", "GB"),
                        DownloadLink = _downloadLink
                    });
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
    return _responseList;
}

private static async Task<(string downloadLink, string size)> GetDataFromLink(string _link)
{
    using var _httpClient = new HttpClient();
    var _html = await _httpClient.GetStringAsync(_link);
    var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
    _htmlDocument.LoadHtml(_html);

    var _downloadLink = _htmlDocument.DocumentNode
        .SelectSingleNode("//a[contains(@class, 'download-btn')]").Attributes["href"].Value;

    var _size = _htmlDocument.DocumentNode
        .SelectSingleNode("//div[contains(@class, 'inside-article')]/div[1]/p[6]/em").InnerText
        .Trim();

    return (_downloadLink, _size);
}

private static string BypassDownloadLink(string url)
{
    using var _client = new HttpClient();
    var _htmlString = _client.GetStringAsync(url).Result;
    var _document = new HtmlAgilityPack.HtmlDocument();
    _document.LoadHtml(_htmlString);
    return _document.DocumentNode.SelectSingleNode("//a[contains(@class, 'button')]")
        .Attributes["href"].Value.Replace("&amp;", "&").Replace("&#038;", "&");
}
}