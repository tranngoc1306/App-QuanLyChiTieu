using ExpenseTracker.Helpers;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System;
using System.Diagnostics;
using System.IO;

namespace ExpenseTracker.Views
{
    public partial class ReportPage : ContentPage
    {
        private readonly ReportViewModel _viewModel;
        private ChartDrawableHelper _chartDrawable;

        public ReportPage()
        {
            try
            {
                InitializeComponent();

                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbPath = Path.Combine(appDataPath, "ExpenseTracker", "expensetracker.db3");
                var database = new DatabaseService(dbPath);

                _viewModel = new ReportViewModel(database);
                BindingContext = _viewModel;

                _chartDrawable = new ChartDrawableHelper(_viewModel);
                ChartCanvas.Drawable = _chartDrawable;

                SetupBindings();
                SubscribeToEvents();

                _ = _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 ReportPage Error: {ex.Message}");
            }
        }

        private void SetupBindings()
        {
            // Wallet picker
            WalletPicker.SetBinding(Picker.ItemsSourceProperty, nameof(ReportViewModel.Wallets));
            WalletPicker.SetBinding(Picker.SelectedItemProperty, nameof(ReportViewModel.SelectedWallet));
            WalletPicker.ItemDisplayBinding = new Binding("Name");

            // Period labels
            PeriodLabel.SetBinding(Label.TextProperty, nameof(ReportViewModel.PeriodLabel));
            MainChartTitle.SetBinding(Label.TextProperty, nameof(ReportViewModel.MainChartTitle));
            XAxisTitle.SetBinding(Label.TextProperty, nameof(ReportViewModel.XAxisTitle));
            PieChartTitle.SetBinding(Label.TextProperty, nameof(ReportViewModel.PieChartTitle));

            // Summary values
            TotalIncomeLabel.SetBinding(Label.TextProperty,
                new Binding(nameof(ReportViewModel.TotalIncome), stringFormat: "{0:N0}đ"));
            TotalExpenseLabel.SetBinding(Label.TextProperty,
                new Binding(nameof(ReportViewModel.TotalExpense), stringFormat: "{0:N0}đ"));
            NetIncomeLabel.SetBinding(Label.TextProperty,
                new Binding(nameof(ReportViewModel.NetIncome), stringFormat: "{0:N0}đ"));
            NetIncomeLabel.SetBinding(Label.TextColorProperty, nameof(ReportViewModel.NetIncomeColor));

            // Chart controls
            ShowIncomeCheckbox.SetBinding(CheckBox.IsCheckedProperty, nameof(ReportViewModel.ShowIncome));
            ShowExpenseCheckbox.SetBinding(CheckBox.IsCheckedProperty, nameof(ReportViewModel.ShowExpense));

            // Visibility
            EmptyStateLayout.SetBinding(VisualElement.IsVisibleProperty, nameof(ReportViewModel.IsEmptyStateVisible));
            ChartCanvas.SetBinding(VisualElement.IsVisibleProperty, nameof(ReportViewModel.IsChartVisible));
            CategoryChartView.SetBinding(VisualElement.IsVisibleProperty, nameof(ReportViewModel.IsChartVisible));

            // Category chart
            CategoryChartView.SetBinding(Microcharts.Maui.ChartView.ChartProperty, nameof(ReportViewModel.CategoryChart));
            CategoryLegendView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ReportViewModel.CategoryLegend));

            // Chart dimensions
            ChartCanvas.SetBinding(VisualElement.WidthRequestProperty, nameof(ReportViewModel.ChartWidth));
        }

        private void SubscribeToEvents()
        {
            _viewModel.ChartDataChanged += OnChartDataChanged;
            _viewModel.ErrorOccurred += OnErrorOccurred;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReportViewModel.Labels))
            {
                UpdateXAxisLabels();
            }
            else if (e.PropertyName == nameof(ReportViewModel.IncomeData) ||
                     e.PropertyName == nameof(ReportViewModel.ExpenseData))
            {
                UpdateYAxisLabels();
            }
        }

        private void UpdateXAxisLabels()
        {
            XAxisLabels.Children.Clear();
            var labelWidth = _viewModel.LabelWidth;

            foreach (var label in _viewModel.Labels)
            {
                XAxisLabels.Children.Add(new Label
                {
                    Text = label,
                    FontSize = 11,
                    TextColor = Colors.Gray,
                    HorizontalTextAlignment = TextAlignment.Center,
                    WidthRequest = labelWidth,
                    HeightRequest = 30,
                    VerticalTextAlignment = TextAlignment.Center
                });
            }
        }

        private void UpdateYAxisLabels()
        {
            YAxisLabels.Children.Clear();

            var maxValue = _viewModel.GetMaxValue();
            if (maxValue == 0) maxValue = 1000000;

            var step = (double)maxValue / 5;

            for (int i = 5; i >= 0; i--)
            {
                var value = (decimal)(step * i);
                string label = _viewModel.FormatYAxisLabel(value);

                YAxisLabels.Children.Add(new Label
                {
                    Text = label,
                    FontSize = 11,
                    TextColor = Colors.Gray,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Center,
                    HeightRequest = 50
                });
            }
        }

        private void OnChartDataChanged(object sender, EventArgs e)
        {
            ChartCanvas.Invalidate();
        }

        private async void OnErrorOccurred(object sender, string message)
        {
            await DisplayAlert("Lỗi", message, "OK");
        }

        private void OnWalletChanged(object sender, EventArgs e)
        {
            // Handled by binding
        }

        private void OnMonthViewClicked(object sender, EventArgs e)
        {
            if (!_viewModel.IsMonthView)
            {
                _viewModel.SwitchToMonthViewCommand.Execute(null);
                UpdateViewButtons(true);
            }
        }

        private void OnYearViewClicked(object sender, EventArgs e)
        {
            if (_viewModel.IsMonthView)
            {
                _viewModel.SwitchToYearViewCommand.Execute(null);
                UpdateViewButtons(false);
            }
        }

        private void UpdateViewButtons(bool isMonthView)
        {
            if (isMonthView)
            {
                MonthViewButton.BackgroundColor = Color.FromArgb("#4ECDC4");
                MonthViewButton.TextColor = Colors.White;
                YearViewButton.BackgroundColor = Color.FromArgb("#E0E0E0");
                YearViewButton.TextColor = Color.FromArgb("#666");
            }
            else
            {
                YearViewButton.BackgroundColor = Color.FromArgb("#4ECDC4");
                YearViewButton.TextColor = Colors.White;
                MonthViewButton.BackgroundColor = Color.FromArgb("#E0E0E0");
                MonthViewButton.TextColor = Color.FromArgb("#666");
            }
        }

        private void OnPreviousPeriodClicked(object sender, EventArgs e)
        {
            _viewModel.PreviousPeriodCommand.Execute(null);
        }

        private void OnNextPeriodClicked(object sender, EventArgs e)
        {
            _viewModel.NextPeriodCommand.Execute(null);
        }

        private void OnBarChartClicked(object sender, EventArgs e)
        {
            _viewModel.SwitchToBarChartCommand.Execute(null);
            UpdateChartTypeButtons("bar");
        }

        private void OnLineChartClicked(object sender, EventArgs e)
        {
            _viewModel.SwitchToLineChartCommand.Execute(null);
            UpdateChartTypeButtons("line");
        }

        private void OnAreaChartClicked(object sender, EventArgs e)
        {
            _viewModel.SwitchToAreaChartCommand.Execute(null);
            UpdateChartTypeButtons("area");
        }

        private void UpdateChartTypeButtons(string chartType)
        {
            var activeColor = Color.FromArgb("#4ECDC4");
            var inactiveColor = Color.FromArgb("#E0E0E0");
            var activeTextColor = Colors.White;
            var inactiveTextColor = Color.FromArgb("#666");

            BarChartButton.BackgroundColor = chartType == "bar" ? activeColor : inactiveColor;
            BarChartButton.TextColor = chartType == "bar" ? activeTextColor : inactiveTextColor;

            LineChartButton.BackgroundColor = chartType == "line" ? activeColor : inactiveColor;
            LineChartButton.TextColor = chartType == "line" ? activeTextColor : inactiveTextColor;

            AreaChartButton.BackgroundColor = chartType == "area" ? activeColor : inactiveColor;
            AreaChartButton.TextColor = chartType == "area" ? activeTextColor : inactiveTextColor;
        }

        private void OnChartDataToggled(object sender, CheckedChangedEventArgs e)
        {
            ChartCanvas.Invalidate();
        }

        private void OnExpenseToggleClicked(object sender, EventArgs e)
        {
            if (_viewModel.SelectedCategoryType != Models.TransactionType.Expense)
            {
                _viewModel.SwitchToExpenseCommand.Execute(null);
                UpdateCategoryTypeButtons(true);
            }
        }

        private void OnIncomeToggleClicked(object sender, EventArgs e)
        {
            if (_viewModel.SelectedCategoryType != Models.TransactionType.Income)
            {
                _viewModel.SwitchToIncomeCommand.Execute(null);
                UpdateCategoryTypeButtons(false);
            }
        }

        private void UpdateCategoryTypeButtons(bool isExpense)
        {
            var activeColor = Color.FromArgb("#4ECDC4");
            var inactiveColor = Color.FromArgb("#E0E0E0");
            var activeTextColor = Colors.White;
            var inactiveTextColor = Color.FromArgb("#666");

            if (isExpense)
            {
                ExpenseToggleButton.BackgroundColor = activeColor;
                ExpenseToggleButton.TextColor = activeTextColor;
                IncomeToggleButton.BackgroundColor = inactiveColor;
                IncomeToggleButton.TextColor = inactiveTextColor;
            }
            else
            {
                IncomeToggleButton.BackgroundColor = activeColor;
                IncomeToggleButton.TextColor = activeTextColor;
                ExpenseToggleButton.BackgroundColor = inactiveColor;
                ExpenseToggleButton.TextColor = inactiveTextColor;
            }
        }

        /*protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.SelectedWallet != null)
            {
                _ = _viewModel.LoadReportDataCommand.Execute(null);
            }
        }*/

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel.SelectedWallet != null)
            {
                _viewModel.LoadReportDataCommand.Execute(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.ChartDataChanged -= OnChartDataChanged;
            _viewModel.ErrorOccurred -= OnErrorOccurred;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
}