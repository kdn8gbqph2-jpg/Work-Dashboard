using Microsoft.EntityFrameworkCore;
using Work_Dashboard.Data;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Services;

public class AuthService(BdaDbContext db) : IAuthService
{
    public async Task<Engineer?> ValidateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var engineer = await db.Engineers
            .FirstOrDefaultAsync(e =>
                e.Username == username.Trim() &&
                e.IsActive &&
                !e.IsDeleted);

        if (engineer is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, engineer.PasswordHash)) return null;

        return engineer;
    }
}
