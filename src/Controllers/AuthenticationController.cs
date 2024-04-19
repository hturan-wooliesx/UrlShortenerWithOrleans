using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using UrlShortenerWithOrleans.Services;

namespace UrlShortenerWithOrleans.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IConfiguration configuration, 
            IAuthenticationService authenticationService)
        {
            _configuration = configuration;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IResult GetBearerToken()
        {
            var bearerToken = _authenticationService.Authenticate();

            return Results.Ok(bearerToken);
        }

        [HttpGet]
        [Route("validate")]
        public async Task<IResult> ValidateBearerToken()
        {
            var bearerToken = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(bearerToken))
            {
                return Results.NotFound("bearer token not found");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = bearerToken.Remove(0, 6).Trim();
            var validationResult = await tokenHandler
                .ValidateTokenAsync(token, new TokenValidationParameters() 
            {           
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidAudience = _configuration["Jwt:Issuer"],
                ValidIssuer = _configuration["Jwt:Issuer"],
                RequireExpirationTime = true
            });
            
            if (validationResult.IsValid)
            {
                return Results.Ok("valid");
            }

            return Results.Ok(validationResult?.Exception?.Message ?? validationResult?.Exception?.ToString());        
        }
    }
}
