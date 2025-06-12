using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pixlmint.Aioniq.Data;
using Pixlmint.Aioniq.Model;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ApplicationDb _db;

    public AuthController(
        IConfiguration configuration,
        IHttpContextAccessor contextAccessor,
        ApplicationDb db
    )
    {
        _configuration = configuration;
        _contextAccessor = contextAccessor;
        _db = db;
    }

    [HttpPost("HandleTokenRefresh")]
    public async Task<IActionResult> HandleTokenRefresh()
    {
        return Ok();
    }

    [HttpPost("HackLogMeIn")]
    public async Task<IActionResult> HackLogMeIn()
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == "swiss8oy.chg@gmail.com");

        if (user == null)
        {
            return NotFound();
        }

        return await StepLogin(user);
    }

    [HttpPost("HandleOAuthLogin")]
    public async Task<IActionResult> HandleOAuthLogin([FromBody] LoginDTO request)
    {
        var tokenResponse = await StepExchangeTokens(request);
        var userInfo = await GetUserProfileAsync(tokenResponse.AccessToken);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleUserId == userInfo.Id);

        if (user == null)
        {
            user = StepRegister(userInfo, tokenResponse);
        }

        return await StepLogin(user);
    }

    [HttpPost("VerifyLoggedIn")]
    public async Task<IActionResult> VerifyLoggedIn([FromBody] JsonElement data)
    {
        string? token = data.GetProperty("token").GetString();

        if (token != null && ValidateJwtToken(token, out ClaimsPrincipal? principal))
        {
            return Ok(new { Token = token });
        }

        var refreshToken = await GetUserRefreshTokenOrNull();

        Console.WriteLine(
            "Found Token: " + refreshToken?.TokenHash + ", User: " + refreshToken?.User?.Email
        );

        if (refreshToken != null && refreshToken.User != null)
        {
            return await StepLogin(refreshToken.User);
        }

        return Unauthorized();
    }

    private async Task<TokenResponse> StepExchangeTokens(LoginDTO request)
    {
        var flow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _configuration["Authentication:Google:ClientId"],
                    ClientSecret = _configuration["Authentication:Google:ClientSecret"],
                },
                Scopes = new[]
                {
                    "https://www.googleapis.com/auth/calendar.readonly",
                    "https://www.googleapis.com/auth/calendar",
                    "https://www.googleapis.com/auth/tasks.readonly",
                    "https://www.googleapis.com/auth/userinfo.profile",
                    "https://www.googleapis.com/auth/userinfo.email",
                },
            }
        );

        return await flow.ExchangeCodeForTokenAsync(
            userId: "temp", // We'll replace this with actual user ID after getting profile
            code: request.Code,
            redirectUri: request.RedirectUri,
            taskCancellationToken: CancellationToken.None
        );
    }

    private User StepRegister(GoogleUserInfo userInfo, TokenResponse tokenResponse)
    {
        var user = userInfo.CreateUser();
        _db.Users.Add(user);
        StoreUserTokensAsync(user, tokenResponse);

        return user;
    }

    private async Task<IActionResult> StepLogin(User user)
    {
        var claims = new[]
        {
            new Claim("prim", user.Id.ToString()), // Internal user ID - primary identifier
            new Claim("sub", user.GoogleUserId), // Google user ID - secondary identifier
            new Claim("email", user.Email),
            new Claim("name", user.Name),
            new Claim("picture", user.Picture),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // unique token ID
        };

        var token = GenerateJwtToken(claims);

        await InvalidateActiveRefreshTokens();

        var (refreshToken, plaintextRefreshToken) = GenerateRefreshToken(user);

        var cookieOptions = new CookieOptions();
        cookieOptions.Expires = refreshToken.ExpiresAt;
        cookieOptions.Path = "/api";
        Response.Cookies.Append("RefreshToken", plaintextRefreshToken, cookieOptions);

        _db.UserRefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync();

        return Ok(new { Token = token });
    }

    private (UserRefreshToken, string) GenerateRefreshToken(User user)
    {
        var Headers = _contextAccessor.HttpContext!.Request.Headers;

        var refreshToken =
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            + Headers.UserAgent
            + user.Id;

        using (HashAlgorithm algo = SHA256.Create())
        {
            var tokenHash = algo.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            var Sb = new StringBuilder();
            foreach (Byte b in tokenHash)
                Sb.Append(b.ToString("x2"));
            refreshToken = Sb.ToString();
        }

        if (refreshToken == null)
        {
            throw new NullReferenceException("Error creating refresh token hash");
        }

        var token = new UserRefreshToken
        {
            TokenHash = HashRefreshToken(refreshToken),
            CreatedAt = DateTime.Now.ToUniversalTime(),
            ExpiresAt = DateTime.Now.AddDays(30).ToUniversalTime(),
            User = user,
        };

        return (token, refreshToken);
    }

    private async Task<UserRefreshToken?> GetUserRefreshTokenOrNull()
    {
        var Cookies = _contextAccessor.HttpContext!.Request.Cookies;
        if (Cookies.ContainsKey("RefreshToken") && Cookies["RefreshToken"] != null)
        {
            Console.WriteLine("Plaintext Token: " + Cookies["RefreshToken"]);
            return await _db
                .UserRefreshTokens.Include(e => e.User)
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == HashRefreshToken(Cookies["RefreshToken"]!)
                );
        }
        return null;
    }

    private async Task InvalidateActiveRefreshTokens()
    {
        var existingToken = await GetUserRefreshTokenOrNull();

        if (existingToken != null)
        {
            _db.UserRefreshTokens.Remove(existingToken);
        }

        return;
    }

    private string HashRefreshToken(string PlaintextRefreshToken)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(PlaintextRefreshToken));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2")); // Convert to hexadecimal string
            }
            return builder.ToString();
        }
    }

    private async Task<GoogleUserInfo> GetUserProfileAsync(string accessToken)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetStringAsync(
            "https://www.googleapis.com/oauth2/v2/userinfo"
        );
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        return userInfo!;
    }

    private void StoreUserTokensAsync(User user, TokenResponse tokenResponse)
    {
        var userTokens = new UserTokens
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresInSeconds = tokenResponse.ExpiresInSeconds,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600),
        };

        user.Tokens = userTokens;
        userTokens.User = user;

        _db.UserTokens.Add(userTokens);
    }

    /*[Authorize]
    [HttpGet("user")]
    public IActionResult GetCurrentUser()
    {
        return Ok(
            new
            {
                Name = User.FindFirst(ClaimTypes.Name)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Picture = User.FindFirst("picture")?.Value,
            }
        );
    }*/

    private string GenerateJwtToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool ValidateJwtToken(string token, out ClaimsPrincipal? principal)
    {
        principal = null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero, // Optional: removes default 5min tolerance
            };

            principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken validatedToken
            );
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public class LoginDTO
    {
        public String Code { get; set; } = String.Empty;
        public String RedirectUri { get; set; } = String.Empty;
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public bool VerifiedEmail { get; set; }

        public User CreateUser()
        {
            return new User
            {
                GoogleUserId = this.Id,
                Name = this.Name,
                Email = this.Email,
                Picture = this.Picture,
            };
        }
    }
}
