using LibrarySystem.API.Dtos.AuthDtos;
using LibrarySystem.API.Dtos.ErrorDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private ErrorDto CreateModelStateErrorResponse()
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return new ErrorDto
        {
            Status = "400 Bad Request",
            Message = "Gönderilen verilerde format hatası var. Lütfen tüm alanları kontrol edin.",
            Errors = errors
        };
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateModelStateErrorResponse());
        }

        try
        {
            var result = await _authService.RegisterAsync(registerDto);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorDto
            {
                Status = "400 Bad Request",
                Message = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto
            {
                Status = "500 Internal Server Error",
                Message = "Beklenmeyen bir sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin."
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateModelStateErrorResponse());
        }

        try
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Unauthorized(new ErrorDto
            {
                Status = "401 Unauthorized",
                Message = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto
            {
                Status = "500 Internal Server Error",
                Message = "Beklenmeyen bir sunucu hatası oluştu."
            });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto tokenRequestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateModelStateErrorResponse());
        }

        try
        {
            var result = await _authService.RefreshTokenAsync(tokenRequestDto.Token, tokenRequestDto.RefreshToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Unauthorized(new ErrorDto
            {
                Status = "401 Unauthorized",
                Message = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto
            {
                Status = "500 Internal Server Error",
                Message = "Token yenilenirken beklenmeyen bir hata oluştu."
            });
        }
    }
}