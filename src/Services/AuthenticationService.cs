using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace UrlShortenerWithOrleans.Services
{
    public interface IAuthenticationService
    {
        string Authenticate();
        Task<bool> ValidateBearerToken();
        Task<bool> ValidateBearerToken(string bearerToken);
    }

    public class AuthenticationService: IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthenticationService(IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Authenticate()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(1),
              signingCredentials: credentials);

            var bearerToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return bearerToken;
        }

        public async Task<bool> ValidateBearerToken()
        {
            var bearerToken = _httpContextAccessor?.HttpContext?.Request?.Headers?.Authorization.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                return false;
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
                return true;
            }

            return false;
        }

        public async Task<bool> ValidateBearerToken(string bearerToken)
        {
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                return false;
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
                return true;
            }

            return false;
        }
    }
}
