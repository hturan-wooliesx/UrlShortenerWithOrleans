using Orleans.Runtime;
using System.Reflection;
using UrlShortenerWithOrleans.Attributes;
using UrlShortenerWithOrleans.Services;

namespace UrlShortenerWithOrleans.Filters
{
    public class AuthenticationGrainFilter : IIncomingGrainCallFilter
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationGrainFilter(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            // var httpContext = _contextAccessor.HttpContext; // always null
            // check if the incoming call made through a method with AuthenticatedOnlyAttribute
            var isAuthenticatedOnly =
                context.ImplementationMethod.GetCustomAttribute<AuthenticatedOnlyAttribute>();

            if (isAuthenticatedOnly is not null)
            {
                var bearerToken = (string)RequestContext.Get("bearerToken");
                var isAuthenticationValid = await _authenticationService.ValidateBearerToken(bearerToken);
                if (!isAuthenticationValid)
                {
                    throw new Exception($"Only authenticated users can access" +
                        $" {((Grain)context.Grain).GrainReference.GrainId.Type}" +
                        $" grain's {context.ImplementationMethod.Name} method!");
                }
            }

            await context.Invoke();
        }
    }
}
