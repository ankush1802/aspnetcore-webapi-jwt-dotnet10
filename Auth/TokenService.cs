using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ProductsApi.Data;
using ProductsApi.Models;

namespace ProductsApi.Auth;

public record TokenResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);

public class TokenService(
    IConfiguration config,
    AppDbContext db,
    UserManager<IdentityUser> users)
{
    public async Task<TokenResponse> CreateTokensAsync(IdentityUser user)
    {
        var jwt = config.GetSection("Jwt");

        var claims = new List<Claim>
        {
            new("sub", user.Id),
            new("email", user.Email!)
        };
        foreach (var role in await users.GetRolesAsync(user))
            claims.Add(new Claim("role", role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var expires = DateTime.UtcNow.AddMinutes(jwt.GetValue<int>("AccessTokenMinutes"));

        var accessToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        });

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(jwt.GetValue<int>("RefreshTokenDays"))
        });
        await db.SaveChangesAsync();

        return new TokenResponse(accessToken, expires, refreshToken);
    }

    public static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
