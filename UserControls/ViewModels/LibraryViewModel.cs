using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using ES.Business.Managers;

namespace UserControls.ViewModels
{
    public class LibraryViewModel:INotifyPropertyChanged
    {
        #region Internal properties
        private List<ItemsToSelect> _items;
        private string _textFilter;
        Timer _timer = null;
        #endregion
        #region External properties

        public string TextFilter
        {
            get
            {
                return _textFilter;
            }
            set
            {
                _textFilter = value.ToLower();
                OnPropertyChanged("TextFilter");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }

        public ObservableCollection<ItemsToSelect> Items { get {
            return _items!=null? new ObservableCollection<ItemsToSelect>(string.IsNullOrEmpty(TextFilter)? _items: _items.Where(s=>s.DisplayName.ToLower().Contains(TextFilter))):new ObservableCollection<ItemsToSelect>()  ; } }
        #endregion

        #region Constructors
        public LibraryViewModel()
        {

        }
        #endregion

        #region Internal methods
        private void TimerElapsed(object obj)
        {
            OnPropertyChanged("Items");
            DisposeTimer();
        }
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
        #endregion

        #region External methods
        public void SetItems(List<ItemsToSelect> items)
        {
            _items = items;
            OnPropertyChanged("Items");
        }
        public void SelectProductItems(List<long> stockIds)
        {
            var productItems = ProductsManager.GetProductItemsFromStocks(stockIds);
            if (productItems == null || productItems.Count == 0) { return; }
            productItems = productItems.Where(s => s.Product != null).OrderBy(s => s.Product.Code).ThenBy(s => s.Product.Description).ToList();
            var items = productItems.GroupBy(s => s.ProductId).Select(s =>
                    new ItemsToSelect(string.Format("{0} {1} {2} - {3}", s.First().Product.Code, s.First().Product.Description, s.First().Product.Price, s.Sum(t => t.Quantity)), s.First().Product)).ToList();
            SetItems(items);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
