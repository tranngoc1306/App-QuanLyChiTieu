using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.ViewModels
{
    public class WalletDetailViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _database;
        private Wallet _wallet;
        private string _name;
        private string _balanceText;
        private string _budgetText;
        private string _selectedIcon;
        private string _selectedColor;
        private bool _isDefault;
        private string _previewName;
        private string _previewBalance;
        private string _previewBudget;
        private Color _previewBalanceColor;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<string> ValidationError;
        public event EventHandler SaveCompleted;
        public event EventHandler DeleteCompleted;
        public event EventHandler<string> LoadError;

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    UpdatePreview();
            }
        }

        public string BalanceText
        {
            get => _balanceText;
            set
            {
                if (SetProperty(ref _balanceText, value))
                    UpdatePreview();
            }
        }

        public string BudgetText
        {
            get => _budgetText;
            set
            {
                if (SetProperty(ref _budgetText, value))
                    UpdatePreview();
            }
        }

        public string SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                if (SetProperty(ref _selectedIcon, value))
                    UpdatePreview();
            }
        }

        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (SetProperty(ref _selectedColor, value))
                    UpdatePreview();
            }
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public string PreviewName
        {
            get => _previewName;
            set => SetProperty(ref _previewName, value);
        }

        public string PreviewBalance
        {
            get => _previewBalance;
            set => SetProperty(ref _previewBalance, value);
        }

        public string PreviewBudget
        {
            get => _previewBudget;
            set => SetProperty(ref _previewBudget, value);
        }

        public Color PreviewBalanceColor
        {
            get => _previewBalanceColor;
            set => SetProperty(ref _previewBalanceColor, value);
        }

        public List<string> Icons { get; }
        public List<string> Colors { get; }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public WalletDetailViewModel(DatabaseService database)
        {
            _database = database;

            Icons = new List<string>
            {
                "💳", "🏦", "💰", "💵", "💴", "💶",
                "💷", "💸", "🪙", "💼", "👛", "👝",
                "🎒", "🏪", "🏧", "💹", "📊", "📈"
            };

            Colors = new List<string>
            {
                "#4ECDC4", "#FF6B6B", "#FED330", "#45AAF2",
                "#A55EEA", "#26DE81", "#FC5C65", "#F7B731",
                "#FDA7DF", "#20BF6B", "#EE5A6F", "#C44569",
                "#0FB9B1", "#2D98DA", "#F79F1F", "#A3CB38"
            };

            SaveCommand = new Command(async () => await SaveWalletAsync());
            DeleteCommand = new Command(async () => await DeleteWalletAsync());
        }

        public async Task LoadWalletAsync(int walletId)
        {
            try
            {
                await _database.InitializeDatabaseAsync();

                _wallet = await _database.GetWalletAsync(walletId);

                if (_wallet == null)
                {
                    LoadError?.Invoke(this, "Không tìm thấy ví");
                    return;
                }

                Name = _wallet.Name;
                BalanceText = _wallet.Balance.ToString();
                BudgetText = _wallet.Budget.ToString();
                IsDefault = _wallet.IsDefault;
                SelectedIcon = _wallet.Icon;
                SelectedColor = _wallet.Color;

                UpdatePreview();

                Debug.WriteLine("✅ Wallet loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadWallet Error: {ex.Message}");
                LoadError?.Invoke(this, $"Không thể tải ví: {ex.Message}");
            }
        }

        private void UpdatePreview()
        {
            PreviewName = string.IsNullOrWhiteSpace(Name) ? "Ví của tôi" : Name;

            if (decimal.TryParse(BalanceText, out decimal balance))
            {
                PreviewBalance = CurrencyService.FormatAmount(balance);
                PreviewBalanceColor = balance >= 0 ? Color.FromArgb("#26DE81") : Color.FromArgb("#FC5C65");
            }
            else
            {
                PreviewBalance = CurrencyService.FormatAmount(0);
                PreviewBalanceColor = Color.FromArgb("#26DE81");
            }

            if (decimal.TryParse(BudgetText, out decimal budget))
            {
                PreviewBudget = $"Ngân sách: {CurrencyService.FormatAmount(budget)}";
            }
            else
            {
                PreviewBudget = $"Ngân sách: {CurrencyService.FormatAmount(0)}";
            }
        }

        private async Task SaveWalletAsync()
        {
            try
            {
                if (!ValidateInputs(out var validationMessage))
                {
                    ValidationError?.Invoke(this, validationMessage);
                    return;
                }

                decimal balance = decimal.Parse(BalanceText);
                decimal budget = string.IsNullOrWhiteSpace(BudgetText) ? 0 : decimal.Parse(BudgetText);

                _wallet.Name = Name.Trim();
                _wallet.Balance = balance;
                _wallet.Budget = budget;
                _wallet.Icon = SelectedIcon;
                _wallet.Color = SelectedColor;
                _wallet.IsDefault = IsDefault;

                await _database.SaveWalletAsync(_wallet);

                SaveCompleted?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine("✅ Wallet updated successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Save Error: {ex.Message}");
                ValidationError?.Invoke(this, $"Không thể lưu: {ex.Message}");
            }
        }

        private async Task DeleteWalletAsync()
        {
            try
            {
                await _database.DeleteWalletAsync(_wallet);
                DeleteCompleted?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine("✅ Wallet deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Delete Error: {ex.Message}");
                ValidationError?.Invoke(this, $"Không thể xóa: {ex.Message}");
            }
        }

        private bool ValidateInputs(out string message)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                message = "Vui lòng nhập tên ví";
                return false;
            }

            if (!decimal.TryParse(BalanceText, out _))
            {
                message = "Số dư không hợp lệ";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(BudgetText))
            {
                if (!decimal.TryParse(BudgetText, out decimal budget) || budget < 0)
                {
                    message = "Ngân sách không hợp lệ";
                    return false;
                }
            }

            message = null;
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}