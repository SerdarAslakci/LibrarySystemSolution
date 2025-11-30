using LibrarySystem.API.Dtos.AuthDtos;
using LibrarySystem.API.Dtos.ErrorDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; 
using System;
using System.Linq;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
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
        _logger.LogInformation("Kayıt olma isteği alındı. Email: {Email}", registerDto?.Email);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Kayıt olma isteği validasyona takıldı. Email: {Email}", registerDto?.Email);
            return BadRequest(CreateModelStateErrorResponse());
        }

        try
        {
            var result = await _authService.RegisterAsync(registerDto);

            _logger.LogInformation("Kayıt işlemi başarılı. Kullanıcı: {UserName}", result.UserName);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Kayıt işlemi başarısız (İş kuralı hatası). Email: {Email}", registerDto?.Email);
            return BadRequest(new ErrorDto
            {
                Status = "400 Bad Request",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt işlemi sırasında beklenmeyen hata. Email: {Email}", registerDto?.Email);
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
        _logger.LogInformation("Giriş isteği alındı. Email: {Email}", loginDto?.Email);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Giriş isteği validasyona takıldı. Email: {Email}", loginDto?.Email);
            return BadRequest(CreateModelStateErrorResponse());
        }

        try
        {
            var result = await _authService.LoginAsync(loginDto);

            _logger.LogInformation("Giriş başarılı. Kullanıcı: {UserName}", result.UserName);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Giriş başarısız (Yetkisiz): {Message}. Email: {Email}", ex.Message, loginDto?.Email);
            return Unauthorized(new ErrorDto
            {
                Status = "401 Unauthorized",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş işlemi sırasında sunucu hatası. Email: {Email}", loginDto?.Email);
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
        _logger.LogInformation("Token yenileme isteği alındı.");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Token yenileme isteği validasyona takıldı.");
            return BadRequest(CreateModelStateErrorResponse());
        }

        try
        {
            var result = await _authService.RefreshTokenAsync(tokenRequestDto.Token, tokenRequestDto.RefreshToken);

            _logger.LogInformation("Token başarıyla yenilendi. Kullanıcı: {UserName}", result.UserName);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Token yenileme başarısız (Yetkisiz): {Message}", ex.Message);
            return Unauthorized(new ErrorDto
            {
                Status = "401 Unauthorized",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token yenileme sırasında sunucu hatası.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto
            {
                Status = "500 Internal Server Error",
                Message = "Token yenilenirken beklenmeyen bir hata oluştu."
            });
        }
    }
}