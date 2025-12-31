<<<<<<< HEAD
﻿using System.IdentityModel.Tokens.Jwt;
=======
using System.IdentityModel.Tokens.Jwt;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
using System.Security.Claims;
using System.Text;
using Chatup.DTOs;
using Chatup.Entities;
<<<<<<< HEAD
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
=======
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
using StackExchange.Redis;

namespace Chatup.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly IDatabase _redis;
    private readonly IConfiguration _config;
<<<<<<< HEAD

=======
    
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    public AuthService(IMongoDatabase database, IConnectionMultiplexer redis, IConfiguration config)
    {
        _users = database.GetCollection<User>("Users");
        _redis = redis.GetDatabase();
        _config = config;
<<<<<<< HEAD


=======
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    }

    public async Task SendOtpAsync(string phoneNumber)
    {
        var code = new Random().Next(100000, 999999).ToString();
<<<<<<< HEAD

        await _redis.StringSetAsync($"otp:{phoneNumber}", code, TimeSpan.FromMinutes((5)));

=======
        
        await _redis.StringSetAsync($"otp:{phoneNumber}", code, TimeSpan.FromMinutes((5)));
        
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n===============================================");
        Console.WriteLine($"[DEV MODE] OTP for {phoneNumber}: {code}");
        Console.WriteLine($"===============================================\n");
        Console.ResetColor();
<<<<<<< HEAD

       
    }


=======
    }

>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    public async Task<AuthResponse> VerifyOtpAsync(string phoneNumber, string code)
    {
        var storedCode = await _redis.StringGetAsync($"otp:{phoneNumber}");

        if (storedCode.IsNullOrEmpty || storedCode != code)
        {
            throw new Exception("Invalid or expired code");
        }

        var user = await _users.Find(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync();

        if (user == null)
<<<<<<< HEAD
        {
            user = new User { PhoneNumber = phoneNumber };
            await _users.InsertOneAsync(user);
        }

        await _redis.KeyDeleteAsync($"otp:{phoneNumber}");
=======
        { 
            user = new User { PhoneNumber = phoneNumber };
            await _users.InsertOneAsync(user);
        }
        
        await _redis.KeyDeleteAsync($"opt:{phoneNumber}");
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be

        var token = GenerateJwtToken(user);

        return new AuthResponse(token, user.Id, user.Nickname);
    }

    private string GenerateJwtToken(User user)
    {
<<<<<<< HEAD
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
=======
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
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    }
}