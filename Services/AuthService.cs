using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Models;

namespace Sensore.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }

    public async Task<User> CreateUserAsync(string email, string fullName, string password, UserRole role)
    {
        var normalizedEmail = email.Trim();
        if (await _dbContext.Users.AnyAsync(u => u.Email == normalizedEmail))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            FullName = fullName.Trim(),
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public Task<List<User>> GetAllUsersAsync()
    {
        return _dbContext.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task SetUserActiveAsync(int userId, bool isActive)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return;
        }

        user.IsActive = isActive;
        await _dbContext.SaveChangesAsync();
    }
}
