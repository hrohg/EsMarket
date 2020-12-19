using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using UserControls.Controls;
using ExcelExportManager = ES.Business.ExcelManager.ExcelExportManager;
using Timer = System.Threading.Timer;

namespace UserControls.ViewModels
{
    public class SelectItemsViewModelBase<T> : ViewModelBase
    {
        #region Internal properties
        Timer _timer;
        private List<T> _items;
        #endregion Internal properties

        #region External properties

        public virtual List<T> Items
        {
            get { return _items ?? new List<T>(); }
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

        public SelectItemsViewModelBase(List<T> items, string title = "Ընտրել")
        {
            Initialize();
            _items = items ?? new List<T>();
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
        protected virtual bool CanExport(object dg)
        {
            return false;
        }
        protected virtual void OnExport(object dg)
        {
            ExcelExportManager.ExportList(Items);
        }

        #endregion Internal methods

        #region External methods

        public virtual List<T> GetItems()
        {
            return Items;
        }
        #endregion External methods

        #region Commands
        public ICommand ExportItemsCommand { get { return new RelayCommand(OnExport, CanExport); } }

        #endregion Commands
    }
    public class SelectItemsViewModel : SelectItemsViewModelBase<ItemsToSelectByCheck>
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties

        public override List<ItemsToSelectByCheck> Items
        {
            get { return base.Items.Where(s => string.IsNullOrEmpty(s.Description) || string.IsNullOrEmpty(Filter) || s.Description.ToLower().Contains(Filter.ToLower())).ToList(); }
            set
            {
                base.Items = value;
                RaisePropertyChanged("Items");
            }
        }

        #endregion External properties

        #region Constructors

        public SelectItemsViewModel(List<ItemsToSelectByCheck> items, string title = "Ընտրել")
            : base(items, title)
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {

        }
        #endregion Internal methods

        #region External methods

        public List<ItemsToSelectByCheck> GetCheckedItems()
        {
            return Items.Where(s => s.IsChecked).ToList();
        }
        #endregion External methods
    }

    public class SelectProductItemsViewModel : SelectItemsViewModelBase<ProductItemsToSelect>
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        public override List<ProductItemsToSelect> Items
        {
            get
            {
                return
                    base.Items.Where(s =>
                            string.IsNullOrEmpty(s.Description) || string.IsNullOrEmpty(Filter) ||
                            s.Description.ToLower().Contains(Filter.ToLower()) ||
                            s.Code.ToLower().Contains(Filter.ToLower())).ToList();
            }
            set
            {
                base.Items = value;
                RaisePropertyChanged("Items");
            }
        }
        #endregion External properties

        #region Constructors

        public SelectProductItemsViewModel(List<ProductItemsToSelect> items, string title = "Ընտրել")
            : base(items, title)
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {

        }
        #endregion Internal methods

        #region External methods

        public override List<ProductItemsToSelect> GetItems()
        {
            return Items.Where(s => s.IsChecked).ToList();
        }
        #endregion External methods
    }

    public class SelectProductItemsByCheckViewModel : SelectItemsViewModelBase<ProductItemsByCheck>
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        public override List<ProductItemsByCheck> Items
        {
            get
            {
                return
                    base.Items.Where(s =>
                            string.IsNullOrEmpty(s.Description) || string.IsNullOrEmpty(Filter) ||
                            s.Description.ToLower().Contains(Filter.ToLower()) ||
                            s.Code.ToLower().Contains(Filter.ToLower())).ToList();
            }
            set
            {
                base.Items = value;
                RaisePropertyChanged("Items");
            }
        }

        public bool? IsChecked
        {
            get { return Items.All(s => s.IsChecked) ? true : Items.All(s => !s.IsChecked) ? false : (bool?)null; }
            set
            {
                if (IsChecked == null) return;
                foreach (var item in Items)
                {
                    item.IsChecked = value != null && (bool)value;
                }
                RaisePropertyChanged("Items");
                RaisePropertyChanged("IsChecked");
            }
        }

        #endregion External properties

        #region Constructors

        public SelectProductItemsByCheckViewModel(List<ProductItemsByCheck> items, string title = "Ընտրել")
            : base(items, title)
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {

        }
        protected override bool CanExport(object dg)
        {
            return Items.Any();
        }
        #endregion Internal methods

        #region External methods

        public override List<ProductItemsByCheck> GetItems()
        {
            return Items.Where(s => s.IsChecked).ToList();
        }
        #endregion External methods

    }
}
