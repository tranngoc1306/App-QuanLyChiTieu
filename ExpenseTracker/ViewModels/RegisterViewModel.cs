using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ExpenseTracker.Services;
using Microsoft.Maui.Controls;

namespace ExpenseTracker.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _fullName;
        private string _email;
        private string _gender = "Nam";
        private DateTime _dateOfBirth = DateTime.Now.AddYears(-20);
        private string _password;
        private string _confirmPassword;
        private bool _isPasswordVisible;
        private bool _isConfirmPasswordVisible;
        private bool _isLoading;
        private string _errorMessage;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;

            RegisterCommand = new Command(async () => await RegisterAsync(), () => CanRegister);
            NavigateToLoginCommand = new Command(async () => await NavigateToLoginAsync());
            TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
            ToggleConfirmPasswordVisibilityCommand = new Command(ToggleConfirmPasswordVisibility);
            SelectGenderCommand = new Command<string>(SelectGender);
        }

        #region Properties

        public string FullName
        {
            get => _fullName;
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged();
                    ClearError();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                    ClearError();
                }
            }
        }

        public string Gender
        {
            get => _gender;
            set
            {
                if (_gender != value)
                {
                    _gender = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsMaleSelected));
                    OnPropertyChanged(nameof(IsFemaleSelected));
                    OnPropertyChanged(nameof(MaleFrameBackgroundColor));
                    OnPropertyChanged(nameof(FemaleFrameBackgroundColor));
                }
            }
        }

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (_dateOfBirth != value)
                {
                    _dateOfBirth = value;
                    OnPropertyChanged();
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
                    ClearError();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
                    OnPropertyChanged();
                    ClearError();
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

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set
            {
                if (_isConfirmPasswordVisible != value)
                {
                    _isConfirmPasswordVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsConfirmPasswordHidden));
                    OnPropertyChanged(nameof(ConfirmPasswordVisibilityIcon));
                }
            }
        }

        public bool IsConfirmPasswordHidden => !IsConfirmPasswordVisible;

        public string ConfirmPasswordVisibilityIcon => IsConfirmPasswordVisible ? "👁️" : "🔒";

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
                    OnPropertyChanged(nameof(CanRegister));
                    OnPropertyChanged(nameof(RegisterButtonText));
                    ((Command)RegisterCommand).ChangeCanExecute();
                }
            }
        }

        public bool IsNotLoading => !IsLoading;

        public bool CanRegister => !IsLoading;

        public string RegisterButtonText => IsLoading ? "Đang đăng ký..." : "Đăng ký";

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

        public bool IsMaleSelected => Gender == "Nam";

        public bool IsFemaleSelected => Gender == "Nữ";

        public Color MaleFrameBackgroundColor => IsMaleSelected
            ? Color.FromArgb("#E3F2FD")
            : Colors.White;

        public Color FemaleFrameBackgroundColor => IsFemaleSelected
            ? Color.FromArgb("#FCE4EC")
            : Colors.White;

        #endregion

        #region Commands

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
        public ICommand SelectGenderCommand { get; }

        #endregion

        #region Methods

        private async Task RegisterAsync()
        {
            if (!ValidateInput())
                return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.RegisterAsync(
                    FullName?.Trim(),
                    Email?.Trim(),
                    Gender,
                    DateOfBirth,
                    Password);

                if (result.Success)
                {
                    await ShowSuccessAndNavigateToLogin();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Đã xảy ra lỗi. Vui lòng thử lại.";
                System.Diagnostics.Debug.WriteLine($"🔴 Register Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Vui lòng nhập họ tên";
                return false;
            }

            if (FullName.Length < 2)
            {
                ErrorMessage = "Họ tên phải có ít nhất 2 ký tự";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Vui lòng nhập email";
                return false;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Email không hợp lệ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu";
                return false;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Vui lòng xác nhận mật khẩu";
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp";
                return false;
            }

            var age = CalculateAge(DateOfBirth);
            if (age < 13)
            {
                ErrorMessage = "Bạn phải từ 13 tuổi trở lên để đăng ký";
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

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }

        private async Task ShowSuccessAndNavigateToLogin()
        {
            await Application.Current.MainPage.DisplayAlert(
                "Thành công",
                "Đăng ký thành công! Vui lòng đăng nhập.",
                "OK");
            await NavigateToLoginAsync();
        }

        private async Task NavigateToLoginAsync()
        {
            // Quay lại trang trước (LoginPage)
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private void SelectGender(string gender)
        {
            Gender = gender;
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ToggleConfirmPasswordVisibility()
        {
            IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
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