using Orleans.Runtime;
using UrlShortenerWithOrleans.Attributes;
using UrlShortenerWithOrleans.Interfaces;
using UrlShortenerWithOrleans.Models;

namespace UrlShortenerWithOrleans.Grains
{
    public sealed class UrlShortenerGrain : Grain, IUrlShortenerGrain
    {
        // private KeyValuePair<string, string> _cache;
        private readonly IPersistentState<UrlDetails> _cache;
        public UrlShortenerGrain(
        [PersistentState(
            stateName: "url",
            storageName: "urls")]
            IPersistentState<UrlDetails> state)
        {
            _cache = state;
        }

        public Task<string> GetUrl()
        {
            return Task.FromResult(_cache.State.FullUrl);
        }

        [AuthenticatedOnly]
        public Task SetUrl(string fullUrl)
        {
            if (string.IsNullOrWhiteSpace(fullUrl))
            {
                return Task.CompletedTask;
            }

            _cache.State = new UrlDetails
            {
                ShortenedRouteSegment = this.GetPrimaryKeyString(),
                FullUrl = fullUrl
            };

            return _cache.WriteStateAsync();
        }
    }
}
