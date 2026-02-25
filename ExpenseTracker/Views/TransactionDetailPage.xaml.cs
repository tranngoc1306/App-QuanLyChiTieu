using ExpenseTracker.Models;
using ExpenseTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExpenseTracker.Views
{
    public partial class TransactionDetailPage : ContentPage
    {
        private DatabaseService _database;
        private Transaction _transaction;
        private TransactionType _selectedType;
        private CategoryViewModel _selectedCategory;
        private Wallet _selectedWallet;
        private Wallet _originalWallet;
        private decimal _originalAmount;
        private TransactionType _originalType;
        private bool _isLoading = false;
        private bool _isInitialized = false;

        private ObservableCollection<CategoryViewModel> _categories = new ObservableCollection<CategoryViewModel>();
        private ObservableCollection<Wallet> _wallets = new ObservableCollection<Wallet>();

        public TransactionDetailPage(int transactionId)
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

                // Load transaction
                LoadTransactionAsync(transactionId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 TransactionDetailPage Error: {ex.Message}");
            }
        }

        private async void LoadTransactionAsync(int transactionId)
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;
                await _database.InitializeDatabaseAsync();

                // Load transaction
                _transaction = await _database.GetTransactionAsync(transactionId);

                if (_transaction == null)
                {
                    await DisplayAlert("Lỗi", "Không tìm thấy giao dịch", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                // Store original values for balance calculation
                _originalAmount = _transaction.Amount;
                _originalType = _transaction.Type;
                _selectedType = _transaction.Type;

                // Load wallets first
                await LoadWalletsAsync();

                // Load categories for current type
                await LoadCategoriesAsync();

                // Load selected category
                var category = await _database.GetCategoryAsync(_transaction.CategoryId);
                if (category != null)
                {
                    _selectedCategory = _categories.FirstOrDefault(c => c.Id == category.Id);
                    if (_selectedCategory != null)
                    {
                        _selectedCategory.IsSelected = true;
                        CategoriesCollectionView.SelectedItem = _selectedCategory;
                    }
                }

                // Populate form
                TransactionDatePicker.Date = _transaction.Date;
                AmountEntry.Text = _transaction.Amount.ToString();
                NoteEntry.Text = _transaction.Note;

                // Update tab selection
                UpdateTabSelection();

                _isInitialized = true;
                _isLoading = false;
                Debug.WriteLine("✅ Transaction loaded successfully");
            }
            catch (Exception ex)
            {
                _isLoading = false;
                Debug.WriteLine($"🔴 LoadTransaction Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể tải giao dịch: {ex.Message}", "OK");
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

                // Select current wallet
                if (_transaction.WalletId.HasValue)
                {
                    _selectedWallet = _wallets.FirstOrDefault(w => w.Id == _transaction.WalletId.Value);
                    _originalWallet = _selectedWallet;

                    if (_selectedWallet != null)
                    {
                        var index = _wallets.ToList().FindIndex(w => w.Id == _selectedWallet.Id);
                        if (index >= 0)
                        {
                            WalletPicker.SelectedIndex = index;
                        }
                        UpdateWalletHeader();
                    }
                }
                else if (_wallets.Count > 0)
                {
                    _selectedWallet = _wallets[0];
                    WalletPicker.SelectedIndex = 0;
                    UpdateWalletHeader();
                }

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

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        WalletNameLabel.Text = _selectedWallet.Name;
                        WalletBalanceLabel.Text = CurrencyService.FormatAmount(_selectedWallet.Balance);
                        WalletBalanceLabel.TextColor = _selectedWallet.Balance >= 0
                            ? Color.FromArgb("#26DE81")
                            : Color.FromArgb("#FC5C65");
                    });
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

                // Store current selection
                var currentSelectedId = _selectedCategory?.Id;

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

                // Re-select if there was a selection
                if (currentSelectedId.HasValue)
                {
                    var reselect = _categories.FirstOrDefault(c => c.Id == currentSelectedId.Value);
                    if (reselect != null)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            reselect.IsSelected = true;
                            _selectedCategory = reselect;
                            CategoriesCollectionView.SelectedItem = reselect;
                        });
                    }
                }

                Debug.WriteLine($"✅ Loaded {_categories.Count} categories for {_selectedType}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadCategories Error: {ex.Message}");
            }
        }

        private async void OnExpenseTabClicked(object sender, EventArgs e)
        {
            if (_selectedType != TransactionType.Expense)
            {
                _selectedType = TransactionType.Expense;
                UpdateTabSelection();

                // Deselect old category
                if (_selectedCategory != null)
                {
                    _selectedCategory.IsSelected = false;
                    _selectedCategory = null;
                }

                await LoadCategoriesAsync();
            }
        }

        private async void OnIncomeTabClicked(object sender, EventArgs e)
        {
            if (_selectedType != TransactionType.Income)
            {
                _selectedType = TransactionType.Income;
                UpdateTabSelection();

                // Deselect old category
                if (_selectedCategory != null)
                {
                    _selectedCategory.IsSelected = false;
                    _selectedCategory = null;
                }

                await LoadCategoriesAsync();
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

        private void OnWalletChanged(object sender, EventArgs e)
        {
            if (WalletPicker.SelectedItem is Wallet wallet)
            {
                _selectedWallet = wallet;
                UpdateWalletHeader();
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Validation
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

                // Revert original balance change
                if (_originalWallet != null)
                {
                    var revertAmount = _originalType == TransactionType.Income ? -_originalAmount : _originalAmount;
                    await _database.UpdateWalletBalanceAsync(_originalWallet.Id, revertAmount);
                }

                // Update transaction
                _transaction.Type = _selectedType;
                _transaction.Amount = amount;
                _transaction.Date = TransactionDatePicker.Date;
                _transaction.Note = NoteEntry.Text;
                _transaction.CategoryId = _selectedCategory.Id;
                _transaction.WalletId = _selectedWallet.Id;

                await _database.SaveTransactionAsync(_transaction);

                // Apply new balance change
                var balanceChange = _selectedType == TransactionType.Income ? amount : -amount;
                await _database.UpdateWalletBalanceAsync(_selectedWallet.Id, balanceChange);

                SaveButton.IsEnabled = true;
                SaveButton.Text = "Lưu";

                await DisplayAlert("Thành công", "Đã cập nhật giao dịch", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                SaveButton.IsEnabled = true;
                SaveButton.Text = "Lưu";
                Debug.WriteLine($"🔴 Save Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể lưu: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            try
            {
                bool confirm = await DisplayAlert(
                    "Xác nhận xóa",
                    "Bạn có chắc muốn xóa giao dịch này?",
                    "Xóa",
                    "Hủy");

                if (!confirm) return;

                DeleteButton.IsEnabled = false;

                // Revert balance change
                if (_originalWallet != null)
                {
                    var revertAmount = _originalType == TransactionType.Income ? -_originalAmount : _originalAmount;
                    await _database.UpdateWalletBalanceAsync(_originalWallet.Id, revertAmount);
                }

                // Delete transaction
                await _database.DeleteTransactionAsync(_transaction);

                await DisplayAlert("Thành công", "Đã xóa giao dịch", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                DeleteButton.IsEnabled = true;
                Debug.WriteLine($"🔴 Delete Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Reload categories when returning to this page (e.g., from ManageCategoriesPage)
            if (_isInitialized && !_isLoading)
            {
                _isLoading = true; // Prevent concurrent loads

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadCategoriesAsync();
                        UpdateWalletHeader();
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