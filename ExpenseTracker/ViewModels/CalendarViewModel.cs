using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ExpenseTracker.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private readonly DatabaseService _database;

        [ObservableProperty]
        private DateTime currentMonth;

        [ObservableProperty]
        private string monthYearDisplay;

        [ObservableProperty]
        private ObservableCollection<CalendarDay> calendarDays;

        [ObservableProperty]
        private ObservableCollection<DayTransactionGroup> transactionGroups;

        [ObservableProperty]
        private decimal totalIncome;

        [ObservableProperty]
        private decimal totalExpense;

        [ObservableProperty]
        private decimal balance;

        [ObservableProperty]
        private Wallet selectedWallet;

        [ObservableProperty]
        private ObservableCollection<Wallet> wallets;

        [ObservableProperty]
        private bool isLoading;

        public CalendarViewModel(DatabaseService database)
        {
            _database = database;
            CurrentMonth = DateTime.Now;
            CalendarDays = new ObservableCollection<CalendarDay>();
            TransactionGroups = new ObservableCollection<DayTransactionGroup>();
            Wallets = new ObservableCollection<Wallet>();

            UpdateMonthYearDisplay();
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                await _database.InitializeDatabaseAsync();
                await LoadWalletsAsync();
                await LoadCalendarDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 InitializeAsync Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadWalletsAsync()
        {
            var wallets = await _database.GetWalletsAsync();
            Wallets.Clear();

            // Thêm option "Tất cả ví" ở đầu
            Wallets.Add(new Wallet
            {
                Id = 0,
                Name = "Tất cả ví",
                Balance = 0,
                Icon = "💰",
                Color = "#4ECDC4"
            });

            // Thêm các ví thực
            foreach (var wallet in wallets)
            {
                Wallets.Add(wallet);
            }

            // Mặc định chọn "Tất cả ví"
            SelectedWallet = Wallets.FirstOrDefault();
        }

        [RelayCommand]
        private async Task PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            UpdateMonthYearDisplay();
            await LoadCalendarDataAsync();
        }

        [RelayCommand]
        private async Task NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            UpdateMonthYearDisplay();
            await LoadCalendarDataAsync();
        }

        [RelayCommand]
        private async Task GoToToday()
        {
            CurrentMonth = DateTime.Now;
            UpdateMonthYearDisplay();
            await LoadCalendarDataAsync();
        }

        public async Task OnWalletChangedAsync()
        {
            await LoadCalendarDataAsync();
            UpdateWalletBalance();
        }

        private void UpdateWalletBalance()
        {
            if (SelectedWallet != null && SelectedWallet.Id == 0)
            {
                // Tính tổng balance của tất cả ví
                var totalBalance = Wallets.Where(w => w.Id != 0).Sum(w => w.Balance);
                SelectedWallet.Balance = totalBalance;
            }
        }

        partial void OnCurrentMonthChanged(DateTime value)
        {
            UpdateMonthYearDisplay();
        }

        private void UpdateMonthYearDisplay()
        {
            MonthYearDisplay = $"Tháng {CurrentMonth.Month}/{CurrentMonth.Year}";
        }

        private async Task LoadCalendarDataAsync()
        {
            IsLoading = true;
            try
            {
                var startDate = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Load all transactions
                var allTransactions = await _database.GetTransactionsAsync();

                // Filter by wallet
                List<Transaction> transactions;
                if (SelectedWallet == null || SelectedWallet.Id == 0)
                {
                    // Hiển thị tất cả giao dịch
                    transactions = allTransactions
                        .Where(t => t.Date >= startDate && t.Date <= endDate)
                        .ToList();
                }
                else
                {
                    // Hiển thị giao dịch của ví được chọn
                    transactions = allTransactions
                        .Where(t => t.WalletId == SelectedWallet.Id)
                        .Where(t => t.Date >= startDate && t.Date <= endDate)
                        .ToList();
                }

                // Load categories
                var categories = new Dictionary<int, Category>();
                foreach (var transaction in transactions)
                {
                    if (!categories.ContainsKey(transaction.CategoryId))
                    {
                        var category = await _database.GetCategoryAsync(transaction.CategoryId);
                        if (category != null)
                        {
                            categories[transaction.CategoryId] = category;
                        }
                    }
                }

                // Calculate statistics
                TotalIncome = transactions
                    .Where(t => t.Type == TransactionType.Income) // && !t.ExcludeFromReport)
                    .Sum(t => t.Amount);

                TotalExpense = transactions
                    .Where(t => t.Type == TransactionType.Expense) // && !t.ExcludeFromReport)
                    .Sum(t => t.Amount);

                Balance = TotalIncome - TotalExpense;

                // Update wallet balance for "Tất cả ví"
                UpdateWalletBalance();

                // Build calendar
                BuildCalendar(transactions, categories);

                // Build transaction groups
                BuildTransactionGroups(transactions, categories);

                Debug.WriteLine($"✅ Loaded {transactions.Count} transactions");
                Debug.WriteLine($"✅ CalendarDays: {CalendarDays.Count}");
                Debug.WriteLine($"✅ TransactionGroups: {TransactionGroups.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadCalendarDataAsync Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BuildCalendar(List<Transaction> transactions, Dictionary<int, Category> categories)
        {
            CalendarDays.Clear();

            var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Fix: Tính offset đúng cho Monday = 0, Sunday = 6
            var dayOfWeek = firstDay.DayOfWeek;
            int offset;
            if (dayOfWeek == DayOfWeek.Sunday)
            {
                offset = 6; // Sunday là ngày cuối tuần (cột 7)
            }
            else
            {
                offset = (int)dayOfWeek - 1; // Monday = 0, Tuesday = 1, ...
            }

            Debug.WriteLine($"First day: {firstDay:dd/MM/yyyy} ({dayOfWeek}), Offset: {offset}");

            // Add previous month days
            for (int i = offset - 1; i >= 0; i--)
            {
                var date = firstDay.AddDays(-i - 1);
                CalendarDays.Add(new CalendarDay
                {
                    Date = date,
                    Day = date.Day,
                    IsCurrentMonth = false,
                    IsToday = false,
                    HasTransactions = false
                });
            }

            // Add current month days
            for (int day = 1; day <= lastDay.Day; day++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                var dayTransactions = transactions.Where(t => t.Date.Date == date.Date).ToList();

                var income = dayTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var expense = dayTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                var total = expense > 0 ? expense : income;

                CalendarDays.Add(new CalendarDay
                {
                    Date = date,
                    Day = day,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Now.Date,
                    HasTransactions = dayTransactions.Any(),
                    Amount = total,
                    AmountDisplay = total > 0 ? FormatAmount(total) : "",
                    TransactionCount = dayTransactions.Count
                });
            }

            // Add next month days to complete the grid
            var totalDays = CalendarDays.Count;
            var remainingDays = (7 - (totalDays % 7)) % 7;
            for (int i = 1; i <= remainingDays; i++)
            {
                var date = lastDay.AddDays(i);
                CalendarDays.Add(new CalendarDay
                {
                    Date = date,
                    Day = date.Day,
                    IsCurrentMonth = false,
                    IsToday = false,
                    HasTransactions = false
                });
            }

            Debug.WriteLine($"Total calendar days: {CalendarDays.Count}");
        }

        private void BuildTransactionGroups(List<Transaction> transactions, Dictionary<int, Category> categories)
        {
            TransactionGroups.Clear();

            var grouped = transactions
                .OrderByDescending(t => t.Date)
                .GroupBy(t => t.Date.Date);

            foreach (var group in grouped)
            {
                var items = new ObservableCollection<TransactionDisplayItem>();

                foreach (var transaction in group)
                {
                    Category category = null;
                    categories.TryGetValue(transaction.CategoryId, out category);

                    items.Add(new TransactionDisplayItem
                    {
                        Id = transaction.Id,
                        Amount = transaction.Amount,
                        Note = transaction.Note,
                        CategoryName = category?.Name ?? "Unknown",
                        CategoryIcon = category?.Icon ?? "❓",
                        AmountDisplay = $"{transaction.Amount:N0}đ",
                        AmountColor = transaction.Type == TransactionType.Income
                            ? Color.FromArgb("#26DE81")
                            : Color.FromArgb("#FC5C65"),
                        HasNote = !string.IsNullOrWhiteSpace(transaction.Note)
                    });
                }

                TransactionGroups.Add(new DayTransactionGroup
                {
                    Date = group.Key,
                    DateDisplay = FormatGroupDate(group.Key),
                    Transactions = items
                });
            }
        }

        private string FormatAmount(decimal amount)
        {
            if (amount >= 1000000)
                return $"{amount / 1000000:0.#}M";
            if (amount >= 1000)
                return $"{amount / 1000:0}k";
            return $"{amount:0}";
        }

        private string FormatGroupDate(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ Hai",
                DayOfWeek.Tuesday => "Thứ Ba",
                DayOfWeek.Wednesday => "Thứ Tư",
                DayOfWeek.Thursday => "Thứ Năm",
                DayOfWeek.Friday => "Thứ Sáu",
                DayOfWeek.Saturday => "Thứ Bảy",
                DayOfWeek.Sunday => "Chủ Nhật",
                _ => ""
            };

            return $"{dayOfWeek}, {date:dd/MM}";
        }
    }

    // Models for Calendar
    public partial class CalendarDay : ObservableObject
    {
        [ObservableProperty]
        private DateTime date;

        [ObservableProperty]
        private int day;

        [ObservableProperty]
        private bool isCurrentMonth;

        [ObservableProperty]
        private bool isToday;

        [ObservableProperty]
        private bool hasTransactions;

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private string amountDisplay;

        [ObservableProperty]
        private int transactionCount;
    }

    public class DayTransactionGroup
    {
        public DateTime Date { get; set; }
        public string DateDisplay { get; set; }
        public ObservableCollection<TransactionDisplayItem> Transactions { get; set; }
    }

    public class TransactionDisplayItem
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string AmountDisplay { get; set; }
        public Color AmountColor { get; set; }
        public bool HasNote { get; set; }
    }
}