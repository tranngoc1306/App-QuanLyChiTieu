using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.Maui.Controls;

namespace ExpenseTracker.ViewModels
{
    public class TransactionDetailViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _database;
        private Transaction _transaction;
        private Category _category;

        private TransactionType _type;
        private DateTime _date;
        private decimal _amount;
        private string _note;
        private bool _excludeFromReport;

        public TransactionDetailViewModel(DatabaseService database, int transactionId)
        {
            _database = database;

            UpdateCommand = new Command(async () => await UpdateTransactionAsync());
            DeleteCommand = new Command(async () => await DeleteTransactionAsync());

            Task.Run(async () => await LoadTransactionAsync(transactionId));
        }

        public TransactionType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged();
            }
        }

        public bool ExcludeFromReport
        {
            get => _excludeFromReport;
            set
            {
                _excludeFromReport = value;
                OnPropertyChanged();
            }
        }

        public string CategoryName => _category?.Name ?? "";
        public string CategoryIcon => _category?.Icon ?? "❓";

        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        private async Task LoadTransactionAsync(int transactionId)
        {
            _transaction = await _database.GetTransactionAsync(transactionId);
            if (_transaction != null)
            {
                Type = _transaction.Type;
                Date = _transaction.Date;
                Amount = _transaction.Amount;
                Note = _transaction.Note;
                //ExcludeFromReport = _transaction.ExcludeFromReport;

                _category = await _database.GetCategoryAsync(_transaction.CategoryId);
                OnPropertyChanged(nameof(CategoryName));
                OnPropertyChanged(nameof(CategoryIcon));
            }
        }

        private async Task UpdateTransactionAsync()
        {
            if (_transaction == null || Amount <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin", "OK");
                return;
            }

            _transaction.Amount = Amount;
            _transaction.Date = Date;
            _transaction.Note = Note;
            //_transaction.ExcludeFromReport = ExcludeFromReport;

            await _database.SaveTransactionAsync(_transaction);

            await Application.Current.MainPage.DisplayAlert("Thành công", "Đã cập nhật giao dịch", "OK");
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task DeleteTransactionAsync()
        {
            if (_transaction == null)
                return;

            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Xác nhận",
                "Bạn có chắc muốn xóa giao dịch này?",
                "Xóa",
                "Hủy");

            if (confirm)
            {
                await _database.DeleteTransactionAsync(_transaction);
                await Application.Current.MainPage.DisplayAlert("Thành công", "Đã xóa giao dịch", "OK");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}