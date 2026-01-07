using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Chatup.DTOs;
using Chatup.Entities;
using FirebaseAdmin.Auth;
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
        
        await _redis.StringSetAsync($"otp:{phoneNumber}", code, TimeSpan.FromMinutes(5));
        
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
        
        await _redis.KeyDeleteAsync($"otp:{phoneNumber}");

        var token = GenerateJwtToken(user);

        return new AuthResponse(token, user.Id, user.Nickname, user.Email);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
        }

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
           
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
       
        return tokenHandler.WriteToken(token);
    }
    
    public async Task<AuthResponse> LoginWithFirebaseAsync(string firebaseIdToken)
    {
        FirebaseToken decodedToken;
        
        try
        {
            decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(firebaseIdToken);
        }
        catch (Exception)
        {
            throw new Exception("Invalid Firebase Token");
        }

        string uid = decodedToken.Uid;
        string? email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;

        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        if (user == null)
        {
            user = new User 
            { 
                Email = email,
                Nickname = email?.Split('@')[0]
            };
            await _users.InsertOneAsync(user);
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse(token, user.Id, user.Nickname, user.Email);
    }
}