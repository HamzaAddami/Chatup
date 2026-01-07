using Chatup.DTOs;
using Chatup.Services;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Chatup.DTOs.LoginRequest;

namespace Chatup.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        await _authService.SendOtpAsync(request.PhoneNumber);
        return Ok(new { message = "OTP sent. Check server console." });
    }


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

    [HttpPost("firebase-login")]
    public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
    {
        try
        {
            var response = await _authService.LoginWithFirebaseAsync(request.FirebaseToken);
            return Ok(response);
        }
        catch (Exception e)
        {
            return Unauthorized(new { error = e.Message });
        }
    }
}