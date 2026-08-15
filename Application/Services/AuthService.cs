using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IAuthCookieService authCookieService)
{
    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);

        if (await userRepository.ExistsByEmailAsync(email))
            throw new EmailAlreadyExistsException();

        var defaultRole = await roleRepository.GetByNameAsync(SystemRoles.User)
            ?? throw new InvalidOperationException("Default 'User' role not found in the system.");

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Register(email, passwordHash, request.FirstName, request.LastName, defaultRole.Id);

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return await CreateUserSessionAsync(user);
    }

    public async Task<UserResponse> LoginAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);

        var user = await userRepository.GetByEmailAsync(email);

        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new InvalidCredentialsException();

        return await CreateUserSessionAsync(user);
    }

    public async Task<UserResponse> RefreshTokenAsync()
    {
        var refreshToken = authCookieService.GetRefreshToken()
            ?? throw new InvalidCredentialsException();

        var storedToken = await refreshTokenRepository.GetByTokenAsync(refreshToken)
            ?? throw new InvalidCredentialsException();

        if (!storedToken.IsActive)
            throw new InvalidCredentialsException();

        var user = await userRepository.GetByIdAsync(storedToken.UserId)
            ?? throw new InvalidCredentialsException();

        if (!user.IsActive)
            throw new InvalidCredentialsException();

        storedToken.Revoke();
        await refreshTokenRepository.SaveChangesAsync();

        return await CreateUserSessionAsync(user);
    }

    public async Task LogoutAsync()
    {
        var refreshToken = authCookieService.GetRefreshToken();

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var storedToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (storedToken is not null && storedToken.IsActive)
            {
                storedToken.Revoke();
                await refreshTokenRepository.SaveChangesAsync();
            }
        }

        authCookieService.ClearTokenCookies();
    }

    public async Task<UserResponse> CreateUserByAdminAsync(CreateUserRequest request, User currentUser)
    {
        var allowedRoles = new[] { SystemRoles.Admin, SystemRoles.Operator };
        if (!allowedRoles.Contains(currentUser.Role.Name))
            throw new UnauthorizedAccessException("Only Admin and Operator roles can create users.");

        var targetRole = await roleRepository.GetByIdAsync(request.RoleId)
            ?? throw new RoleNotFoundException($"Role with id '{request.RoleId}' was not found.");

        var email = NormalizeEmail(request.Email);

        if (await userRepository.ExistsByEmailAsync(email))
            throw new EmailAlreadyExistsException();

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.CreateByAdmin(email, passwordHash, request.FirstName, request.LastName, targetRole.Id);

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return user.ToResponse();
    }

    private async Task<UserResponse> CreateUserSessionAsync(User user)
    {
        var token = tokenService.GenerateToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue);
        await refreshTokenRepository.AddAsync(refreshToken);
        await refreshTokenRepository.SaveChangesAsync();

        authCookieService.SetTokenCookies(token.Token, token.ExpiresAt, refreshTokenValue);

        return user.ToResponse();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
