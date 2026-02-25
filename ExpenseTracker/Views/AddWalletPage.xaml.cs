using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System;
using System.IO;

namespace ExpenseTracker.Views
{
    public partial class AddWalletPage : ContentPage
    {
        private readonly AddWalletViewModel _viewModel;

        public AddWalletPage()
        {
            InitializeComponent();

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appDataPath, "ExpenseTracker", "expensetracker.db3");
            var database = new DatabaseService(dbPath);

            _viewModel = new AddWalletViewModel(database);
            BindingContext = _viewModel;

            SetupBindings();
            SubscribeToEvents();
        }

        private void SetupBindings()
        {
            NameEntry.SetBinding(Entry.TextProperty, nameof(AddWalletViewModel.Name));
            BalanceEntry.SetBinding(Entry.TextProperty, nameof(AddWalletViewModel.BalanceText));
            BudgetEntry.SetBinding(Entry.TextProperty, nameof(AddWalletViewModel.BudgetText));
            IsDefaultCheckBox.SetBinding(CheckBox.IsCheckedProperty, nameof(AddWalletViewModel.IsDefault));

            IconsCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(AddWalletViewModel.Icons));
            IconsCollectionView.SetBinding(CollectionView.SelectedItemProperty, nameof(AddWalletViewModel.SelectedIcon));

            ColorsCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(AddWalletViewModel.Colors));
            ColorsCollectionView.SetBinding(CollectionView.SelectedItemProperty, nameof(AddWalletViewModel.SelectedColor));
        }

        private void SubscribeToEvents()
        {
            _viewModel.ValidationError += OnValidationError;
            _viewModel.SaveCompleted += OnSaveCompleted;
        }

        private async void OnValidationError(object sender, string message)
        {
            await DisplayAlert("Lỗi", message, "OK");
        }

        private async void OnSaveCompleted(object sender, EventArgs e)
        {
            await DisplayAlert("Thành công", "Đã thêm ví mới", "OK");
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

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_viewModel.SaveCommand.CanExecute(null))
            {
                _viewModel.SaveCommand.Execute(null);
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.ValidationError -= OnValidationError;
            _viewModel.SaveCompleted -= OnSaveCompleted;
        }
    }
}