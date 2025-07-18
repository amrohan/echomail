using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EmailRelayServer.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    public AuthService(IConfiguration config) => _config = config;

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public string GenerateJwtToken(string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "emailrelay",
            audience: "emailrelay",
            claims: new[] { new Claim(ClaimTypes.Email, email) },
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
