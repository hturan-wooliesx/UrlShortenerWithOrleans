using Orleans.Runtime;

namespace UrlShortenerWithOrleans.Middlewares
{
    public class RequestContextPopulatorMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // read bearer token
            var bearerToken = context.Request?.Headers?.Authorization.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(bearerToken))
            {
                // set token into Orleans request context
                RequestContext.Set("bearerToken", bearerToken);
            }
           
            await next(context);
        }
    }
}
