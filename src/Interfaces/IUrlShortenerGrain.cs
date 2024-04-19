using Microsoft.AspNetCore.Authorization;

namespace UrlShortenerWithOrleans.Interfaces
{
    public interface IUrlShortenerGrain : IGrainWithStringKey
    {
        Task SetUrl(string fullUrl);
        Task<string> GetUrl();
    }
}
