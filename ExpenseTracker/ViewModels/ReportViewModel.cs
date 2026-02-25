using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microcharts;
using SkiaSharp;

namespace ExpenseTracker.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _database;

        private ObservableCollection<Wallet> _wallets;
        private ObservableCollection<CategoryLegendItem> _categoryLegend;
        private Wallet _selectedWallet;
        private int _currentYear;
        private int _currentMonth;
        private bool _isMonthView;
        private string _chartType;
        private TransactionType _selectedCategoryType;
        private bool _isEmptyStateVisible;
        private bool _isChartVisible;
        private bool _showIncome;
        private bool _showExpense;

        private string _periodLabel;
        private string _mainChartTitle;
        private string _xAxisTitle;
        private string _pieChartTitle;
        private decimal _totalIncome;
        private decimal _totalExpense;
        private decimal _netIncome;
        private Color _netIncomeColor;

        private List<decimal> _incomeData;
        private List<decimal> _expenseData;
        private List<string> _labels;
        private Chart _categoryChart;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ChartDataChanged;
        public event EventHandler<string> ErrorOccurred;

        public ObservableCollection<Wallet> Wallets
        {
            get => _wallets;
            set => SetProperty(ref _wallets, value);
        }

        public ObservableCollection<CategoryLegendItem> CategoryLegend
        {
            get => _categoryLegend;
            set => SetProperty(ref _categoryLegend, value);
        }

        public Wallet SelectedWallet
        {
            get => _selectedWallet;
            set
            {
                if (SetProperty(ref _selectedWallet, value))
                    _ = LoadReportDataAsync();
            }
        }

        public int CurrentYear
        {
            get => _currentYear;
            set => SetProperty(ref _currentYear, value);
        }

        public int CurrentMonth
        {
            get => _currentMonth;
            set => SetProperty(ref _currentMonth, value);
        }

        public bool IsMonthView
        {
            get => _isMonthView;
            set => SetProperty(ref _isMonthView, value);
        }

        public string ChartType
        {
            get => _chartType;
            set
            {
                if (SetProperty(ref _chartType, value))
                    NotifyChartDataChanged();
            }
        }

        public TransactionType SelectedCategoryType
        {
            get => _selectedCategoryType;
            set => SetProperty(ref _selectedCategoryType, value);
        }

        public bool IsEmptyStateVisible
        {
            get => _isEmptyStateVisible;
            set => SetProperty(ref _isEmptyStateVisible, value);
        }

        public bool IsChartVisible
        {
            get => _isChartVisible;
            set => SetProperty(ref _isChartVisible, value);
        }

        public bool ShowIncome
        {
            get => _showIncome;
            set
            {
                if (SetProperty(ref _showIncome, value))
                    NotifyChartDataChanged();
            }
        }

        public bool ShowExpense
        {
            get => _showExpense;
            set
            {
                if (SetProperty(ref _showExpense, value))
                    NotifyChartDataChanged();
            }
        }

        public string PeriodLabel
        {
            get => _periodLabel;
            set => SetProperty(ref _periodLabel, value);
        }

        public string MainChartTitle
        {
            get => _mainChartTitle;
            set => SetProperty(ref _mainChartTitle, value);
        }

        public string XAxisTitle
        {
            get => _xAxisTitle;
            set => SetProperty(ref _xAxisTitle, value);
        }

        public string PieChartTitle
        {
            get => _pieChartTitle;
            set => SetProperty(ref _pieChartTitle, value);
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set => SetProperty(ref _totalExpense, value);
        }

        public decimal NetIncome
        {
            get => _netIncome;
            set => SetProperty(ref _netIncome, value);
        }

        public Color NetIncomeColor
        {
            get => _netIncomeColor;
            set => SetProperty(ref _netIncomeColor, value);
        }

        public List<decimal> IncomeData
        {
            get => _incomeData;
            set => SetProperty(ref _incomeData, value);
        }

        public List<decimal> ExpenseData
        {
            get => _expenseData;
            set => SetProperty(ref _expenseData, value);
        }

        public List<string> Labels
        {
            get => _labels;
            set => SetProperty(ref _labels, value);
        }

        public Chart CategoryChart
        {
            get => _categoryChart;
            set => SetProperty(ref _categoryChart, value);
        }

        public double ChartWidth => IsMonthView
            ? DateTime.DaysInMonth(CurrentYear, CurrentMonth) * 45
            : 12 * 70;

        public double LabelWidth => IsMonthView ? 45.0 : 70.0;

        public ICommand LoadWalletsCommand { get; }
        public ICommand LoadReportDataCommand { get; }
        public ICommand SwitchToMonthViewCommand { get; }
        public ICommand SwitchToYearViewCommand { get; }
        public ICommand PreviousPeriodCommand { get; }
        public ICommand NextPeriodCommand { get; }
        public ICommand SwitchToBarChartCommand { get; }
        public ICommand SwitchToLineChartCommand { get; }
        public ICommand SwitchToAreaChartCommand { get; }
        public ICommand SwitchToExpenseCommand { get; }
        public ICommand SwitchToIncomeCommand { get; }

        public ReportViewModel(DatabaseService database)
        {
            _database = database;
            _wallets = new ObservableCollection<Wallet>();
            _categoryLegend = new ObservableCollection<CategoryLegendItem>();
            _incomeData = new List<decimal>();
            _expenseData = new List<decimal>();
            _labels = new List<string>();

            _currentYear = DateTime.Now.Year;
            _currentMonth = DateTime.Now.Month;
            _isMonthView = true;
            _chartType = "bar";
            _selectedCategoryType = TransactionType.Expense;
            _showIncome = true;
            _showExpense = true;
            _pieChartTitle = "Chi phí theo danh mục";

            LoadWalletsCommand = new Command(async () => await LoadWalletsAsync());
            LoadReportDataCommand = new Command(async () => await LoadReportDataAsync());
            SwitchToMonthViewCommand = new Command(async () => await SwitchToMonthViewAsync());
            SwitchToYearViewCommand = new Command(async () => await SwitchToYearViewAsync());
            PreviousPeriodCommand = new Command(async () => await MoveToPreviousPeriodAsync());
            NextPeriodCommand = new Command(async () => await MoveToNextPeriodAsync());
            SwitchToBarChartCommand = new Command(() => ChartType = "bar");
            SwitchToLineChartCommand = new Command(() => ChartType = "line");
            SwitchToAreaChartCommand = new Command(() => ChartType = "area");
            SwitchToExpenseCommand = new Command(async () => await SwitchToExpenseAsync());
            SwitchToIncomeCommand = new Command(async () => await SwitchToIncomeAsync());
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _database.InitializeDatabaseAsync();
                await LoadWalletsAsync();
                UpdatePeriodLabel();
                await LoadReportDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Initialize Error: {ex.Message}");
                ErrorOccurred?.Invoke(this, $"Không thể khởi tạo: {ex.Message}");
            }
        }

        private async Task LoadWalletsAsync()
        {
            try
            {
                var wallets = await _database.GetWalletsAsync();
                Wallets.Clear();

                Wallets.Add(new Wallet { Id = 0, Name = "Tất cả ví", Icon = "💼" });

                foreach (var wallet in wallets)
                {
                    Wallets.Add(wallet);
                }

                if (Wallets.Count > 0)
                {
                    SelectedWallet = Wallets[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadWallets Error: {ex.Message}");
            }
        }

        private async Task LoadReportDataAsync()
        {
            try
            {
                var walletId = SelectedWallet?.Id == 0 ? null : SelectedWallet?.Id;
                List<Transaction> transactions;

                if (IsMonthView)
                {
                    transactions = await _database.GetTransactionsByMonthAsync(CurrentYear, CurrentMonth, walletId);
                }
                else
                {
                    transactions = await GetTransactionsByYearAsync(CurrentYear, walletId);
                }

                if (transactions.Count == 0)
                {
                    IsEmptyStateVisible = true;
                    IsChartVisible = false;
                    return;
                }

                IsEmptyStateVisible = false;
                IsChartVisible = true;

                CalculateTotals(transactions);
                PrepareChartData(transactions);
                await LoadCategoryChartAsync(transactions);

                NotifyChartDataChanged();

                Debug.WriteLine($"✅ Loaded report data: {transactions.Count} transactions");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadReportData Error: {ex.Message}");
                ErrorOccurred?.Invoke(this, $"Không thể tải báo cáo: {ex.Message}");
            }
        }

        private async Task<List<Transaction>> GetTransactionsByYearAsync(int year, int? walletId)
        {
            var transactions = new List<Transaction>();
            for (int month = 1; month <= 12; month++)
            {
                var monthTransactions = await _database.GetTransactionsByMonthAsync(year, month, walletId);
                transactions.AddRange(monthTransactions);
            }
            return transactions;
        }

        private void CalculateTotals(List<Transaction> transactions)
        {
            TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            NetIncome = TotalIncome - TotalExpense;
            NetIncomeColor = NetIncome >= 0
                ? Color.FromArgb("#26DE81")
                : Color.FromArgb("#FC5C65");
        }

        private void PrepareChartData(List<Transaction> transactions)
        {
            IncomeData = new List<decimal>();
            ExpenseData = new List<decimal>();
            Labels = new List<string>();

            if (IsMonthView)
            {
                PrepareMonthlyChartData(transactions);
            }
            else
            {
                PrepareYearlyChartData(transactions);
            }

            OnPropertyChanged(nameof(ChartWidth));
            OnPropertyChanged(nameof(LabelWidth));
        }

        private void PrepareMonthlyChartData(List<Transaction> transactions)
        {
            var daysInMonth = DateTime.DaysInMonth(CurrentYear, CurrentMonth);
            var incomeByDay = transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Date.Day)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var expenseByDay = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Date.Day)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            for (int day = 1; day <= daysInMonth; day++)
            {
                Labels.Add(day.ToString());
                IncomeData.Add(incomeByDay.ContainsKey(day) ? incomeByDay[day] : 0);
                ExpenseData.Add(expenseByDay.ContainsKey(day) ? expenseByDay[day] : 0);
            }
        }

        private void PrepareYearlyChartData(List<Transaction> transactions)
        {
            var monthNames = new[] { "", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };
            var incomeByMonth = transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Date.Month)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var expenseByMonth = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Date.Month)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            for (int month = 1; month <= 12; month++)
            {
                Labels.Add(monthNames[month]);
                IncomeData.Add(incomeByMonth.ContainsKey(month) ? incomeByMonth[month] : 0);
                ExpenseData.Add(expenseByMonth.ContainsKey(month) ? expenseByMonth[month] : 0);
            }
        }

        private async Task LoadCategoryChartAsync(List<Transaction> transactions)
        {
            try
            {
                var filteredTransactions = transactions
                    .Where(t => t.Type == SelectedCategoryType)
                    .ToList();

                CategoryLegend.Clear();

                if (filteredTransactions.Count == 0)
                {
                    CategoryChart = null;
                    return;
                }

                var groupedByCategory = filteredTransactions
                    .GroupBy(t => t.CategoryId)
                    .Select(g => new
                    {
                        CategoryId = g.Key,
                        Total = g.Sum(t => t.Amount)
                    })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                var entries = new List<ChartEntry>();
                var total = groupedByCategory.Sum(x => x.Total);

                var colors = new[]
                {
                    "#FF6B6B", "#4ECDC4", "#FFE66D", "#FF6B9D", "#C44569",
                    "#FFA502", "#A29BFE", "#FDA7DF", "#F8B500", "#FF6348",
                    "#1E90FF", "#FF69B4", "#26DE81", "#95A5A6", "#FED330"
                };

                for (int i = 0; i < groupedByCategory.Count; i++)
                {
                    var item = groupedByCategory[i];
                    var category = await _database.GetCategoryAsync(item.CategoryId);
                    var percentage = (double)item.Total / (double)total * 100;
                    var colorHex = category?.Color ?? colors[i % colors.Length];

                    entries.Add(new ChartEntry((float)item.Total)
                    {
                        Label = category?.Name ?? "Unknown",
                        ValueLabel = $"{percentage:F1}%",
                        Color = SKColor.Parse(colorHex)
                    });

                    CategoryLegend.Add(new CategoryLegendItem
                    {
                        Name = $"{category?.Icon ?? "❓"} {category?.Name ?? "Unknown"}",
                        Amount = item.Total,
                        AmountDisplay = CurrencyService.FormatAmount(item.Total),
                        Percentage = percentage,
                        Color = Color.FromArgb(colorHex)
                    });
                }

                CategoryChart = new DonutChart
                {
                    Entries = entries,
                    LabelTextSize = 32,
                    BackgroundColor = SKColors.White,
                    HoleRadius = 0.4f,
                    LabelMode = LabelMode.None
                };

                Debug.WriteLine($"✅ Loaded category chart: {entries.Count} categories");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadCategoryChart Error: {ex.Message}");
            }
        }

        private void UpdatePeriodLabel()
        {
            if (IsMonthView)
            {
                if (CurrentYear == DateTime.Now.Year && CurrentMonth == DateTime.Now.Month)
                {
                    PeriodLabel = "Tháng này";
                }
                else
                {
                    PeriodLabel = $"Tháng {CurrentMonth}/{CurrentYear}";
                }
                MainChartTitle = "Biểu đồ giao dịch theo ngày";
                XAxisTitle = "Ngày trong tháng";
            }
            else
            {
                if (CurrentYear == DateTime.Now.Year)
                {
                    PeriodLabel = "Năm nay";
                }
                else
                {
                    PeriodLabel = $"Năm {CurrentYear}";
                }
                MainChartTitle = "Biểu đồ giao dịch theo tháng";
                XAxisTitle = "Tháng trong năm";
            }
        }

        private async Task SwitchToMonthViewAsync()
        {
            if (!IsMonthView)
            {
                IsMonthView = true;
                UpdatePeriodLabel();
                await LoadReportDataAsync();
            }
        }

        private async Task SwitchToYearViewAsync()
        {
            if (IsMonthView)
            {
                IsMonthView = false;
                UpdatePeriodLabel();
                await LoadReportDataAsync();
            }
        }

        private async Task MoveToPreviousPeriodAsync()
        {
            if (IsMonthView)
            {
                CurrentMonth--;
                if (CurrentMonth < 1)
                {
                    CurrentMonth = 12;
                    CurrentYear--;
                }
            }
            else
            {
                CurrentYear--;
            }
            UpdatePeriodLabel();
            await LoadReportDataAsync();
        }

        private async Task MoveToNextPeriodAsync()
        {
            if (IsMonthView)
            {
                if (CurrentYear == DateTime.Now.Year && CurrentMonth >= DateTime.Now.Month)
                    return;
                CurrentMonth++;
                if (CurrentMonth > 12)
                {
                    CurrentMonth = 1;
                    CurrentYear++;
                }
            }
            else
            {
                if (CurrentYear >= DateTime.Now.Year)
                    return;
                CurrentYear++;
            }
            UpdatePeriodLabel();
            await LoadReportDataAsync();
        }

        private async Task SwitchToExpenseAsync()
        {
            if (SelectedCategoryType != TransactionType.Expense)
            {
                SelectedCategoryType = TransactionType.Expense;
                PieChartTitle = "Chi phí theo danh mục";
                await LoadReportDataAsync();
            }
        }

        private async Task SwitchToIncomeAsync()
        {
            if (SelectedCategoryType != TransactionType.Income)
            {
                SelectedCategoryType = TransactionType.Income;
                PieChartTitle = "Thu nhập theo danh mục";
                await LoadReportDataAsync();
            }
        }

        private void NotifyChartDataChanged()
        {
            ChartDataChanged?.Invoke(this, EventArgs.Empty);
        }

        public float GetMaxValue()
        {
            return Math.Max(
                IncomeData.Count > 0 ? (float)IncomeData.Max() : 0,
                ExpenseData.Count > 0 ? (float)ExpenseData.Max() : 0
            );
        }

        public string FormatYAxisLabel(decimal value)
        {
            if (value >= 1000000)
            {
                return $"{value / 1000000:F1}M";
            }
            else if (value >= 1000)
            {
                return $"{value / 1000:F0}K";
            }
            else if (value > 0)
            {
                return $"{value:F0}";
            }
            else
            {
                return "0";
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

    public class CategoryLegendItem
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string AmountDisplay { get; set; }
        public double Percentage { get; set; }
        public Color Color { get; set; }
    }
}