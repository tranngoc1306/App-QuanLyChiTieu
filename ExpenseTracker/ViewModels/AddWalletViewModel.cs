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
    public class AddWalletViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _database;
        private string _name;
        private string _balanceText;
        private string _budgetText;
        private string _selectedIcon;
        private string _selectedColor;
        private bool _isDefault;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<string> ValidationError;
        public event EventHandler SaveCompleted;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string BalanceText
        {
            get => _balanceText;
            set => SetProperty(ref _balanceText, value);
        }

        public string BudgetText
        {
            get => _budgetText;
            set => SetProperty(ref _budgetText, value);
        }

        public string SelectedIcon
        {
            get => _selectedIcon;
            set => SetProperty(ref _selectedIcon, value);
        }

        public string SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public List<string> Icons { get; }
        public List<string> Colors { get; }

        public ICommand SaveCommand { get; }

        public AddWalletViewModel(DatabaseService database)
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

            _selectedIcon = "💳";
            _selectedColor = "#4ECDC4";

            SaveCommand = new Command(async () => await SaveWalletAsync());
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

                decimal balance = string.IsNullOrWhiteSpace(BalanceText) ? 0 : decimal.Parse(BalanceText);
                decimal budget = string.IsNullOrWhiteSpace(BudgetText) ? 0 : decimal.Parse(BudgetText);

                await _database.InitializeDatabaseAsync();

                var wallet = new Wallet
                {
                    Name = Name.Trim(),
                    Balance = balance,
                    Budget = budget,
                    Icon = SelectedIcon,
                    Color = SelectedColor,
                    IsDefault = IsDefault
                };

                await _database.SaveWalletAsync(wallet);

                SaveCompleted?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine("✅ Wallet saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Save Error: {ex.Message}");
                ValidationError?.Invoke(this, $"Không thể lưu: {ex.Message}");
            }
        }

        private bool ValidateInputs(out string message)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                message = "Vui lòng nhập tên ví";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(BalanceText) && !decimal.TryParse(BalanceText, out _))
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