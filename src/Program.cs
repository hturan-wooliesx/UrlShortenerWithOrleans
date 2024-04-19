using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UrlShortenerWithOrleans.Filters;
using UrlShortenerWithOrleans.Middlewares;
using UrlShortenerWithOrleans.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IUrlService, UrlService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
//builder.Services.AddTransient<RequestContextPopulatorMiddleware>();

var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>() ?? "cannot read jwt issuer";
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>() ?? "cannot read jwt key";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddAdoNetGrainStorage("urls", options =>
    {
       options.Invariant = "System.Data.SqlClient";
       options.ConnectionString = "Data Source=localhost\\MSSQLSERVER05;Initial Catalog=UrlShortenerWithOrleans;User ID=sa;Password=p@ssw0rd;Integrated Security=False;Trusted_Connection=True;";
    });

    siloBuilder.AddIncomingGrainCallFilter<AuthenticationGrainFilter>();
    // builder.Services.AddSingleton<IIncomingGrainCallFilter, AuthenticationGrainFilter>();
    siloBuilder.Services.AddTransient<RequestContextPopulatorMiddleware>();

    siloBuilder.UseDashboard(options =>
    {
        // options.Port = 4555;
        options.HostSelf = true;
    });  
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RequestContextPopulatorMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Map("/dashboard", x => x.UseOrleansDashboard());

app.MapGet("/shorten", static async(IUrlService urlService, HttpRequest request, string url) =>
{
    var host = $"{request.Scheme}://{request.Host.Value}";

    var shortenedUrlResult = await urlService.ShortenUrl(host, url);

    if (!shortenedUrlResult.Success)
    {
        Results.BadRequest(shortenedUrlResult.ErrorMessage);
    }

    var resultBuilder = new UriBuilder(host)
    {
        Path = $"/go/{shortenedUrlResult.ShortenedUrl}"
    }; 

    return Results.Ok(resultBuilder.Uri);
});


app.MapGet("/go/{shortenedRouteSegment:required}", static async(string shortenedRouteSegment, IUrlService urlService) =>
{
    var (Success, RedirectUrl) = await urlService.RedirectUrl(shortenedRouteSegment);
    if (!Success)
    {
        Results.NotFound();
    }

    return Results.Redirect(RedirectUrl);
});

app.MapControllers();

app.Run();
