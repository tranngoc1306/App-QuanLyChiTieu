using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public class AuthService
    {
        private readonly DatabaseService _database;

        public AuthService(DatabaseService database)
        {
            _database = database;
        }

        public async Task<(bool Success, string Message, User User)> RegisterAsync(
            string fullName, string email, string gender, DateTime dateOfBirth, string password)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Vui lòng nhập họ tên", null);

            if (string.IsNullOrWhiteSpace(email))
                return (false, "Vui lòng nhập email", null);

            if (!IsValidEmail(email))
                return (false, "Email không hợp lệ", null);

            if (string.IsNullOrWhiteSpace(gender))
                return (false, "Vui lòng chọn giới tính", null);

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Vui lòng nhập mật khẩu", null);

            if (password.Length < 6)
                return (false, "Mật khẩu phải có ít nhất 6 ký tự", null);

            // Check if email already exists
            var existingUser = await _database.GetUserByEmailAsync(email);
            if (existingUser != null)
                return (false, "Email đã được sử dụng", null);

            // Create new user
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Gender = gender,
                DateOfBirth = dateOfBirth,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.Now,
                LastLoginAt = DateTime.Now
            };

            await _database.SaveUserAsync(user);

            return (true, "Đăng ký thành công", user);
        }

        public async Task<(bool Success, string Message, User User)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Vui lòng nhập email", null);

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Vui lòng nhập mật khẩu", null);

            var user = await _database.GetUserByEmailAsync(email);
            if (user == null)
                return (false, "Email hoặc mật khẩu không đúng", null);

            if (!VerifyPassword(password, user.PasswordHash))
                return (false, "Email hoặc mật khẩu không đúng", null);

            user.LastLoginAt = DateTime.Now;
            await _database.SaveUserAsync(user);

            return (true, "Đăng nhập thành công", user);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
