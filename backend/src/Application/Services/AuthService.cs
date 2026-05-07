using Application.DTOs.Auth;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditService _auditService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _auditService = auditService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUserNameAsync(request.UserName.Trim(), cancellationToken);
        if (user?.Role == null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Credenciales inválidas.");
        }

        var (token, expiresAtUtc) = _jwtTokenService.GenerateUserToken(user, user.Role.Name);

        await _auditService.WriteAsync(
            user.Id,
            null,
            AuditActionType.Login,
            $"Inicio de sesión del usuario {user.UserName}",
            new { user.UserName, user.Email },
            ipAddress,
            cancellationToken);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = _jwtTokenService.MapUser(user, user.Role.Name)
        };
    }
}
