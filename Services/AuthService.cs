using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Chatup.DTOs;
using Chatup.Entities;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Chatup.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly IDatabase _redis;
    private readonly IConfiguration _config;
    
    public AuthService(IMongoDatabase database, IConnectionMultiplexer redis, IConfiguration config)
    {
        _users = database.GetCollection<User>("Users");
        _redis = redis.GetDatabase();
        _config = config;
    }

    public async Task SendOtpAsync(string phoneNumber)
    {
        var code = new Random().Next(100000, 999999).ToString();
        
        await _redis.StringSetAsync($"otp:{phoneNumber}", code, TimeSpan.FromMinutes((5)));
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n===============================================");
        Console.WriteLine($"[DEV MODE] OTP for {phoneNumber}: {code}");
        Console.WriteLine($"===============================================\n");
        Console.ResetColor();
    }

    public async Task<AuthResponse> VerifyOtpAsync(string phoneNumber, string code)
    {
        var storedCode = await _redis.StringGetAsync($"otp:{phoneNumber}");

        if (storedCode.IsNullOrEmpty || storedCode != code)
        {
            throw new Exception("Invalid or expired code");
        }

        var user = await _users.Find(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync();

        if (user == null)
        { 
            user = new User { PhoneNumber = phoneNumber };
            await _users.InsertOneAsync(user);
        }
        
        await _redis.KeyDeleteAsync($"opt:{phoneNumber}");

        var token = GenerateJwtToken(user);

        return new AuthResponse(token, user.Id, user.Nickname);
    }

    private string GenerateJwtToken(User user)
    {
       var jwtSettings = _config.GetSection("JwtSettings");
       var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

       var tokenDescriptor = new SecurityTokenDescriptor
       {
           Subject = new ClaimsIdentity([
               new Claim(ClaimTypes.NameIdentifier, user.Id),
               new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
           ]),
           
           Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
           Issuer = jwtSettings["Issuer"],
           Audience = jwtSettings["Audience"],
           SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
       };

       var tokenHandler = new JwtSecurityTokenHandler();
       var token = tokenHandler.CreateToken(tokenDescriptor);
       
       return tokenHandler.WriteToken(token);
    }
}