using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ES.Common.ViewModels.Base;
using UserControls.Controls;

namespace UserControls.ViewModels
{
    public class SelectItemsViewModel : ViewModelBase
    {
        #region Internal properties
        Timer _timer = null;
        private List<ItemsToSelectByCheck> _items;
        #endregion Internal properties

        #region External properties

        public List<ItemsToSelectByCheck> Items
        {
            get { return _items != null ? _items.Where(s => string.IsNullOrEmpty(s.Description) || string.IsNullOrEmpty(Filter) || s.Description.ToLower().Contains(Filter.ToLower())).ToList() : new List<ItemsToSelectByCheck>(); }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        #region Title
        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                RaisePropertyChanged("Title");
            }
        }
        #endregion Title

        #region Filter
        private string _filter;

        public string Filter
        {
            get { return _filter; }
            set
            {
                if (value != null && value == Filter) return;
                _filter = value;
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
                RaisePropertyChanged("Filter");
            }
        }
        #endregion Filter

        #endregion External properties

        #region Constructors

        public SelectItemsViewModel(List<ItemsToSelectByCheck> items, string title = "Ընտրել")
        {
            Initialize();
            Items = items ?? new List<ItemsToSelectByCheck>();
            Title = title;
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {

        }
        private void TimerElapsed(object obj)
        {
            DisposeTimer();
            RaisePropertyChanged("Items");
        }
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
        #endregion Internal methods

        #region External methods

        public List<ItemsToSelectByCheck> GetCheckedItems()
        {
            return Items.Where(s => s.IsChecked).ToList();
        }
        #endregion External methods
    }
}
