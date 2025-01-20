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
}