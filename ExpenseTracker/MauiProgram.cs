using Microsoft.Extensions.Logging;
using Microcharts.Maui;
using ExpenseTracker.Services;
using ExpenseTracker.Views;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            SQLitePCL.Batteries_V2.Init();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Database Service
            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "expensetracker.db3");

            builder.Services.AddSingleton<DatabaseService>(s =>
                new DatabaseService(dbPath));

            // Register Auth Service
            builder.Services.AddSingleton<AuthService>();

            // Register Pages and ViewModels
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<TransactionPage>();
            builder.Services.AddTransient<TransactionsViewModel>();
            builder.Services.AddTransient<TransactionDetailPage>();
            builder.Services.AddTransient<TransactionDetailViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
