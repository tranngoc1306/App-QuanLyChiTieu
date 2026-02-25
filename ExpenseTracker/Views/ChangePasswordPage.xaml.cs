using ExpenseTracker.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace ExpenseTracker.Views
{
    public partial class ChangePasswordPage : ContentPage
    {
        private DatabaseService _database;
        private AuthService _authService;
        private bool _isCurrentPasswordVisible = false;
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public ChangePasswordPage()
        {
            InitializeComponent();

            // Initialize services
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expensetracker.db3");
            _database = new DatabaseService(dbPath);
            _authService = new AuthService(_database);
        }

        private void OnToggleCurrentPasswordVisibility(object sender, EventArgs e)
        {
            _isCurrentPasswordVisible = !_isCurrentPasswordVisible;
            CurrentPasswordEntry.IsPassword = !_isCurrentPasswordVisible;
            CurrentPasswordIcon.Text = _isCurrentPasswordVisible ? "👁️" : "🔒";
        }

        private void OnToggleNewPasswordVisibility(object sender, EventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;
            NewPasswordEntry.IsPassword = !_isNewPasswordVisible;
            NewPasswordIcon.Text = _isNewPasswordVisible ? "👁️" : "🔒";
        }

        private void OnToggleConfirmPasswordVisibility(object sender, EventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
            ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
            ConfirmPasswordIcon.Text = _isConfirmPasswordVisible ? "👁️" : "🔒";
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            try
            {
                var currentPassword = CurrentPasswordEntry.Text?.Trim();
                var newPassword = NewPasswordEntry.Text?.Trim();
                var confirmPassword = ConfirmPasswordEntry.Text?.Trim();

                // Validation
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("EnterCurrentPassword"),
                        LocalizationService.GetString("OK"));
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("EnterNewPassword"),
                        LocalizationService.GetString("OK"));
                    return;
                }

                if (newPassword.Length < 6)
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("PasswordMinLength"),
                        LocalizationService.GetString("OK"));
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("PasswordNotMatch"),
                        LocalizationService.GetString("OK"));
                    return;
                }

                if (currentPassword == newPassword)
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("PasswordSameAsCurrent"),
                        LocalizationService.GetString("OK"));
                    return;
                }

                ChangePasswordButton.IsEnabled = false;
                ChangePasswordButton.Text = "Đang xử lý...";

                // Get current user
                await _database.InitializeDatabaseAsync();
                var userId = Preferences.Get("UserId", 0);
                var userEmail = Preferences.Get("UserEmail", "");

                Debug.WriteLine($"🔵 UserId: {userId}, Email: {userEmail}");
                Debug.WriteLine($"🔵 Current Password: {currentPassword}");

                // Verify current password using AuthService
                var loginResult = await _authService.LoginAsync(userEmail, currentPassword);
                Debug.WriteLine($"🔵 Login Result: {loginResult.Success}, Message: {loginResult.Message}");

                if (!loginResult.Success)
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("PasswordIncorrect"),
                        LocalizationService.GetString("OK"));
                    ChangePasswordButton.IsEnabled = true;
                    ChangePasswordButton.Text = "Đổi mật khẩu";
                    return;
                }

                // Get user and update password
                var user = await _database.GetUserAsync(userId);
                if (user != null)
                {
                    // Hash new password using the same method as AuthService
                    user.PasswordHash = HashPassword(newPassword);
                    await _database.SaveUserAsync(user);

                    Debug.WriteLine($"✅ Password changed successfully");

                    await DisplayAlert(
                        LocalizationService.GetString("Success"),
                        LocalizationService.GetString("PasswordChangeSuccess"),
                        LocalizationService.GetString("OK"));

                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        "Không tìm thấy thông tin người dùng",
                        LocalizationService.GetString("OK"));
                }

                ChangePasswordButton.IsEnabled = true;
                ChangePasswordButton.Text = "Đổi mật khẩu";
            }
            catch (Exception ex)
            {
                ChangePasswordButton.IsEnabled = true;
                ChangePasswordButton.Text = "Đổi mật khẩu";
                Debug.WriteLine($"🔴 ChangePassword Error: {ex.Message}");
                Debug.WriteLine($"🔴 Stack Trace: {ex.StackTrace}");
                await DisplayAlert(
                    LocalizationService.GetString("Error"),
                    $"Không thể đổi mật khẩu: {ex.Message}",
                    LocalizationService.GetString("OK"));
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}