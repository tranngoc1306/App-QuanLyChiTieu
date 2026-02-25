using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System;
using System.IO;

namespace ExpenseTracker.Views
{
    public partial class WalletDetailPage : ContentPage
    {
        private readonly WalletDetailViewModel _viewModel;
        private readonly int _walletId;

        public WalletDetailPage(int walletId)
        {
            InitializeComponent();

            _walletId = walletId;

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appDataPath, "ExpenseTracker", "expensetracker.db3");
            var database = new DatabaseService(dbPath);

            _viewModel = new WalletDetailViewModel(database);
            BindingContext = _viewModel;

            SetupBindings();
            SubscribeToEvents();
        }

        private void SetupBindings()
        {
            NameEntry.SetBinding(Entry.TextProperty, nameof(WalletDetailViewModel.Name));
            BalanceEntry.SetBinding(Entry.TextProperty, nameof(WalletDetailViewModel.BalanceText));
            BudgetEntry.SetBinding(Entry.TextProperty, nameof(WalletDetailViewModel.BudgetText));
            IsDefaultCheckBox.SetBinding(CheckBox.IsCheckedProperty, nameof(WalletDetailViewModel.IsDefault));

            PreviewIconLabel.SetBinding(Label.TextProperty, nameof(WalletDetailViewModel.SelectedIcon));
            PreviewNameLabel.SetBinding(Label.TextProperty, nameof(WalletDetailViewModel.PreviewName));
            PreviewBalanceLabel.SetBinding(Label.TextProperty, nameof(WalletDetailViewModel.PreviewBalance));
            PreviewBalanceLabel.SetBinding(Label.TextColorProperty, nameof(WalletDetailViewModel.PreviewBalanceColor));
            PreviewBudgetLabel.SetBinding(Label.TextProperty, nameof(WalletDetailViewModel.PreviewBudget));

            IconsCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(WalletDetailViewModel.Icons));
            IconsCollectionView.SetBinding(CollectionView.SelectedItemProperty, nameof(WalletDetailViewModel.SelectedIcon));

            ColorsCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(WalletDetailViewModel.Colors));
            ColorsCollectionView.SetBinding(CollectionView.SelectedItemProperty, nameof(WalletDetailViewModel.SelectedColor));
        }

        private void SubscribeToEvents()
        {
            _viewModel.ValidationError += OnValidationError;
            _viewModel.SaveCompleted += OnSaveCompleted;
            _viewModel.DeleteCompleted += OnDeleteCompleted;
            _viewModel.LoadError += OnLoadError;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WalletDetailViewModel.SelectedColor))
                {
                    PreviewIconFrame.BackgroundColor = Color.FromArgb(_viewModel.SelectedColor ?? "#4ECDC4");
                }
            };
        }

        private async void OnValidationError(object sender, string message)
        {
            await DisplayAlert("Lỗi", message, "OK");
        }

        private async void OnSaveCompleted(object sender, EventArgs e)
        {
            await DisplayAlert("Thành công", "Đã cập nhật ví", "OK");
            await Navigation.PopAsync();
        }

        private async void OnDeleteCompleted(object sender, EventArgs e)
        {
            await DisplayAlert("Thành công", "Đã xóa ví", "OK");
            await Navigation.PopAsync();
        }

        private async void OnLoadError(object sender, string message)
        {
            await DisplayAlert("Lỗi", message, "OK");
            await Navigation.PopAsync();
        }

        private void OnIconSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                _viewModel.SelectedIcon = e.CurrentSelection[0] as string;
            }
        }

        private void OnColorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                _viewModel.SelectedColor = e.CurrentSelection[0] as string;
            }
        }

        private void OnPreviewChanged(object sender, TextChangedEventArgs e)
        {
            // Preview updates automatically through bindings
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_viewModel.SaveCommand.CanExecute(null))
            {
                _viewModel.SaveCommand.Execute(null);
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa ví '{_viewModel.Name}'?\nTất cả giao dịch liên quan sẽ bị ảnh hưởng.",
                "Xóa",
                "Hủy");

            if (confirm && _viewModel.DeleteCommand.CanExecute(null))
            {
                _viewModel.DeleteCommand.Execute(null);
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadWalletAsync(_walletId);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.ValidationError -= OnValidationError;
            _viewModel.SaveCompleted -= OnSaveCompleted;
            _viewModel.DeleteCompleted -= OnDeleteCompleted;
            _viewModel.LoadError -= OnLoadError;
        }
    }
}