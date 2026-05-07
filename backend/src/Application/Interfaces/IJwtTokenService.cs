using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateUserToken(User user, string roleName);
    string GenerateOnlyOfficeToken(object payload);
    bool ValidateOnlyOfficeToken(string token);
    string GenerateDownloadToken(Guid documentId, Guid? userId, DateTime expiresAtUtc);
    bool TryValidateDownloadToken(string token, Guid documentId, out Guid? userId);
    UserSummaryDto MapUser(User user, string roleName);
}
