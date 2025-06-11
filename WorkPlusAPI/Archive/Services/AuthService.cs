using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkPlusAPI.Archive.Data.Workplus;
using WorkPlusAPI.Archive.Models.Auth;
using WorkPlusAPI.Archive.Models.WorkPlus;

namespace WorkPlusAPI.Archive.Services;

public interface IAuthService
{
    Task<AuthResponse?> Login(LoginRequest request);
    Task<AuthResponse?> Register(RegisterRequest request);
    Task<bool> UpdatePassword(UpdatePasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly LoginWorkPlusContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(LoginWorkPlusContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse?> Login(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        try
        {
            // User lookup
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", request.Username);
                return null;
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", request.Username);
                return null;
            }

            // Generate token
            var roles = user.Roles.Select(r => r.Name).ToList();
            var token = GenerateJwtToken(user);
            
            _logger.LogInformation("Login successful for user: {Username}", request.Username);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = token,
                Roles = roles
            };
        }
        catch (MySqlConnector.MySqlException dbEx)
        {
            _logger.LogError(dbEx, "Database error during login for user: {Username}", request.Username);
            throw new Exception($"Database connection error: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login process for user: {Username}", request.Username);
            throw new Exception($"Login error: {ex.Message}");
        }
    }

    public async Task<AuthResponse?> Register(RegisterRequest request)
    {
        _logger.LogInformation("Attempting to register user: {Username}", request.Username);

        if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
        {
            _logger.LogWarning("Registration failed: Username or email already exists: {Username}", request.Username);
            return null;
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        // Assign roles based on username
        if (request.Username.ToLower() == "admintest")
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                user.Roles.Add(adminRole);
                _logger.LogInformation("Assigned Admin role to user: {Username}", request.Username);
            }
            else
            {
                _logger.LogWarning("Admin role not found in database");
            }
        }
        else
        {
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null)
            {
                user.Roles.Add(userRole);
                _logger.LogInformation("Assigned User role to user: {Username}", request.Username);
            }
            else
            {
                _logger.LogWarning("User role not found in database");
            }
        }

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully registered user: {Username}", request.Username);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = GenerateJwtToken(user),
                Roles = user.Roles.Select(r => r.Name).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Username}", request.Username);
            throw;
        }
    }

    public async Task<bool> UpdatePassword(UpdatePasswordRequest request)
    {
        _logger.LogInformation("Password update request for user ID: {UserId}", request.UserId);

        try
        {
            var user = await _context.Users.FindAsync(request.UserId);
            
            if (user == null)
            {
                _logger.LogWarning("Password update failed - User not found: {UserId}", request.UserId);
                return false;
            }

            // Update the password hash
            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Password successfully updated for user: {Username} (ID: {UserId})", 
                user.Username, user.Id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user ID: {UserId}", request.UserId);
            throw;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(1);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        byte[] salt = Encoding.UTF8.GetBytes("WorkPlusStaticSalt123!@#");
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        byte[] salt = Encoding.UTF8.GetBytes("WorkPlusStaticSalt123!@#");
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        var computedHash = Convert.ToBase64String(hash);
        return computedHash == storedHash;
    }
}