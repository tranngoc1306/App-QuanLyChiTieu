using ExpenseTracker.ViewModels;
using ExpenseTracker.Services;

namespace ExpenseTracker.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = CreateViewModel();
        }

        // Constructor with dependency injection
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private LoginViewModel CreateViewModel()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expensetracker.db3");
            var database = new DatabaseService(dbPath);
            var authService = new AuthService(database);

            // Initialize database asynchronously
            Task.Run(async () => await database.InitializeDatabaseAsync());

            return new LoginViewModel(authService);
        }
    }
}