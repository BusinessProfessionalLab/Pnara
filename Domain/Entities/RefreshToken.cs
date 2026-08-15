using Domain.Exceptions;

namespace Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken()
    {
    }

    private RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public static RefreshToken Create(Guid userId, string token, int expiryDays = 7)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Refresh token cannot be empty.");

        var expiresAt = DateTime.UtcNow.AddDays(expiryDays);
        return new RefreshToken(userId, token, expiresAt);
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
