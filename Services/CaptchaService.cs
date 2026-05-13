using Microsoft.AspNetCore.DataProtection;

namespace Work_Dashboard.Services;

public class CaptchaService
{
    private readonly IDataProtector _protector;
    private static readonly Random _rng = new();

    public CaptchaService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("BDA.LoginCaptcha.v1");
    }

    public (string Question, string Token) Generate()
    {
        var a = _rng.Next(1, 10);
        var b = _rng.Next(1, 10);
        var answer = a + b;
        var expiresUnix = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        var payload = $"{answer}|{expiresUnix}";
        var token = _protector.Protect(payload);
        return ($"{a} + {b}", token);
    }

    public bool Validate(string? token, string? userInput)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userInput))
            return false;
        if (!int.TryParse(userInput.Trim(), out var userAnswer))
            return false;

        try
        {
            var payload = _protector.Unprotect(token);
            var parts = payload.Split('|');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out var expected)) return false;
            if (!long.TryParse(parts[1], out var expiresUnix)) return false;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresUnix) return false;
            return userAnswer == expected;
        }
        catch
        {
            return false;
        }
    }
}
