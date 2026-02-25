using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.Maui.Controls;

namespace ExpenseTracker.ViewModels
{
    public class TransactionsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _database;

        private TransactionType _selectedType = TransactionType.Expense;
        private DateTime _selectedDate = DateTime.Now;
        private decimal _amount;
        private string _note;
        private Category _selectedCategory;
        private Wallet _selectedWallet;
        private bool _excludeFromReport;

        private decimal _totalIncome;
        private decimal _totalExpense;
        private decimal _balance;

        public TransactionsViewModel(DatabaseService database)
        {
            _database = database;

            SaveCommand = new Command(async () => await SaveTransactionAsync());
            SelectCategoryCommand = new Command<Category>(SelectCategory);
            SelectDateCommand = new Command(async () => await SelectDateAsync());
            LoadDataCommand = new Command(async () => await LoadDataAsync());
        }

        // Properties
        public TransactionType SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged();
                _ = LoadCategoriesAsync(); // Fire and forget
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged();
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public Wallet SelectedWallet
        {
            get => _selectedWallet;
            set
            {
                _selectedWallet = value;
                OnPropertyChanged();
            }
        }

        public bool ExcludeFromReport
        {
            get => _excludeFromReport;
            set
            {
                _excludeFromReport = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set
            {
                _totalIncome = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set
            {
                _totalExpense = value;
                OnPropertyChanged();
            }
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<Wallet> Wallets { get; } = new ObservableCollection<Wallet>();
        public ObservableCollection<TransactionGroup> TransactionGroups { get; } = new ObservableCollection<TransactionGroup>();

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand SelectDateCommand { get; }
        public ICommand LoadDataCommand { get; }

        // Public async methods để gọi từ code-behind
        public async Task LoadDataAsync()
        {
            await LoadCategoriesAsync();
            await LoadWalletsAsync();
            await LoadTransactionsAsync();
        }

        public async Task SaveTransactionAsync()
        {
            if (Amount <= 0 || SelectedCategory == null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin", "OK");
                return;
            }

            var transaction = new Transaction
            {
                Type = SelectedType,
                Amount = Amount,
                Date = SelectedDate,
                Note = Note,
                CategoryId = SelectedCategory.Id,
                WalletId = SelectedWallet?.Id,
                //ExcludeFromReport = ExcludeFromReport
            };

            await _database.SaveTransactionAsync(transaction);

            // Update wallet balance
            if (SelectedWallet != null)
            {
                var amount = SelectedType == TransactionType.Income ? Amount : -Amount;
                await _database.UpdateWalletBalanceAsync(SelectedWallet.Id, amount);
            }

            // Reset form
            Amount = 0;
            Note = string.Empty;
            SelectedCategory = null;
            ExcludeFromReport = false;

            await LoadTransactionsAsync();
            await LoadWalletsAsync();

            await Application.Current.MainPage.DisplayAlert("Thành công", "Đã lưu giao dịch", "OK");
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _database.GetCategoriesAsync(SelectedType);
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private async Task LoadWalletsAsync()
        {
            var wallets = await _database.GetWalletsAsync();
            Wallets.Clear();
            foreach (var wallet in wallets)
            {
                Wallets.Add(wallet);
            }

            if (Wallets.Count > 0 && SelectedWallet == null)
            {
                SelectedWallet = Wallets[0];
            }
        }

        private async Task LoadTransactionsAsync()
        {
            var transactions = await _database.GetTransactionsAsync();

            // Calculate totals
            TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            Balance = TotalIncome - TotalExpense;

            // Group by date
            var grouped = transactions.GroupBy(t => t.Date.Date)
                                    .OrderByDescending(g => g.Key);

            TransactionGroups.Clear();
            foreach (var group in grouped)
            {
                var transactionGroup = new TransactionGroup(group.Key, group.ToList(), _database);
                TransactionGroups.Add(transactionGroup);
            }
        }

        private void SelectCategory(Category category)
        {
            SelectedCategory = category;
        }

        private async Task SelectDateAsync()
        {
            // This would open a date picker in the actual implementation
            await Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TransactionGroup : ObservableCollection<TransactionItem>
    {
        public string DateLabel { get; set; }
        public DateTime Date { get; set; }

        public TransactionGroup(DateTime date, List<Transaction> transactions, DatabaseService database)
        {
            Date = date;
            DateLabel = GetDateLabel(date);

            foreach (var transaction in transactions)
            {
                Add(new TransactionItem(transaction, database));
            }
        }

        private string GetDateLabel(DateTime date)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            if (date.Date == today)
                return "Hôm Nay";
            else if (date.Date == yesterday)
                return "Hôm Qua";
            else
                return date.ToString("dd/MM/yyyy");
        }
    }

    public class TransactionItem : INotifyPropertyChanged
    {
        private readonly Transaction _transaction;
        private readonly DatabaseService _database;
        private Category _category;

        public TransactionItem(Transaction transaction, DatabaseService database)
        {
            _transaction = transaction;
            _database = database;

            LoadCategoryAsync();
        }

        private async void LoadCategoryAsync()
        {
            _category = await _database.GetCategoryAsync(_transaction.CategoryId);
            OnPropertyChanged(nameof(CategoryName));
            OnPropertyChanged(nameof(CategoryIcon));
        }

        public int Id => _transaction.Id;
        public decimal Amount => _transaction.Amount;
        public string Note => _transaction.Note;
        public DateTime Date => _transaction.Date;
        public TransactionType Type => _transaction.Type;
        public string CategoryName => _category?.Name ?? "";
        public string CategoryIcon => _category?.Icon ?? "❓";

        public string AmountDisplay => $"{Amount:N0}đ";
        public Color AmountColor => Type == TransactionType.Income
            ? Color.FromArgb("#26DE81")
            : Color.FromArgb("#FC5C65");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}