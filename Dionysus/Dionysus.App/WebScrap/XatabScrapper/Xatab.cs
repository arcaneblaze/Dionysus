using System.Net;
using craftersmine.SteamGridDBNet;
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
    
    public static async Task<bool> GetStatus()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_baseLink);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Website {_baseLink} is available. Status: {response.StatusCode}");
                return true;
            }
            else
            {
                Console.WriteLine($"Website {_baseLink} is unavailable. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
                if(_title.Contains("Decepticon") || _title.Contains("[GOG]") || _title == null) continue;

                var (_downloadLink, _size) = await GetDataFromLink(_gameLink);
                
                var _rephrasedName = _title
                    .Replace("&#8211;", "-")
                    .Replace("&#038;","&")
                    .Replace("&#8217;","`")
                    .Replace(":", "")
                    .Replace("-", "");
                var _rephrasedRequest = _request.Replace(":", "").Replace("-", "");
                if (_rephrasedName.ToLower().Contains(_rephrasedRequest.ToLower()))
                {
                    var gameA = await GamesPage._steamGridDb.SearchForGamesAsync(_title);
                    var icons = await GamesPage._steamGridDb.GetGridsForGameAsync(gameA[0],
                        dimensions: SteamGridDbDimensions.W920H430);
                    var imageUrl = icons[0].FullImageUrl;
                
                    _responseList.Add(new SearchGameInfoStruct()
                    {
                        Cover = imageUrl,
                        Name = _title,
                        Link = _gameLink,
                        Size = _size.Replace("Гб", "GB").Replace("гб","GB"),
                        DownloadLink = _downloadLink
                    });   
                }
            }
            
            async Task<(string _downloadLink, string _size)> GetDataFromLink(string _link)
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
                        Console.WriteLine("Download link not founded.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                

                var _size = _htmlDocument.DocumentNode.SelectSingleNode("//span[@class='entry__info-size']").InnerText
                    .Trim();

                return (_downloadLink, _size);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return _responseList;
    }
}