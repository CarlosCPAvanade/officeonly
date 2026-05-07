using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Options;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly OnlyOfficeOptions _onlyOfficeOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions, IOptions<OnlyOfficeOptions> onlyOfficeOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _onlyOfficeOptions = onlyOfficeOptions.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateUserToken(User user, string roleName)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, roleName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    public string GenerateOnlyOfficeToken(object payload)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_onlyOfficeOptions.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(payload))
            ?? new Dictionary<string, object?>();

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Expires = DateTime.UtcNow.AddMinutes(_onlyOfficeOptions.UrlExpirationMinutes),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

    public bool ValidateOnlyOfficeToken(string token)
    {
        return ValidateToken(token, _onlyOfficeOptions.JwtSecret, validateIssuer: false, validateAudience: false);
    }

    public string GenerateDownloadToken(Guid documentId, Guid? userId, DateTime expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_onlyOfficeOptions.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new("documentId", documentId.ToString())
        };

        if (userId.HasValue)
        {
            claims.Add(new Claim("userId", userId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryValidateDownloadToken(string token, Guid documentId, out Guid? userId)
    {
        userId = null;
        if (!ValidateToken(token, _onlyOfficeOptions.JwtSecret, validateIssuer: false, validateAudience: false, out var principal))
        {
            return false;
        }

        var documentIdClaim = principal?.Claims.FirstOrDefault(x => x.Type == "documentId")?.Value;
        if (!Guid.TryParse(documentIdClaim, out var parsedDocumentId) || parsedDocumentId != documentId)
        {
            return false;
        }

        var userIdClaim = principal?.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        return true;
    }

    public UserSummaryDto MapUser(User user, string roleName)
    {
        return new UserSummaryDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = roleName
        };
    }

    private bool ValidateToken(string token, string secret, bool validateIssuer, bool validateAudience)
    {
        return ValidateToken(token, secret, validateIssuer, validateAudience, out _);
    }

    private bool ValidateToken(string token, string secret, bool validateIssuer, bool validateAudience, out ClaimsPrincipal? principal)
    {
        principal = null;
        var handler = new JwtSecurityTokenHandler();

        try
        {
            principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = validateIssuer,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = validateAudience,
                ValidAudience = _jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
