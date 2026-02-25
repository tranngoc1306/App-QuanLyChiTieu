using ExpenseTracker.Models;
using ExpenseTracker.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ExpenseTracker.Views
{
    public partial class ManageCategoriesPage : ContentPage
    {
        private readonly DatabaseService _database;
        private TransactionType _currentType = TransactionType.Expense;

        public ManageCategoriesPage()
        {
            InitializeComponent();

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expensetracker.db3");
            _database = new DatabaseService(dbPath);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCategories();
        }

        private async System.Threading.Tasks.Task LoadCategories()
        {
            try
            {
                await _database.InitializeDatabaseAsync();
                var categories = await _database.GetCategoriesAsync(_currentType);
                CategoriesCollectionView.ItemsSource = categories;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 LoadCategories Error: {ex.Message}");
            }
        }

        private async void OnExpenseTabTapped(object sender, EventArgs e)
        {
            _currentType = TransactionType.Expense;
            ExpenseTabFrame.BackgroundColor = Color.FromArgb("#4ECDC4");
            IncomeTabFrame.BackgroundColor = Colors.White;
            await LoadCategories();
        }

        private async void OnIncomeTabTapped(object sender, EventArgs e)
        {
            _currentType = TransactionType.Income;
            ExpenseTabFrame.BackgroundColor = Colors.White;
            IncomeTabFrame.BackgroundColor = Color.FromArgb("#4ECDC4");
            await LoadCategories();
        }

        private async void OnAddCategoryClicked(object sender, EventArgs e)
        {
            await ShowCategoryEditorAsync(null);
        }

        private async void OnEditCategory(object sender, EventArgs e)
        {
            if (sender is Label label && label.BindingContext is Category category)
            {
                await ShowCategoryEditorAsync(category);
            }
        }

        private async void OnDeleteCategory(object sender, EventArgs e)
        {
            try
            {
                if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Category category)
                {
                    var confirm = await DisplayAlert(
                        LocalizationService.GetString("DeleteCategory"),
                        LocalizationService.GetString("DeleteCategoryConfirm"),
                        LocalizationService.GetString("Delete"),
                        LocalizationService.GetString("Cancel"));

                    if (confirm)
                    {
                        await _database.DeleteAsync(category);
                        await DisplayAlert(
                            LocalizationService.GetString("Success"),
                            LocalizationService.GetString("CategoryDeleteSuccess"),
                            LocalizationService.GetString("OK"));
                        await LoadCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 DeleteCategory Error: {ex.Message}");
                await DisplayAlert(
                    LocalizationService.GetString("Error"),
                    ex.Message,
                    LocalizationService.GetString("OK"));
            }
        }

        private async System.Threading.Tasks.Task ShowCategoryEditorAsync(Category category)
        {
            var isEdit = category != null;
            var title = isEdit ? LocalizationService.GetString("EditCategory") : LocalizationService.GetString("AddCategory");

            var name = await DisplayPromptAsync(
                title,
                LocalizationService.GetString("CategoryName"),
                initialValue: category?.Name ?? "",
                maxLength: 50,
                keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(name))
                return;

            // Icon selection
            var icons = new[] { "🍳", "👨‍👩‍👧", "🚗", "👕", "🛍️", "🎮", "🅿️", "⛽", "📞", "❤️", "📚", "💑", "💰", "❓", "💵", "🎁", "📈", "🏛️", "🤝" };
            var selectedIcon = await DisplayActionSheet(
                LocalizationService.GetString("SelectIcon"),
                LocalizationService.GetString("Cancel"),
                null,
                icons);

            if (string.IsNullOrEmpty(selectedIcon) || selectedIcon == LocalizationService.GetString("Cancel"))
                return;

            // Color selection
            var colors = new Dictionary<string, string>
            {
                ["Đỏ"] = "#FF6B6B",
                ["Xanh dương"] = "#4ECDC4",
                ["Vàng"] = "#FFE66D",
                ["Hồng"] = "#FF6B9D",
                ["Cam"] = "#FFA502",
                ["Tím"] = "#A29BFE",
                ["Xanh lá"] = "#26DE81",
                ["Xám"] = "#95A5A6"
            };

            var selectedColorName = await DisplayActionSheet(
                LocalizationService.GetString("SelectColor"),
                LocalizationService.GetString("Cancel"),
                null,
                colors.Keys.ToArray());

            if (string.IsNullOrEmpty(selectedColorName) || selectedColorName == LocalizationService.GetString("Cancel"))
                return;

            var selectedColor = colors[selectedColorName];

            try
            {
                if (isEdit)
                {
                    category.Name = name;
                    category.Icon = selectedIcon;
                    category.Color = selectedColor;
                    await _database.UpdateAsync(category);
                }
                else
                {
                    var newCategory = new Category
                    {
                        Name = name,
                        Icon = selectedIcon,
                        Color = selectedColor,
                        Type = _currentType
                    };
                    await _database.InsertAsync(newCategory);
                }

                await DisplayAlert(
                    LocalizationService.GetString("Success"),
                    LocalizationService.GetString("CategorySaveSuccess"),
                    LocalizationService.GetString("OK"));

                await LoadCategories();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔴 SaveCategory Error: {ex.Message}");
                await DisplayAlert(
                    LocalizationService.GetString("Error"),
                    ex.Message,
                    LocalizationService.GetString("OK"));
            }
        }
    }
}