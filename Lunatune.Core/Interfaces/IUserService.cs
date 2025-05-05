using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByAuth0IdAsync(string auth0Id);
    Task<User> CreateUserAsync(User user);
    Task<bool> UserExistsAsync(string auth0Id);
}

