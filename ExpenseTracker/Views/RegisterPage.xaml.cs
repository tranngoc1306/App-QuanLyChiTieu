using ExpenseTracker.ViewModels;
using ExpenseTracker.Services;

namespace ExpenseTracker.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = CreateViewModel();
        }

        // Constructor with dependency injection
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private RegisterViewModel CreateViewModel()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expensetracker.db3");
            var database = new DatabaseService(dbPath);
            var authService = new AuthService(database);

            // Initialize database asynchronously
            Task.Run(async () => await database.InitializeDatabaseAsync());

            return new RegisterViewModel(authService);
        }
    }
}