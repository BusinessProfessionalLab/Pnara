using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    TokenResult GenerateToken(User user);
    string GenerateRefreshToken();
}
