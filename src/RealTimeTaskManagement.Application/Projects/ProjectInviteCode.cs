using System.Security.Cryptography;
using System.Text;

namespace RealTimeTaskManagement.Application.Projects;

public static class ProjectInviteCode
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate()
    {
        var characters = new char[12];
        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = CodeAlphabet[
                RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        }

        return $"TFLW-{new string(characters, 0, 4)}-{new string(characters, 4, 4)}-{new string(characters, 8, 4)}";
    }

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
