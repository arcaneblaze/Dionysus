using craftersmine.SteamGridDBNet;
using Dionysus.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dionysus.WebScrap.GOGScrapper;

public class GOG
{
    public static async Task<IEnumerable<SearchGameInfoStruct>> GetSearchResponse(string _request)
    {
        var _siteLink = $"https://freegogpcgames.com/?s={_request}";
        var _responseList = new List<SearchGameInfoStruct>();
        try
        {
            var _httpClient = new HttpClient();
            var _html = await _httpClient.GetStringAsync(_siteLink);
            var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.LoadHtml(_html);

            var _responseDivs =
                _htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'generate-columns-container')]/article");
            if (_responseDivs != null)
            {
                foreach (var _div in _responseDivs)
                {
                    string _cover;
                    if (_responseDivs.First() == _div)
                    {
                        _cover = _div.SelectSingleNode(".//div[@class='post-image']/a/img").Attributes["src"]?.Value;   
                    }
                    else
                    {
                        _cover = _div.SelectSingleNode(".//div[@class='post-image']/a/img").Attributes["data-src"]?.Value;
                    }

                    var _name = _div.SelectSingleNode(".//header/h2/a");
                    var _link = _name.Attributes["href"].Value;

                    var _rephrasedName = _name.InnerText.Trim()
                        .Replace("&#8211;", "-")
                        .Replace("&#038;","&")
                        .Replace("&#8217;","`")
                        .Replace(":", "")
                        .Replace("-", "");
                    var _rephrasedRequest = _request.Replace(":", "").Replace("-", "");
                    
                    if (_rephrasedName.ToLower().Contains(_rephrasedRequest.ToLower()))
                    {
                        try
                        {
                            var gameA = await GamesPage._steamGridDb.SearchForGamesAsync(_rephrasedName);
                            var icons = await GamesPage._steamGridDb.GetGridsForGameAsync(gameA[0],
                                dimensions: SteamGridDbDimensions.W920H430);
                            var imageUrl = icons[0].FullImageUrl;

                            _responseList.Add(new SearchGameInfoStruct()
                            {
                                Cover = imageUrl,
                                Name = _rephrasedName,
                                Link = _link
                            });
                        }
                        catch (Exception e)
                        {
                            return new List<SearchGameInfoStruct>();
                        }
                        
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return _responseList;
    }

    public static async Task<IEnumerable<LinkGameInfoStruct>> GetLinkResponse(string _link)
    {
        var _responseList = new List<LinkGameInfoStruct>();
        try
        {
            var _httpClient = new HttpClient();
            var _html = await _httpClient.GetStringAsync(_link);
            var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.LoadHtml(_html);
            
            var _name = _htmlDocument.DocumentNode.SelectSingleNode("//h1[contains(@class, 'entry-title')]");
            var _image = _htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'featured-image')]/img").Attributes["src"].Value;
            var _downloadLink = _htmlDocument.DocumentNode.SelectSingleNode("//a[contains(@class, 'download-btn')]").Attributes["href"].Value;
            var _size = _htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'inside-article')]/div[1]/p[6]/em");
            var _gameMediaDiv =
                _htmlDocument.DocumentNode.SelectSingleNode("//section[@id='game-media']");
            var _imgNodes = _gameMediaDiv.SelectNodes(".//img");
            var _gameMediaList = _imgNodes.Select(img => img.GetAttributeValue("data-src", "")).ToList();
            _gameMediaList.RemoveAt(_gameMediaList.Count - 2);
            
            
            string bypassDownloadLink(string url) {
                var _client = new HttpClient();
                var _htmlString = _client.GetStringAsync(url).Result;
                var _document = new HtmlAgilityPack.HtmlDocument();
                _document.LoadHtml(_htmlString);
                return _document.DocumentNode.SelectSingleNode("//a[contains(@class, 'button')]").
                    Attributes["href"].Value.Replace("&amp;", "&").Replace("&#038;", "&");
            }
            
            _responseList.Add(new LinkGameInfoStruct() {
                Name = _name.InnerText.Trim(),
                Cover = _image,
                DownloadLink = bypassDownloadLink(_downloadLink),
                Size = _size.InnerText.Trim().Replace("Size: ", ""),
                GameMedia = _gameMediaList
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return _responseList;
    }
}