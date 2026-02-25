using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System;
using System.Diagnostics;
using System.IO;

namespace ExpenseTracker.Views
{
    public partial class WalletPage : ContentPage
    {
        private readonly WalletViewModel _viewModel;

        public WalletPage()
        {
            try
            {
                InitializeComponent();

                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbPath = Path.Combine(appDataPath, "ExpenseTracker", "expensetracker.db3");
                var database = new DatabaseService(dbPath);

                _viewModel = new WalletViewModel(database);
                BindingContext = _viewModel;

                SetupBindings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 WalletPage Error: {ex.Message}");
            }
        }

        private void SetupBindings()
        {
            WalletsCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(WalletViewModel.Wallets));
            TotalBalanceLabel.SetBinding(Label.TextProperty, nameof(WalletViewModel.TotalBalance), stringFormat: "{0:N0}đ");
            WalletCountLabel.SetBinding(Label.TextProperty, nameof(WalletViewModel.WalletCount));
            TotalBudgetLabel.SetBinding(Label.TextProperty, nameof(WalletViewModel.TotalBudget), stringFormat: "{0:N0}đ");
            EmptyStateLayout.SetBinding(VisualElement.IsVisibleProperty, nameof(WalletViewModel.IsEmptyStateVisible));
        }

        private async void OnAddWalletClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddWalletPage());
        }

        private async void OnWalletSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                var wallet = e.CurrentSelection[0] as WalletDisplayItem;
                if (wallet != null)
                {
                    await Navigation.PushAsync(new WalletDetailPage(wallet.Id));
                }

                WalletsCollectionView.SelectedItem = null;
            }
        }

        private async void OnEditWalletTapped(object sender, EventArgs e)
        {
            if (sender is Label label && label.BindingContext is WalletDisplayItem wallet)
            {
                await Navigation.PushAsync(new WalletDetailPage(wallet.Id));
            }
        }

        private async void OnDeleteWalletTapped(object sender, EventArgs e)
        {
            if (sender is Label label && label.BindingContext is WalletDisplayItem wallet)
            {
                bool confirm = await DisplayAlert(
                    "Xác nhận xóa",
                    $"Bạn có chắc muốn xóa ví '{wallet.Name}'?\nTất cả giao dịch liên quan sẽ bị ảnh hưởng.",
                    "Xóa",
                    "Hủy");

                if (confirm)
                {
                    try
                    {
                        await _viewModel.DeleteWalletAsync(wallet);
                        await DisplayAlert("Thành công", "Đã xóa ví", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Lỗi", $"Không thể xóa ví: {ex.Message}", "OK");
                    }
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadWalletsAsync();
        }
    }
}