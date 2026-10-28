using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsApi.Auth;
using ProductsApi.Data;

namespace ProductsApi.Controllers;

public record AuthRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<IdentityUser> users,
    SignInManager<IdentityUser> signIn,
    TokenService tokens,
    AppDbContext db) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        await users.AddToRoleAsync(user, "User");
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(AuthRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized();

        var check = await signIn.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: true);
        if (!check.Succeeded) return Unauthorized();

        return await tokens.CreateTokensAsync(user);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest request)
    {
        var hash = TokenService.Hash(request.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t =>
            t.TokenHash == hash && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
        if (stored is null) return Unauthorized();

        stored.RevokedAt = DateTime.UtcNow;

        var user = await users.FindByIdAsync(stored.UserId);
        if (user is null) return Unauthorized();

        return await tokens.CreateTokensAsync(user);
    }
}
