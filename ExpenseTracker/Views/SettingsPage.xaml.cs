using ExpenseTracker.Services;
using ExpenseTracker.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly DatabaseService _database;

        public SettingsPage()
        {
            InitializeComponent();

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expensetracker.db3");
            _database = new DatabaseService(dbPath);

            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            try
            {
                var userName = Preferences.Get("UserName", "User");
                var userEmail = Preferences.Get("UserEmail", "user@example.com");
                var userAvatar = Preferences.Get("UserAvatar", "👤");
                var currentLanguage = LocalizationService.CurrentLanguage;
                var currentCurrency = CurrencyService.CurrentCurrency;

                UserNameLabel.Text = userName;
                UserEmailLabel.Text = userEmail;
                AvatarLabel.Text = userAvatar;

                // Update language display
                LanguageValueLabel.Text = currentLanguage == "vi" ? "Tiếng Việt" : "English";

                // Update currency display
                CurrencyValueLabel.Text = currentCurrency;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadUserInfo Error: {ex.Message}");
            }
        }

        private async void OnEditProfileTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("editprofile");
        }

        private async void OnChangePasswordTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("changepassword");
        }

        private async void OnLanguageTapped(object sender, EventArgs e)
        {
            var languages = new[] { "Tiếng Việt", "English" };
            var action = await DisplayActionSheet(
                LocalizationService.GetString("Language"),
                LocalizationService.GetString("Cancel"),
                null,
                languages);

            if (!string.IsNullOrEmpty(action) && action != LocalizationService.GetString("Cancel"))
            {
                var languageCode = action == "Tiếng Việt" ? "vi" : "en";
                LocalizationService.SetLanguage(languageCode);

                await DisplayAlert(
                    LocalizationService.GetString("Language"),
                    $"{LocalizationService.GetString("Language")}: {action}\n\nVui lòng khởi động lại ứng dụng để áp dụng thay đổi.",
                    LocalizationService.GetString("OK"));

                LoadUserInfo();
            }
        }

        private async void OnCurrencyTapped(object sender, EventArgs e)
        {
            var currencies = new[] { "VND", "USD", "EUR" };
            var action = await DisplayActionSheet(
                LocalizationService.GetString("Currency"),
                LocalizationService.GetString("Cancel"),
                null,
                currencies);

            if (!string.IsNullOrEmpty(action) && action != LocalizationService.GetString("Cancel"))
            {
                CurrencyService.SetCurrency(action);

                await DisplayAlert(
                    LocalizationService.GetString("Currency"),
                    $"{LocalizationService.GetString("Currency")}: {action}",
                    LocalizationService.GetString("OK"));

                LoadUserInfo();
            }
        }

        /*private async void OnExportDataTapped(object sender, EventArgs e)
        {
            try
            {
                await _database.InitializeDatabaseAsync();

                // Get all transactions
                var transactions = await _database.GetTransactionsAsync();

                if (transactions.Count == 0)
                {
                    await DisplayAlert(
                        LocalizationService.GetString("ExportData"),
                        LocalizationService.GetString("NoDataToExport"),
                        LocalizationService.GetString("OK"));
                    return;
                }

                // Create CSV content
                var csv = new StringBuilder();
                csv.AppendLine("Date,Category,Amount,Type,Note,Wallet");

                foreach (var transaction in transactions)
                {
                    var category = await _database.GetCategoryAsync(transaction.CategoryId);

                    // Fix: Handle nullable WalletId
                    Wallet wallet = null;
                    if (transaction.WalletId.HasValue)
                    {
                        wallet = await _database.GetWalletAsync(transaction.WalletId.Value);
                    }

                    csv.AppendLine($"{transaction.Date:yyyy-MM-dd},{category?.Name ?? "Unknown"}," +
                                 $"{transaction.Amount},{transaction.Type}," +
                                 $"\"{transaction.Note?.Replace("\"", "\"\"") ?? ""}\",{wallet?.Name ?? "Unknown"}");
                }

                // Save to file
                var fileName = $"ExpenseTracker_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                await File.WriteAllTextAsync(filePath, csv.ToString());

                // Try to share the file
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = LocalizationService.GetString("ExportData"),
                    File = new ShareFile(filePath)
                });

                await DisplayAlert(
                    LocalizationService.GetString("Success"),
                    $"{LocalizationService.GetString("ExportSuccess")}\n\n{fileName}",
                    LocalizationService.GetString("OK"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Export Data Error: {ex.Message}");
                await DisplayAlert(
                    LocalizationService.GetString("Error"),
                    LocalizationService.GetString("ExportFailed"),
                    LocalizationService.GetString("OK"));
            }
        }*/

        private async void OnExportDataTapped(object sender, EventArgs e)
        {
            await DisplayAlert(
                "Thông báo",
                "Tính năng đang được phát triển.",
                "OK");
        }


        private async void OnManageCategoriesTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("managecategories");
        }

        private async void OnAboutTapped(object sender, EventArgs e)
        {
            await DisplayAlert(
                LocalizationService.GetString("AboutApp"),
                "Expense Tracker v1.0.0\n\nỨng dụng quản lý chi tiêu cá nhân\n\nPhát triển bởi: Trần Hải Ngọc",
                LocalizationService.GetString("OK"));
        }

        private async void OnDeleteAccountTapped(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                LocalizationService.GetString("DeleteAccount"),
                LocalizationService.GetString("DeleteAccountConfirm"),
                LocalizationService.GetString("DeleteAccountFinal"),
                LocalizationService.GetString("Cancel"));

            if (confirm)
            {
                try
                {
                    await _database.InitializeDatabaseAsync();
                    var userId = Preferences.Get("UserId", 0);

                    // Delete all transactions
                    var transactions = await _database.GetTransactionsAsync();
                    foreach (var transaction in transactions)
                    {
                        await _database.DeleteTransactionAsync(transaction);
                    }

                    // Delete all wallets
                    var wallets = await _database.GetWalletsAsync();
                    foreach (var wallet in wallets)
                    {
                        await _database.DeleteWalletAsync(wallet);
                    }

                    // Delete user account
                    var user = await _database.GetUserAsync(userId);
                    if (user != null)
                    {
                        await _database.DeleteAsync(user);
                    }

                    // Clear preferences and logout
                    Preferences.Clear();

                    await DisplayAlert(
                        LocalizationService.GetString("Success"),
                        LocalizationService.GetString("DeleteAccountSuccess"),
                        LocalizationService.GetString("OK"));

                    // Navigate to login page
                    Application.Current.MainPage = new NavigationPage(new LoginPage())
                    {
                        BarBackgroundColor = Color.FromArgb("#4ECDC4"),
                        BarTextColor = Colors.White
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"🔴 Delete Account Error: {ex.Message}");
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("DeleteAccountFailed"),
                        LocalizationService.GetString("OK"));
                }
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                LocalizationService.GetString("Logout"),
                LocalizationService.GetString("LogoutConfirm"),
                LocalizationService.GetString("Logout"),
                LocalizationService.GetString("Cancel"));

            if (confirm)
            {
                // Clear all preferences
                Preferences.Clear();

                // Navigate to login page
                Application.Current.MainPage = new NavigationPage(new LoginPage())
                {
                    BarBackgroundColor = Color.FromArgb("#4ECDC4"),
                    BarTextColor = Colors.White
                };
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserInfo();
        }
    }
}