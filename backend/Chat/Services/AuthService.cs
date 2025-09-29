using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore; 
using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                if (await UserExistsAsync(request.Login))
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Пользователь с таким логином уже существует" 
                    };
                }

                var passwordHash = HashPassword(request.Password);

                var user = new User
                {
                    Login = request.Login,
                    Password = passwordHash,
                    CreatedAt = DateTime.Now 
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new AuthResponse 
                { 
                    Success = true, 
                    Message = "Регистрация успешна",
                    User = new UserData
                    {
                        Id = user.Id,
                        Login = user.Login,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Ошибка регистрации: {ex.Message}" 
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Login == request.Login);

                if (user == null || !VerifyPassword(request.Password, user.Password))
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Неверный логин или пароль" 
                    };
                }

                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new AuthResponse 
                { 
                    Success = true, 
                    Message = "Вход выполнен успешно",
                    User = new UserData
                    {
                        Id = user.Id,
                        Login = user.Login,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Ошибка входа: {ex.Message}" 
                };
            }
        }

        public async Task<bool> UserExistsAsync(string login)
        {
            return await _context.Users.AnyAsync(u => u.Login == login);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}