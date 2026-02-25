using ExpenseTracker.Services;
using System;
using System.Diagnostics;

namespace ExpenseTracker.Views
{
    public partial class EditProfilePage : ContentPage
    {
        private readonly DatabaseService _database;
        private string _selectedAvatar;

        public EditProfilePage()
        {
            InitializeComponent();

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expensetracker.db3");
            _database = new DatabaseService(dbPath);

            LoadCurrentProfile();
        }

        private void LoadCurrentProfile()
        {
            try
            {
                var userName = Preferences.Get("UserName", "User");
                var userAvatar = Preferences.Get("UserAvatar", "👤");

                DisplayNameEntry.Text = userName;
                CurrentAvatarLabel.Text = userAvatar;
                _selectedAvatar = userAvatar;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadCurrentProfile Error: {ex.Message}");
            }
        }

        private void OnAvatarSelected(object sender, EventArgs e)
        {
            if (sender is Label label && label.GestureRecognizers[0] is TapGestureRecognizer tap)
            {
                _selectedAvatar = tap.CommandParameter as string;
                CurrentAvatarLabel.Text = _selectedAvatar;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                var displayName = DisplayNameEntry.Text?.Trim();

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    await DisplayAlert(
                        LocalizationService.GetString("Error"),
                        LocalizationService.GetString("EnterName"),
                        LocalizationService.GetString("OK"));
                    return;
                }

                SaveButton.IsEnabled = false;
                SaveButton.Text = "Đang lưu...";

                // Update database
                await _database.InitializeDatabaseAsync();
                var userId = Preferences.Get("UserId", 0);
                var user = await _database.GetUserAsync(userId);

                if (user != null)
                {
                    user.FullName = displayName;
                    await _database.SaveUserAsync(user);
                }

                // Update preferences
                Preferences.Set("UserName", displayName);
                Preferences.Set("UserAvatar", _selectedAvatar);

                await DisplayAlert(
                    LocalizationService.GetString("Success"),
                    LocalizationService.GetString("UpdateSuccess"),
                    LocalizationService.GetString("OK"));

                // Navigate back and refresh settings page
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 Save Profile Error: {ex.Message}");
                await DisplayAlert(
                    LocalizationService.GetString("Error"),
                    LocalizationService.GetString("UpdateFailed"),
                    LocalizationService.GetString("OK"));
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Text = "Lưu thay đổi";
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}