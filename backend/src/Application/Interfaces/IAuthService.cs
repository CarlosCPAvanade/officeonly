using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
}
