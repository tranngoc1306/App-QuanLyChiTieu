using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    [Table("users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Gender { get; set; } // "Nam" or "Nữ"

        public DateTime DateOfBirth { get; set; }

        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastLoginAt { get; set; }
    }

    public enum TransactionType
    {
        Expense,    // Chi phí
        Income,     // Thu nhập
        //Transfer    // Chuyển tiền
    }

    [Table("transactions")]
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string Note { get; set; }

        public int CategoryId { get; set; }

        public int? WalletId { get; set; }

        /*public bool ExcludeFromReport { get; set; }

        public string ImagePath { get; set; }*/

        public DateTime CreatedAt { get; set; }
    }

    [Table("categories")]
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public TransactionType Type { get; set; }

        public string Color { get; set; }
    }

    [Table("wallets")]
    public class Wallet
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public string Icon { get; set; }

        public string Color { get; set; }

        public decimal Budget { get; set; } // Ngân sách tháng

        public bool IsDefault { get; set; } // Ví mặc định
    }
}
