using System.Security.Cryptography;
using System.Text;

namespace StaySync.Application.Features.Invites;

public static class InviteTokenHelper
{
    /// <summary>Generates a cryptographically secure URL-safe base64 token (32 bytes → 43 chars).</summary>
    public static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>Returns the SHA-256 hex hash of a raw token for safe storage.</summary>
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
