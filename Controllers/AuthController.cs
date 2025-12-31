<<<<<<< HEAD
﻿using Chatup.DTOs;
=======
using Chatup.DTOs;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
using Chatup.Services;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Chatup.DTOs.LoginRequest;

namespace Chatup.Controllers;

[ApiController]
<<<<<<< HEAD
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

=======
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService; 
    
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        await _authService.SendOtpAsync(request.PhoneNumber);
        return Ok(new { message = "OTP sent. Check server console." });
    }

<<<<<<< HEAD

=======
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var response = await _authService.VerifyOtpAsync(request.PhoneNumber, request.Code);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}