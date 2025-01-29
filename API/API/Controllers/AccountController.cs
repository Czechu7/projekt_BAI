using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{

    private static readonly Dictionary<string, (int attempts, DateTime lastAttempt)> _loginAttempts = 
        new Dictionary<string, (int attempts, DateTime lastAttempt)>();
    private const int MaxAttempts = 5;
    private const int LockoutMinutes = 15;
    [HttpPost("register")] // account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => 
            x.UserName == loginDto.Username.ToLower());

        if (user == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

[HttpPost("loginBruteForce")]
public async Task<ActionResult<UserDto>> LoginBruteForce(LoginDto loginDto)
{
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
    var key = $"{loginDto.Username.ToLower()}_{ipAddress}";

    // Sprawdzenie czy użytkownik jest zablokowany
    if (_loginAttempts.ContainsKey(key))
    {
        var (attempts, lastAttempt) = _loginAttempts[key];
        var timeSinceLastAttempt = DateTime.UtcNow - lastAttempt;

        if (attempts >= MaxAttempts)
        {
            if (timeSinceLastAttempt.TotalMinutes < LockoutMinutes)
            {
                var remainingMinutes = LockoutMinutes - (int)timeSinceLastAttempt.TotalMinutes;
                return Unauthorized($"Konto zostało tymczasowo zablokowane. Spróbuj ponownie za {remainingMinutes} minut.");
            }
            _loginAttempts.Remove(key);
        }
    }

    var user = await context.Users.FirstOrDefaultAsync(x => 
        x.UserName == loginDto.Username.ToLower());

    // Sprawdzamy, czy użytkownik istnieje, ale nie informujemy, czy to problem z nazwą użytkownika czy hasłem
    if (user == null || !VerifyPassword(user, loginDto.Password))
    {
        UpdateLoginAttempts(key);
        return Unauthorized("Nieprawidłowe dane logowania.");
    }

    // Udane logowanie - reset licznika
    if (_loginAttempts.ContainsKey(key))
    {
        _loginAttempts.Remove(key);
    }

    return new UserDto
    {
        Username = user.UserName,
        Token = tokenService.CreateToken(user)
    };
}

private bool VerifyPassword(AppUser user, string password)
{
    using var hmac = new HMACSHA512(user.PasswordSalt);
    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

    for (int i = 0; i < computedHash.Length; i++)
    {
        if (computedHash[i] != user.PasswordHash[i])
            return false;
    }

    return true;
}

    [HttpPost("login-insecure")]
    public async Task<ActionResult<UserDto>> LoginInsecure(LoginDto loginDto)
    {
        //' OR 1=1 --'

        var query = $"SELECT * FROM Users WHERE UserName = '{loginDto.Username.ToLower()}'";
        var user = await context.Users.FromSqlRaw(query).FirstOrDefaultAsync();

        if (user == null) return Unauthorized("Invalid username");

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }


    private async Task<bool> UserExists(string username) 
    {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower()); 
    }

    [HttpPost("stealToken")]
    public IActionResult StealToken([FromBody] StealTokenDto dto )
    {
        return Ok(new { message = "ukradlem ci token", token = dto.Token });

    }


    [HttpPost("setAvatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar, [FromForm] string userId)
    {
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (avatar == null || string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("Invalid request. Avatar or user ID is missing.");
        }

        
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

    
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

    
        var filePath = Path.Combine(uploadsFolder, avatar.FileName);

        try
        {
         
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

          
            user.Avatar = $"/uploads/{avatar.FileName}";
            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "Avatar uploaded and user updated successfully",
                avatarUrl = user.Avatar
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


 [HttpPost("setAvatarSecure")]
public async Task<IActionResult> UploadAvatarSecure([FromForm] IFormFile avatar, [FromForm] string userId)
{
    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    if (avatar == null || string.IsNullOrWhiteSpace(userId))
    {
        return BadRequest("Invalid request. Avatar or user ID is missing.");
    }

    // Sprawdzamy rozszerzenie pliku
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
    var fileExtension = Path.GetExtension(avatar.FileName).ToLower();

    if (!allowedExtensions.Contains(fileExtension))
    {
        return BadRequest("Invalid file type. Only jpg, jpeg, and png files are allowed.");
    }

    var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
    if (user == null)
    {
        return NotFound("User not found.");
    }

    if (!Directory.Exists(uploadsFolder))
    {
        Directory.CreateDirectory(uploadsFolder);
    }

    var filePath = Path.Combine(uploadsFolder, avatar.FileName);

    try
    {
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        user.Avatar = $"/uploads/{avatar.FileName}";
        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Avatar uploaded and user updated successfully",
            avatarUrl = user.Avatar
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}

    
private void UpdateLoginAttempts(string key)
    {
        if (_loginAttempts.ContainsKey(key))
        {
            var (attempts, _) = _loginAttempts[key];
            _loginAttempts[key] = (attempts + 1, DateTime.UtcNow);
        }
        else
        {
            _loginAttempts.Add(key, (1, DateTime.UtcNow));
        }

}
}
