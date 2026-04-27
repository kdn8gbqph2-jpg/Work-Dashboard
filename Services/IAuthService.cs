using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Services;

public interface IAuthService
{
    Task<Engineer?> ValidateAsync(string username, string password);
}
