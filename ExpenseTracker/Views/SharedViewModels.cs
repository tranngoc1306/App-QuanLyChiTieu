using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExpenseTracker.Models;
using Microsoft.Maui.Graphics;

namespace ExpenseTracker.Views
{
    // CategoryViewModel with IsSelected property for visual feedback
    public class CategoryViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private readonly Category _category;

        public CategoryViewModel(Category category)
        {
            _category = category;
        }

        public int Id => _category.Id;
        public string Name => _category.Name;
        public string Icon => _category.Icon;
        public TransactionType Type => _category.Type;
        public string Color => _category.Color;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for displaying transactions
    public class TransactionDisplayItem
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string AmountDisplay { get; set; }
        public Color AmountColor { get; set; }
        public bool HasNote { get; set; }
    }
}