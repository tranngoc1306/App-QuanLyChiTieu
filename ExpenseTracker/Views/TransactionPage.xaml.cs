using ExpenseTracker.Models;
using ExpenseTracker.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ExpenseTracker.Views
{
    public partial class TransactionPage : ContentPage
    {
        private DatabaseService _database;
        private TransactionType _selectedType = TransactionType.Expense;
        private CategoryViewModel _selectedCategory;
        private Wallet _selectedWallet;
        private DateTime _selectedDate = DateTime.Now;
        private bool _isInitialized = false;
        private bool _isLoading = false;

        private ObservableCollection<CategoryViewModel> _categories = new ObservableCollection<CategoryViewModel>();
        private ObservableCollection<Wallet> _wallets = new ObservableCollection<Wallet>();
        private ObservableCollection<TransactionDisplayItem> _transactions = new ObservableCollection<TransactionDisplayItem>();

        public TransactionPage()
        {
            try
            {
                InitializeComponent();

                // Initialize database
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbPath = Path.Combine(appDataPath, "ExpenseTracker", "expensetracker.db3");

                _database = new DatabaseService(dbPath);

                // Bind collections
                CategoriesCollectionView.ItemsSource = _categories;
                TransactionsCollectionView.ItemsSource = _transactions;

                // Set default date
                TransactionDatePicker.Date = _selectedDate;
                UpdateTransactionListTitle();

                // Load data
                InitializePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 TransactionPage Error: {ex.Message}");
            }
        }

        private async void InitializePageAsync()
        {
            if (_isInitialized) return;

            try
            {
                _isLoading = true;
                await _database.InitializeDatabaseAsync();
                await LoadWalletsAsync();
                await SelectDefaultWalletAsync();
                await LoadCategoriesAsync();
                await LoadTransactionsAsync();

                UpdateTabSelection();

                _isInitialized = true;
                _isLoading = false;
                Debug.WriteLine("✅ TransactionPage initialized");
            }
            catch (Exception ex)
            {
                _isLoading = false;
                Debug.WriteLine($"🔴 Initialize Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể khởi tạo trang: {ex.Message}", "OK");
            }
        }

        private async System.Threading.Tasks.Task SelectDefaultWalletAsync()
        {
            try
            {
                if (_wallets.Count == 0) return;

                var defaultWallet = await _database.GetDefaultWalletAsync();

                if (defaultWallet != null)
                {
                    _selectedWallet = defaultWallet;
                    var index = _wallets.ToList().FindIndex(w => w.Id == defaultWallet.Id);
                    if (index >= 0)
                    {
                        WalletPicker.SelectedIndex = index;
                    }
                }
                else if (_wallets.Count > 0)
                {
                    _selectedWallet = _wallets[0];
                    WalletPicker.SelectedIndex = 0;
                }

                UpdateWalletHeader();
                Debug.WriteLine($"✅ Selected wallet: {_selectedWallet?.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 SelectDefaultWallet Error: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadWalletsAsync()
        {
            try
            {
                var wallets = await _database.GetWalletsAsync();
                _wallets.Clear();

                foreach (var wallet in wallets)
                {
                    _wallets.Add(wallet);
                }

                WalletPicker.ItemsSource = _wallets.ToList();
                WalletPicker.ItemDisplayBinding = new Binding("Name");

                Debug.WriteLine($"✅ Loaded {_wallets.Count} wallets");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadWallets Error: {ex.Message}");
            }
        }

        private async void UpdateWalletHeader()
        {
            try
            {
                if (_selectedWallet == null) return;

                // Reload wallet from database to get latest balance
                var updatedWallet = await _database.GetWalletAsync(_selectedWallet.Id);
                if (updatedWallet != null)
                {
                    _selectedWallet = updatedWallet;

                    // Update UI on main thread with currency formatting
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        WalletNameLabel.Text = _selectedWallet.Name;
                        WalletBalanceLabel.Text = CurrencyService.FormatAmount(_selectedWallet.Balance);
                        WalletBalanceLabel.TextColor = _selectedWallet.Balance >= 0
                            ? Color.FromArgb("#26DE81")
                            : Color.FromArgb("#FC5C65");
                    });

                    Debug.WriteLine($"✅ Updated wallet header: {_selectedWallet.Name} - {CurrencyService.FormatAmount(_selectedWallet.Balance)}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 UpdateWalletHeader Error: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _database.GetCategoriesAsync(_selectedType);

                // Clear on UI thread to prevent race condition
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _categories.Clear();
                });

                // Add categories on UI thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var category in categories)
                    {
                        _categories.Add(new CategoryViewModel(category));
                    }
                });

                // Reset selection
                _selectedCategory = null;

                Debug.WriteLine($"✅ Loaded {_categories.Count} categories for {_selectedType}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadCategories Error: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadTransactionsAsync()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;

                // Clear transactions first to prevent duplicates
                _transactions.Clear();

                // Load ALL transactions
                var allTransactions = await _database.GetTransactionsAsync();

                // Filter by selected wallet
                var walletTransactions = _selectedWallet != null
                    ? allTransactions.Where(t => t.WalletId == _selectedWallet.Id).ToList()
                    : allTransactions;

                // Calculate totals for the selected wallet
                decimal totalIncome = walletTransactions
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount);

                decimal totalExpense = walletTransactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                // Update summary on main thread with currency formatting
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TotalIncomeLabel.Text = CurrencyService.FormatAmount(totalIncome);
                    TotalExpenseLabel.Text = CurrencyService.FormatAmount(totalExpense);

                    var balance = totalIncome - totalExpense;
                    BalanceLabel.Text = CurrencyService.FormatAmount(balance);
                    BalanceLabel.TextColor = balance >= 0
                        ? Color.FromArgb("#26DE81")
                        : Color.FromArgb("#FC5C65");

                    SummaryFrame.IsVisible = walletTransactions.Count > 0;
                });

                // Filter transactions by selected date
                var transactionsForDate = walletTransactions
                    .Where(t => t.Date.Date == _selectedDate.Date)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();

                // Load transactions for display with currency formatting
                foreach (var transaction in transactionsForDate)
                {
                    var category = await _database.GetCategoryAsync(transaction.CategoryId);

                    _transactions.Add(new TransactionDisplayItem
                    {
                        Id = transaction.Id,
                        Amount = transaction.Amount,
                        Note = transaction.Note,
                        CategoryName = category?.Name ?? "Unknown",
                        CategoryIcon = category?.Icon ?? "❓",
                        AmountDisplay = CurrencyService.FormatAmount(transaction.Amount),
                        AmountColor = transaction.Type == TransactionType.Income
                            ? Color.FromArgb("#26DE81")
                            : Color.FromArgb("#FC5C65"),
                        HasNote = !string.IsNullOrWhiteSpace(transaction.Note)
                    });
                }

                Debug.WriteLine($"✅ Loaded {_transactions.Count} transactions for {_selectedDate:dd/MM/yyyy}");
                _isLoading = false;
            }
            catch (Exception ex)
            {
                _isLoading = false;
                Debug.WriteLine($"🔴 LoadTransactions Error: {ex.Message}");
            }
        }

        private void UpdateTransactionListTitle()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            if (_selectedDate.Date == today)
                TransactionListTitle.Text = "Giao dịch hôm nay";
            else if (_selectedDate.Date == yesterday)
                TransactionListTitle.Text = "Giao dịch hôm qua";
            else
                TransactionListTitle.Text = $"Giao dịch ngày {_selectedDate:dd/MM/yyyy}";
        }

        private void OnExpenseTabClicked(object sender, EventArgs e)
        {
            if (_selectedType != TransactionType.Expense)
            {
                _selectedType = TransactionType.Expense;
                UpdateTabSelection();
                _ = LoadCategoriesAsync();
            }
        }

        private void OnIncomeTabClicked(object sender, EventArgs e)
        {
            if (_selectedType != TransactionType.Income)
            {
                _selectedType = TransactionType.Income;
                UpdateTabSelection();
                _ = LoadCategoriesAsync();
            }
        }

        private void UpdateTabSelection()
        {
            ExpenseTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
            ExpenseTabButton.TextColor = Color.FromArgb("#666666");
            IncomeTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
            IncomeTabButton.TextColor = Color.FromArgb("#666666");

            switch (_selectedType)
            {
                case TransactionType.Expense:
                    ExpenseTabButton.BackgroundColor = Color.FromArgb("#4ECDC4");
                    ExpenseTabButton.TextColor = Colors.White;
                    break;
                case TransactionType.Income:
                    IncomeTabButton.BackgroundColor = Color.FromArgb("#4ECDC4");
                    IncomeTabButton.TextColor = Colors.White;
                    break;
            }
        }

        private void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Deselect previous category
            if (_selectedCategory != null)
            {
                _selectedCategory.IsSelected = false;
            }

            if (e.CurrentSelection.Count > 0)
            {
                _selectedCategory = e.CurrentSelection[0] as CategoryViewModel;
                if (_selectedCategory != null)
                {
                    _selectedCategory.IsSelected = true;
                    Debug.WriteLine($"✅ Selected category: {_selectedCategory.Name}");
                }
            }
        }

        private async void OnWalletChanged(object sender, EventArgs e)
        {
            if (WalletPicker.SelectedItem is Wallet wallet)
            {
                _selectedWallet = wallet;
                UpdateWalletHeader();

                // Reload transactions and summary for new wallet
                await LoadTransactionsAsync();

                Debug.WriteLine($"✅ Wallet changed to: {wallet.Name}");
            }
        }

        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _selectedDate = e.NewDate;
            UpdateTransactionListTitle();
            await LoadTransactionsAsync();
        }

        private async void OnPreviousDayClicked(object sender, EventArgs e)
        {
            _selectedDate = _selectedDate.AddDays(-1);
            TransactionDatePicker.Date = _selectedDate;
            UpdateTransactionListTitle();
            await LoadTransactionsAsync();
        }

        private async void OnNextDayClicked(object sender, EventArgs e)
        {
            _selectedDate = _selectedDate.AddDays(1);
            TransactionDatePicker.Date = _selectedDate;
            UpdateTransactionListTitle();
            await LoadTransactionsAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AmountEntry.Text))
                {
                    await DisplayAlert("Lỗi", "Vui lòng nhập số tiền", "OK");
                    return;
                }

                if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
                {
                    await DisplayAlert("Lỗi", "Số tiền không hợp lệ", "OK");
                    return;
                }

                if (_selectedCategory == null)
                {
                    await DisplayAlert("Lỗi", "Vui lòng chọn danh mục", "OK");
                    return;
                }

                if (_selectedWallet == null)
                {
                    await DisplayAlert("Lỗi", "Vui lòng chọn ví", "OK");
                    return;
                }

                SaveButton.IsEnabled = false;
                SaveButton.Text = "Đang lưu...";

                var transaction = new Transaction
                {
                    Type = _selectedType,
                    Amount = amount,
                    Date = _selectedDate,
                    Note = NoteEntry.Text,
                    CategoryId = _selectedCategory.Id,
                    WalletId = _selectedWallet.Id
                };

                await _database.SaveTransactionAsync(transaction);

                // Update wallet balance
                var balanceChange = _selectedType == TransactionType.Income ? amount : -amount;
                await _database.UpdateWalletBalanceAsync(_selectedWallet.Id, balanceChange);

                // Reset form
                AmountEntry.Text = string.Empty;
                NoteEntry.Text = string.Empty;
                if (_selectedCategory != null)
                {
                    _selectedCategory.IsSelected = false;
                }
                CategoriesCollectionView.SelectedItem = null;
                _selectedCategory = null;

                // Reload data
                UpdateWalletHeader();
                await LoadTransactionsAsync();

                SaveButton.IsEnabled = true;
                SaveButton.Text = "Lưu giao dịch";

                await DisplayAlert("Thành công", "Đã lưu giao dịch", "OK");
            }
            catch (Exception ex)
            {
                SaveButton.IsEnabled = true;
                SaveButton.Text = "Lưu giao dịch";
                Debug.WriteLine($"🔴 Save Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể lưu: {ex.Message}", "OK");
            }
        }

        private async void OnTransactionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                var item = e.CurrentSelection[0] as TransactionDisplayItem;

                // Deselect immediately
                TransactionsCollectionView.SelectedItem = null;

                if (item != null)
                {
                    await Navigation.PushAsync(new TransactionDetailPage(item.Id));
                }
            }
        }

        private async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
        {
            try
            {
                var swipeItem = sender as SwipeItem;
                var item = swipeItem?.BindingContext as TransactionDisplayItem;

                if (item == null) return;

                bool confirm = await DisplayAlert("Xác nhận",
                    "Bạn có chắc muốn xóa giao dịch này?",
                    "Xóa", "Hủy");

                if (!confirm) return;

                var transaction = await _database.GetTransactionAsync(item.Id);
                if (transaction != null)
                {
                    // Revert balance
                    if (transaction.WalletId.HasValue)
                    {
                        var revertAmount = transaction.Type == TransactionType.Income
                            ? -transaction.Amount
                            : transaction.Amount;
                        await _database.UpdateWalletBalanceAsync(transaction.WalletId.Value, revertAmount);
                    }

                    await _database.DeleteTransactionAsync(transaction);

                    UpdateWalletHeader();
                    await LoadTransactionsAsync();

                    await DisplayAlert("Thành công", "Đã xóa giao dịch", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Delete Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_isInitialized && !_isLoading)
            {
                _isLoading = true; // Prevent concurrent loads

                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Reload categories to get any updates from ManageCategoriesPage
                        await LoadCategoriesAsync();
                        UpdateWalletHeader();
                        await LoadTransactionsAsync();
                    }
                    finally
                    {
                        _isLoading = false;
                    }
                });
            }
        }
    }
}