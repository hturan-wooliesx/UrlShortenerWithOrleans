namespace UrlShortenerWithOrleans.Models
{
    [GenerateSerializer]
    public class UrlDetails
    {
        [Id(0)]
        public string FullUrl { get; set; } = string.Empty;

        [Id(1)]
        public string ShortenedRouteSegment { get; set; } = string.Empty;
    }
}
