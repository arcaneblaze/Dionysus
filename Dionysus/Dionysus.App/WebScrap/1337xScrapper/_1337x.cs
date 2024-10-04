using Dionysus.WebScrap;
using HtmlAgilityPack;

namespace Dionysus.App.WebScrap._1337Scrapper;

public class _1337x
{
    private static string _1337xLink = "https://1337xx.to";

    public static async Task<IEnumerable<SearchGameInfoStruct>> SearchRequestData(string request)
{
    var torrentsList = new List<SearchGameInfoStruct>();
    request = request.Replace("the", "")
        .Replace("The", "");
    var finishLink = $"{_1337xLink}/sort-category-search/{request}/Games/seeders/desc/1/";

    try
    {
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(finishLink);

        var htmlDocument = new HtmlAgilityPack.HtmlDocument();
        htmlDocument.LoadHtml(html);

        var torrents = htmlDocument.DocumentNode.SelectNodes(
            "//table[contains(@class, 'table-list table table-responsive table-striped')]/tbody/tr");

        if (torrents != null)
        {

            foreach (var torrent in torrents)
            {
                var nameNode = torrent.SelectSingleNode(".//td[contains(@class, 'coll-1 name')]/a[2]");
                var uploaderNode = torrent.SelectSingleNode(".//td[contains(@class, 'coll-5 uploader')]");
                var sizeNode = torrent.SelectSingleNode(".//td[contains(@class, 'coll-4 size')]");
                var seedsNode = torrent.SelectSingleNode(".//td[contains(@class, 'coll-2 seeds')]");
                var leechesNode = torrent.SelectSingleNode(".//td[contains(@class, 'coll-3 leeches')]");
                var timeNode = torrent.SelectSingleNode(".//td[contains(@class, 'coll-date')]");

                if (nameNode != null && uploaderNode != null && sizeNode != null && seedsNode != null && leechesNode != null && timeNode != null)
                {

                    var link = _1337xLink + nameNode.Attributes["href"].Value;
                    var name = nameNode.InnerText.Trim();
                    var uploader = uploaderNode.InnerText.Trim();
                    var size = sizeNode.ChildNodes[0].InnerText.Trim();
                    var seeds = seedsNode.InnerText.Trim();
                    var leeches = leechesNode.InnerText.Trim();
                    var time = timeNode.InnerText.Trim();
                    
                    var _rephrasedName = name.Replace(":", " ").Replace("-", " ").Replace(".", " ");
                    var _rephrasedRequest = request.Replace(":", "").Replace("-", "").Replace(".", " ");
                    
                    if (!uploader.Equals("FitGirl", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_rephrasedName.Contains(_rephrasedRequest, StringComparison.OrdinalIgnoreCase))
                        {
                            torrentsList.Add(new SearchGameInfoStruct()
                            {
                                Name = name,
                                Uploader = uploader,
                                Link = link,
                                Size = size
                            });
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("No torrents found!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occurred: {ex.Message}");
    }

    return torrentsList;
}
    
    private static string RemoveCommonArticles(string input)
    {
        input.Replace("the", "");
        return input.Trim();
    }


    public static async Task<IEnumerable<LinkGameInfoStruct>> LinkRequestData(string link)
    {
        var infoList = new List<LinkGameInfoStruct>();

        try
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(link);
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(html);
            
            var name = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[1]/h1").InnerText.Trim();
            var category = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[2]/li[1]/span").InnerText.Trim();
            var type = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[2]/li[2]/span").InnerText.Trim();
            var language = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[2]/li[3]/span").InnerText.Trim();
            var totalSize = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[2]/li[4]/span").InnerText.Trim();
            var uploadedBy = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[2]/li[5]/span/a").InnerText.Trim();
            var downloads = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[3]/li[1]/span").InnerText.Trim();
            var seeds = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[3]/li[4]/span").InnerText.Trim();
            var leeches = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[3]/li[5]/span").InnerText.Trim();
            var downloadLink = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[1]/li[1]/a")
                    .Attributes["href"].Value.Replace("&amp;", "&");
            var filesNode = 
                htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"files\"]");
            var trackerListNode = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[33]/div/div[3]/ul");
            var infohash = 
                htmlDocument.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[25]/div/p/span").InnerText.Trim();

            var files = new List<string>();
            foreach (var ul in filesNode.SelectNodes("ul") ?? Enumerable.Empty<HtmlNode>())
            {
                files.AddRange(ul.SelectNodes("li").Select(li => li.InnerText.Trim()));
            }
            
            var trackerList = new List<string>();
            foreach (var li in trackerListNode.ChildNodes)
            {
                trackerList.Add(li.InnerText.Trim());
            }
            
            infoList.Add(new LinkGameInfoStruct()
            {
                Name = name,
                DownloadLink = downloadLink,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return infoList;
    }
}