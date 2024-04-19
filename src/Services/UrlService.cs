using UrlShortenerWithOrleans.Interfaces;

namespace UrlShortenerWithOrleans.Services
{
    public interface IUrlService
    {
        Task<(bool Success, string ShortenedUrl, string ErrorMessage)> ShortenUrl(string host, string url);
        Task<(bool Success, string RedirectUrl)> RedirectUrl(string shortenedRouteSegment);
    }

    public class UrlService(IGrainFactory grains) : IUrlService
    {
        private readonly IGrainFactory _grains = grains;
        public async Task<(bool Success, string RedirectUrl)> RedirectUrl(string shortenedRouteSegment)
        {
            // Retrieve the grain using the shortened ID and url to the original URL
            var shortenerGrain =
                grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

            if (shortenerGrain is null)
            {
                return (false, string.Empty);
            }

            var url = await shortenerGrain.GetUrl();

            // Handles missing schemes, defaults to "http://".
            var redirectBuilder = new UriBuilder(url);

            return (true, redirectBuilder.Uri.ToString());
        }

        public async Task<(bool, string, string)> ShortenUrl(string host, string url)
        {
            // Validate the URL query string.
            if (string.IsNullOrWhiteSpace(url) &&
                Uri.IsWellFormedUriString(url, UriKind.Absolute) is false)
            {
                return (false, string.Empty, $"""
                The URL query string is required and needs to be well formed.
                Consider, ${host}/shorten?url=https://www.microsoft.com.
                """);
            }

            // Create a unique, short ID
            var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");

            // Create and persist a grain with the shortened ID and full URL
            var shortenerGrain = _grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

            // not null but repetitive as its needed for each grain method that have AuthenticatedOnly attribute

            //var bearerToken = _contextAccessor?.HttpContext?.Request?.Headers?.Authorization.ToString() ?? string.Empty;
            //RequestContext.Set("bearerToken", bearerToken);

            await shortenerGrain.SetUrl(url);

            return (true, shortenedRouteSegment, string.Empty);
        }
    }
}
