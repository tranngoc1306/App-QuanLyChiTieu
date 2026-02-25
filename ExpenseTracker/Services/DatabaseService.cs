using SQLite;
using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private bool _isInitialized = false;

        public DatabaseService(string dbPath)
        {
            try
            {
                // ✅ TẠO THÊM MỤC TRƯỚC KHI TẠO DATABASE
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    System.Diagnostics.Debug.WriteLine($"🟡 Creating directory: {directory}");
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"✅ Directory created successfully");
                }

                System.Diagnostics.Debug.WriteLine($"🟢 Database path: {dbPath}");
                _database = new SQLiteAsyncConnection(dbPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔴 DatabaseService constructor error: {ex.Message}");
                throw;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("🟢 Starting database initialization");

                await _database.CreateTableAsync<User>();
                System.Diagnostics.Debug.WriteLine("✅ Users table created");

                await _database.CreateTableAsync<Transaction>();
                System.Diagnostics.Debug.WriteLine("✅ Transactions table created");

                await _database.CreateTableAsync<Category>();
                System.Diagnostics.Debug.WriteLine("✅ Categories table created");

                await _database.CreateTableAsync<Wallet>();
                System.Diagnostics.Debug.WriteLine("✅ Wallets table created");

                await InitializeDefaultDataAsync();

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("✅ Database initialization completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔴 Database initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"🔴 Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task InitializeDefaultDataAsync()
        {
            try
            {
                var categoryCount = await _database.Table<Category>().CountAsync();
                if (categoryCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine("🟡 Initializing default categories");
                    await InitializeDefaultCategoriesAsync();
                }

                var walletCount = await _database.Table<Wallet>().CountAsync();
                if (walletCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine("🟡 Initializing default wallet");
                    await InitializeDefaultWalletAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔴 InitializeDefaultData error: {ex.Message}");
                throw;
            }
        }

        private async Task InitializeDefaultCategoriesAsync()
        {
            var expenseCategories = new List<Category>
            {
                new Category { Name = "Ăn uống", Icon = "🍳", Type = TransactionType.Expense, Color = "#FF6B6B" },
                new Category { Name = "Gia đình", Icon = "👨‍👩‍👧", Type = TransactionType.Expense, Color = "#4ECDC4" },
                new Category { Name = "Di chuyển", Icon = "🚗", Type = TransactionType.Expense, Color = "#FFE66D" },
                new Category { Name = "Quần áo", Icon = "👕", Type = TransactionType.Expense, Color = "#FF6B9D" },
                new Category { Name = "Mua sắm", Icon = "🛍️", Type = TransactionType.Expense, Color = "#C44569" },
                new Category { Name = "Giải trí", Icon = "🎮", Type = TransactionType.Expense, Color = "#FFA502" },
                new Category { Name = "Gửi xe", Icon = "🅿️", Type = TransactionType.Expense, Color = "#A29BFE" },
                new Category { Name = "Xăng dầu", Icon = "⛽", Type = TransactionType.Expense, Color = "#FDA7DF" },
                new Category { Name = "Điện thoại", Icon = "📞", Type = TransactionType.Expense, Color = "#F8B500" },
                new Category { Name = "Sức khỏe", Icon = "❤️", Type = TransactionType.Expense, Color = "#FF6348" },
                new Category { Name = "Giáo dục", Icon = "📚", Type = TransactionType.Expense, Color = "#1E90FF" },
                new Category { Name = "Bạn bè", Icon = "💑", Type = TransactionType.Expense, Color = "#FF69B4" },
                new Category { Name = "Cho vay", Icon = "💰", Type = TransactionType.Expense, Color = "#26DE81" },
                new Category { Name = "Khác", Icon = "❓", Type = TransactionType.Expense, Color = "#95A5A6" }
            };

            var incomeCategories = new List<Category>
            {
                new Category { Name = "Lương", Icon = "💰", Type = TransactionType.Income, Color = "#26DE81" },
                new Category { Name = "Thưởng", Icon = "💵", Type = TransactionType.Income, Color = "#FED330" },
                new Category { Name = "Quà tặng", Icon = "🎁", Type = TransactionType.Income, Color = "#FC5C65" },
                new Category { Name = "Đầu tư", Icon = "📈", Type = TransactionType.Income, Color = "#45AAF2" },
                new Category { Name = "Lãi", Icon = "🏛️", Type = TransactionType.Income, Color = "#F7B731" },
                new Category { Name = "Khác", Icon = "❓", Type = TransactionType.Income, Color = "#95A5A6" },
                new Category { Name = "Đi vay", Icon = "🤝", Type = TransactionType.Income, Color = "#A55EEA" }
            };

            await _database.InsertAllAsync(expenseCategories);
            await _database.InsertAllAsync(incomeCategories);
            System.Diagnostics.Debug.WriteLine("✅ Default categories inserted");
        }

        private async Task InitializeDefaultWalletAsync()
        {
            var defaultWallet = new Wallet
            {
                Name = "Chi tiêu",
                Balance = 0,
                Icon = "💳",
                Color = "#4ECDC4"
            };

            await _database.InsertAsync(defaultWallet);
            System.Diagnostics.Debug.WriteLine("✅ Default wallet inserted");
        }

        // Transaction Operations
        public Task<List<Transaction>> GetTransactionsAsync()
        {
            return _database.Table<Transaction>()
                           .OrderByDescending(t => t.Date)
                           .ThenByDescending(t => t.CreatedAt)
                           .ToListAsync();
        }

        public Task<Transaction> GetTransactionAsync(int id)
        {
            return _database.Table<Transaction>()
                           .Where(t => t.Id == id)
                           .FirstOrDefaultAsync();
        }

        public Task<int> SaveTransactionAsync(Transaction transaction)
        {
            if (transaction.Id != 0)
            {
                return _database.UpdateAsync(transaction);
            }
            else
            {
                transaction.CreatedAt = DateTime.Now;
                return _database.InsertAsync(transaction);
            }
        }

        public Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            return _database.DeleteAsync(transaction);
        }

        // Category Operations
        public Task<List<Category>> GetCategoriesAsync(TransactionType type)
        {
            return _database.Table<Category>()
                           .Where(c => c.Type == type)
                           .ToListAsync();
        }

        public Task<Category> GetCategoryAsync(int id)
        {
            return _database.Table<Category>()
                           .Where(c => c.Id == id)
                           .FirstOrDefaultAsync();
        }

        // Wallet Operations
        public Task<List<Wallet>> GetWalletsAsync()
        {
            return _database.Table<Wallet>()
                           .OrderByDescending(w => w.IsDefault)
                           .ThenBy(w => w.Name)
                           .ToListAsync();
        }

        public Task<Wallet> GetWalletAsync(int id)
        {
            return _database.Table<Wallet>()
                           .Where(w => w.Id == id)
                           .FirstOrDefaultAsync();
        }

        public async Task<int> SaveWalletAsync(Wallet wallet)
        {
            // If setting as default, unset other defaults
            if (wallet.IsDefault)
            {
                var wallets = await GetWalletsAsync();
                foreach (var w in wallets.Where(w => w.Id != wallet.Id && w.IsDefault))
                {
                    w.IsDefault = false;
                    await _database.UpdateAsync(w);
                }
            }

            if (wallet.Id != 0)
            {
                return await _database.UpdateAsync(wallet);
            }
            else
            {
                return await _database.InsertAsync(wallet);
            }
        }

        public Task<int> DeleteWalletAsync(Wallet wallet)
        {
            return _database.DeleteAsync(wallet);
        }

        public async Task UpdateWalletBalanceAsync(int walletId, decimal amount)
        {
            var wallet = await GetWalletAsync(walletId);
            if (wallet != null)
            {
                wallet.Balance += amount;
                await _database.UpdateAsync(wallet);
            }
        }

        // Get default wallet
        public async Task<Wallet> GetDefaultWalletAsync()
        {
            var defaultWallet = await _database.Table<Wallet>()
                                              .Where(w => w.IsDefault)
                                              .FirstOrDefaultAsync();

            if (defaultWallet == null)
            {
                var wallets = await GetWalletsAsync();
                defaultWallet = wallets.FirstOrDefault();
            }

            return defaultWallet;
        }

        // User Operations
        public Task<User> GetUserByEmailAsync(string email)
        {
            return _database.Table<User>()
                           .Where(u => u.Email == email)
                           .FirstOrDefaultAsync();
        }

        public Task<User> GetUserAsync(int id)
        {
            return _database.Table<User>()
                           .Where(u => u.Id == id)
                           .FirstOrDefaultAsync();
        }

        public Task<int> SaveUserAsync(User user)
        {
            if (user.Id != 0)
            {
                return _database.UpdateAsync(user);
            }
            else
            {
                return _database.InsertAsync(user);
            }
        }

        public Task<List<Transaction>> GetTransactionsByMonthAsync(int year, int month, int? walletId = null)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _database.Table<Transaction>()
                                 .Where(t => t.Date >= startDate && t.Date <= endDate);

            if (walletId.HasValue)
            {
                query = query.Where(t => t.WalletId == walletId.Value);
            }

            return query.OrderByDescending(t => t.Date).ToListAsync();
        }

        // Generic delete method
        public Task<int> DeleteAsync<T>(T entity) where T : class
        {
            return _database.DeleteAsync(entity);
        }

        // Generic insert method
        public Task<int> InsertAsync<T>(T entity) where T : class
        {
            return _database.InsertAsync(entity);
        }

        // Generic update method  
        public Task<int> UpdateAsync<T>(T entity) where T : class
        {
            return _database.UpdateAsync(entity);
        }
    }
}