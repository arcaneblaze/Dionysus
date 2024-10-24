namespace Dionysus.WebScrap;

public struct LinkGameInfoStruct
{
    public string Name { get; set; }
    public string Cover { get; set; }
    public string DownloadLink { get; set; }
    public string Size { get; set; }
    public List<string> GameMedia { get; set; }
}

public struct SearchGameInfoStruct
{
    public string Cover { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public string Uploader { get; set; }
    public string Size { get; set; }
    public string Source { get; set; }
    public string DownloadLink { get; set; }
    public string Version { get; set; }
}