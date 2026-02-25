using ExpenseTracker.Views;

/*namespace ExpenseTracker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Test với page đơn giản nhất
            MainPage = new TestPage();
        }
    }
}*/

namespace ExpenseTracker
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            // Check if user is logged in
            var userId = Preferences.Get("UserId", 0);

            if (userId > 0)
            {
                // User is logged in, go to main page
                //MainPage = new NavigationPage(new TransactionPage())
                MainPage = new AppShell()
                {
                    //BarBackgroundColor = Color.FromArgb("#4ECDC4"),
                    //BarTextColor = Colors.White
                };
                
            }
            else
            {
                // User is not logged in, go to login page
                MainPage = new NavigationPage(new LoginPage())
                {
                    BarBackgroundColor = Color.FromArgb("#4ECDC4"),
                    BarTextColor = Colors.White
                };
            }
        }
    }
}