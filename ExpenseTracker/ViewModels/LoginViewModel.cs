using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ExpenseTracker.Services;
using Microsoft.Maui.Controls;

namespace ExpenseTracker.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _email;
        private string _password;
        private bool _isPasswordVisible;
        private bool _isLoading;
        private string _errorMessage;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;

            LoginCommand = new Command(async () => await LoginAsync(), () => CanLogin);
            NavigateToRegisterCommand = new Command(async () => await NavigateToRegisterAsync());
            ForgotPasswordCommand = new Command(async () => await ForgotPasswordAsync());
            TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
        }

        #region Properties

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    ErrorMessage = string.Empty;
                }
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPasswordHidden));
                    OnPropertyChanged(nameof(PasswordVisibilityIcon));
                }
            }
        }

        public bool IsPasswordHidden => !IsPasswordVisible;

        public string PasswordVisibilityIcon => IsPasswordVisible ? "👁️" : "🔒";

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNotLoading));
                    OnPropertyChanged(nameof(CanLogin));
                    OnPropertyChanged(nameof(LoginButtonText));
                    ((Command)LoginCommand).ChangeCanExecute();
                }
            }
        }

        public bool IsNotLoading => !IsLoading;

        public bool CanLogin => !IsLoading;

        public string LoginButtonText => IsLoading ? "Đang đăng nhập..." : "Đăng Nhập";

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        #endregion

        #region Commands

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        #endregion

        #region Methods

        private async Task LoginAsync()
        {
            if (!ValidateInput())
                return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.LoginAsync(Email?.Trim(), Password);

                if (result.Success)
                {
                    SaveUserSession(result.User);
                    await NavigateToMainPage();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Đã xảy ra lỗi. Vui lòng thử lại.";
                System.Diagnostics.Debug.WriteLine($"🔴 Login Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Vui lòng nhập email";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu";
                return false;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Email không hợp lệ";
                return false;
            }

            return true;
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

        private void SaveUserSession(dynamic user)
        {
            Preferences.Set("UserId", user.Id);
            Preferences.Set("UserEmail", user.Email);
            Preferences.Set("UserName", user.FullName);
        }

        private async Task NavigateToMainPage()
        {
            Application.Current.MainPage = new AppShell();
        }

        private async Task NavigateToRegisterAsync()
        {
            // Sử dụng Navigation.PushAsync thay vì Shell nếu đang dùng NavigationPage
            await Application.Current.MainPage.Navigation.PushAsync(new Views.RegisterPage());
        }

        private async Task ForgotPasswordAsync()
        {
            await Application.Current.MainPage.DisplayAlert(
                "Quên mật khẩu",
                "Tính năng đang được phát triển",
                "OK");
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}