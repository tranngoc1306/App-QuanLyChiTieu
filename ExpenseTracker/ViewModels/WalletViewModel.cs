using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.ViewModels
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _database;
        private ObservableCollection<WalletDisplayItem> _wallets;
        private decimal _totalBalance;
        private int _walletCount;
        private decimal _totalBudget;
        private bool _isEmptyStateVisible;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<WalletDisplayItem> Wallets
        {
            get => _wallets;
            set => SetProperty(ref _wallets, value);
        }

        public decimal TotalBalance
        {
            get => _totalBalance;
            set => SetProperty(ref _totalBalance, value);
        }

        public int WalletCount
        {
            get => _walletCount;
            set => SetProperty(ref _walletCount, value);
        }

        public decimal TotalBudget
        {
            get => _totalBudget;
            set => SetProperty(ref _totalBudget, value);
        }

        public bool IsEmptyStateVisible
        {
            get => _isEmptyStateVisible;
            set => SetProperty(ref _isEmptyStateVisible, value);
        }

        public ICommand LoadWalletsCommand { get; }
        public ICommand DeleteWalletCommand { get; }

        public WalletViewModel(DatabaseService database)
        {
            _database = database;
            _wallets = new ObservableCollection<WalletDisplayItem>();

            LoadWalletsCommand = new Command(async () => await LoadWalletsAsync());
            DeleteWalletCommand = new Command<WalletDisplayItem>(async (wallet) => await DeleteWalletAsync(wallet));
        }

        public async Task LoadWalletsAsync()
        {
            try
            {
                await _database.InitializeDatabaseAsync();

                var wallets = await _database.GetWalletsAsync();
                Wallets.Clear();

                decimal totalBalance = 0;
                decimal totalBudget = 0;

                foreach (var wallet in wallets)
                {
                    totalBalance += wallet.Balance;
                    totalBudget += wallet.Budget;

                    Wallets.Add(CreateWalletDisplayItem(wallet));
                }

                TotalBalance = totalBalance;
                WalletCount = wallets.Count;
                TotalBudget = totalBudget;
                IsEmptyStateVisible = wallets.Count == 0;

                Debug.WriteLine($"✅ Loaded {wallets.Count} wallets");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadWallets Error: {ex.Message}");
                throw;
            }
        }

        private WalletDisplayItem CreateWalletDisplayItem(Wallet wallet)
        {
            return new WalletDisplayItem
            {
                Id = wallet.Id,
                Name = wallet.Name,
                Balance = wallet.Balance,
                Budget = wallet.Budget,
                Icon = wallet.Icon,
                Color = wallet.Color,
                IsDefault = wallet.IsDefault,
                BalanceDisplay = CurrencyService.FormatAmount(wallet.Balance),
                BudgetDisplay = CurrencyService.FormatAmount(wallet.Budget),
                BalanceColor = wallet.Balance >= 0 ? Color.FromArgb("#26DE81") : Color.FromArgb("#FC5C65"),
                HasBudget = wallet.Budget > 0,
                BudgetProgress = wallet.Budget > 0 ? (double)(Math.Abs(wallet.Balance) / wallet.Budget) : 0,
                BudgetProgressColor = CalculateBudgetProgressColor(wallet)
            };
        }

        private Color CalculateBudgetProgressColor(Wallet wallet)
        {
            if (wallet.Balance >= 0)
                return Color.FromArgb("#26DE81");

            return Math.Abs(wallet.Balance) > wallet.Budget
                ? Color.FromArgb("#FC5C65")
                : Color.FromArgb("#FF9800");
        }

        public async Task DeleteWalletAsync(WalletDisplayItem walletDisplay)
        {
            try
            {
                var wallet = await _database.GetWalletAsync(walletDisplay.Id);
                if (wallet != null)
                {
                    await _database.DeleteWalletAsync(wallet);
                    await LoadWalletsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Delete Error: {ex.Message}");
                throw;
            }
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

    public class WalletDisplayItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public decimal Budget { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public bool IsDefault { get; set; }
        public string BalanceDisplay { get; set; }
        public string BudgetDisplay { get; set; }
        public Color BalanceColor { get; set; }
        public bool HasBudget { get; set; }
        public double BudgetProgress { get; set; }
        public Color BudgetProgressColor { get; set; }
    }
}