using Microsoft.AspNetCore.Mvc;
using ChatApp.Models;
using ChatApp.Services;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Логин и пароль обязательны" 
                });
            }

            if (request.Login.Length < 3)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Логин должен содержать минимум 3 символа" 
                });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Пароль должен содержать минимум 6 символов" 
                });
            }

            var result = await _authService.RegisterAsync(request);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Логин и пароль обязательны" 
                });
            }

            var result = await _authService.LoginAsync(request);
            
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
    }
}