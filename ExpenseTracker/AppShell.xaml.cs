using ExpenseTracker.Views;

namespace ExpenseTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("calendar", typeof(CalendarPage));
            Routing.RegisterRoute("transactiondetail", typeof(TransactionDetailPage));
            Routing.RegisterRoute("changepassword", typeof(ChangePasswordPage));
            Routing.RegisterRoute("walletdetail", typeof(WalletDetailPage));
            Routing.RegisterRoute("addwallet", typeof(AddWalletPage));
            Routing.RegisterRoute(nameof(ReportPage), typeof(ReportPage));
            Routing.RegisterRoute("editprofile", typeof(EditProfilePage));
            Routing.RegisterRoute("changepassword", typeof(ChangePasswordPage));
            Routing.RegisterRoute("managecategories", typeof(ManageCategoriesPage));
        }
    }
}