using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System.Diagnostics;
using System.IO;

namespace ExpenseTracker.Views
{
    public partial class CalendarPage : ContentPage
    {
        private readonly CalendarViewModel _viewModel;

        public CalendarPage()
        {
            try
            {
                Debug.WriteLine("🟢 CalendarPage: Constructor started");

                InitializeComponent();
                Debug.WriteLine("✅ CalendarPage: InitializeComponent completed");

                // Initialize database service
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbPath = Path.Combine(appDataPath, "ExpenseTracker", "expensetracker.db3");

                Debug.WriteLine($"📁 Database path: {dbPath}");

                var database = new DatabaseService(dbPath);

                // Initialize ViewModel
                _viewModel = new CalendarViewModel(database);

                Debug.WriteLine("✅ CalendarPage: ViewModel created");

                // Set BindingContext
                BindingContext = _viewModel;

                // Subscribe to property changes
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                Debug.WriteLine("✅ CalendarPage: BindingContext set");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 CalendarPage Constructor Error: {ex.Message}");
                Debug.WriteLine($"🔴 StackTrace: {ex.StackTrace}");
                DisplayAlert("Lỗi", $"Không thể khởi tạo Calendar: {ex.Message}", "OK");
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Update colors when properties change
            if (e.PropertyName == nameof(_viewModel.Balance))
            {
                UpdateBalanceColor();
            }
            else if (e.PropertyName == nameof(_viewModel.SelectedWallet))
            {
                UpdateWalletBalanceColor();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Debug.WriteLine("🟢 CalendarPage: OnAppearing started");

            try
            {
                await _viewModel.InitializeAsync();

                Debug.WriteLine("✅ CalendarPage: InitializeAsync completed");
                Debug.WriteLine($"🔍 CalendarDays: {_viewModel.CalendarDays?.Count ?? -1}");
                Debug.WriteLine($"🔍 TransactionGroups: {_viewModel.TransactionGroups?.Count ?? -1}");

                // Update colors after initialization
                UpdateBalanceColor();
                UpdateWalletBalanceColor();

                // Force UI update
                OnPropertyChanged(nameof(BindingContext));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 OnAppearing Error: {ex.Message}");
                Debug.WriteLine($"🔴 StackTrace: {ex.StackTrace}");
                await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
        }

        private void UpdateBalanceColor()
        {
            if (BalanceLabel != null)
            {
                BalanceLabel.TextColor = _viewModel.Balance >= 0
                    ? Color.FromArgb("#26DE81")
                    : Color.FromArgb("#FC5C65");
            }
        }

        private void UpdateWalletBalanceColor()
        {
            if (WalletBalanceLabel != null && _viewModel.SelectedWallet != null)
            {
                WalletBalanceLabel.TextColor = _viewModel.SelectedWallet.Balance >= 0
                    ? Color.FromArgb("#26DE81")
                    : Color.FromArgb("#FC5C65");
            }
        }

        private async void OnWalletChanged(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("🟢 Wallet changed");
                UpdateWalletBalanceColor();
                await _viewModel.OnWalletChangedAsync();
                UpdateBalanceColor();
                Debug.WriteLine("✅ Wallet changed completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 OnWalletChanged Error: {ex.Message}");
            }
        }

        private async void OnPreviousMonthClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("🟢 Previous month clicked");
                await _viewModel.PreviousMonthCommand.ExecuteAsync(null);
                UpdateBalanceColor();
                Debug.WriteLine("✅ Previous month completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 OnPreviousMonthClicked Error: {ex.Message}");
            }
        }

        private async void OnNextMonthClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("🟢 Next month clicked");
                await _viewModel.NextMonthCommand.ExecuteAsync(null);
                UpdateBalanceColor();
                Debug.WriteLine("✅ Next month completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 OnNextMonthClicked Error: {ex.Message}");
            }
        }

        private async void OnDaySelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection.Count > 0)
                {
                    var day = e.CurrentSelection[0] as CalendarDay;
                    if (day != null && day.IsCurrentMonth)
                    {
                        Debug.WriteLine($"🟢 Day selected: {day.Date:dd/MM/yyyy}");

                        
                    }

                    // Deselect
                    ((CollectionView)sender).SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 OnDaySelected Error: {ex.Message}");
            }
        }

        private async void OnTransactionSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection.Count > 0)
                {
                    var item = e.CurrentSelection[0] as TransactionDisplayItem;

                    // Deselect ngay lập tức
                    ((CollectionView)sender).SelectedItem = null;

                    if (item != null)
                    {
                        Debug.WriteLine($"🟢 Transaction selected: {item.Id}");
                        // Chuyển sang trang chi tiết giao dịch
                        await Navigation.PushAsync(new TransactionDetailPage(item.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 OnTransactionSelected Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể mở chi tiết giao dịch: {ex.Message}", "OK");
            }
        }


        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("🟢 Add transaction button clicked");

                // Chuyển sang trang giao dịch
                await Navigation.PushAsync(new TransactionPage());

                Debug.WriteLine("✅ Navigated to TransactionPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 OnAddTransactionClicked Error: {ex.Message}");
                await DisplayAlert("Lỗi", $"Không thể mở trang giao dịch: {ex.Message}", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe from events
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }
    }
}