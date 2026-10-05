using System.Security.Cryptography;
using System.Text;

namespace RealTimeTaskManagement.Application.Projects;

public static class ProjectInviteCode
{
    public static string Normalize(string value)
    {
        return string.Concat(value
            .Where(char.IsLetterOrDigit))
            .ToUpperInvariant();
    }

    public static string Hash(string value)
    {
        var normalizedCode = Normalize(value);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedCode));
        return Convert.ToHexString(hash);
    }
}
