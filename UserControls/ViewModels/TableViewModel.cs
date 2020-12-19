using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.ViewModels.Base;
using Shared.Helpers;
using UserControls.Interfaces;
using Timer = System.Threading.Timer;

namespace UserControls.ViewModels
{
    public abstract class TableViewModelBase : DocumentViewModel
    {
        protected TableViewModelBase() { Init(); }
        protected void Init() { Initialize(); }
        protected abstract void Initialize();
    }

    public class TableViewModel : TableViewModelBase, ITableViewModel
    {
        #region Internal properties
        private string _filterText = string.Empty;
        Timer _timer = null;
        #endregion
        public virtual string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value.ToLower();
                RaisePropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        protected Tuple<DateTime?, DateTime?> StartEndDate { get; set; }
        protected List<object> Items;
        public virtual CollectionViewSource View { get; set; }
        protected override void Initialize()
        {
            StartEndDate = new Tuple<DateTime?, DateTime?>(null, null);
            Items = new List<object>();
            View = new CollectionViewSource { Source = Items };
            UpdateCommand = new RelayCommand(OnUpdate);
            ExportCommand = new RelayCommand(OnExport);
            PrintCommand = new RelayCommand(OnPrint);
        }

        private void TimerElapsed(object obj)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                View.View.Refresh();
                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("TotalRows");
                RaisePropertyChanged("TotalCount");
                RaisePropertyChanged("Total");
                RaisePropertyChanged("View");
            });
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
        protected bool Filter(object item)
        {
            if (string.IsNullOrEmpty(FilterText)) return true;
            var productOrder = item as ProductOrderModelBase;
            if (productOrder == null || productOrder.Product == null) return false;
            return productOrder.Product.Description.ToLower().Contains(FilterText.ToLower()) || productOrder.Product.Code.ToLower().Contains(FilterText);
        }

        public void Update()
        {
            OnUpdate();
        }
        protected virtual void OnUpdate()
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                var startEndDate = UIHelper.Managers.SelectManager.GetDateIntermediate(StartEndDate.Item1 ?? DateTime.Today, StartEndDate.Item2 ?? DateTime.Now);
                StartEndDate = new Tuple<DateTime?, DateTime?>(startEndDate.Item1, startEndDate.Item2);
            });
        }

        protected virtual void OnExport(object obj)
        {
            ExcelExportManager.ExportList(GetViewItems());
        }
        protected List<object> GetViewItems()
        {
            return View.View.OfType<object>().ToList();
        }
        protected virtual void OnPrint(object obj)
        {

        }
        public ICommand UpdateCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand PrintCommand { get; private set; }
    }

    public class TableViewModel<T> : TableViewModelBase, ITableViewModel
    {
        #region Constants

        #endregion

        #region Internal properties
        private string _filterText = string.Empty;
        Timer _timer = null;

        private bool _isShowNulls;
        private double _totalCount;
        private double _total;
        protected ObservableCollection<T> Reports { get; private set; }
        protected ObservableCollection<ProductOrderModelBase> Items;

        #endregion

        #region External Properties

        public int TotalRows
        {
            get { return ViewList.Count; }
        }
        public override double TotalCount { get { return _totalCount; } set { _totalCount = value; RaisePropertyChanged("TotalCount"); } }
        public override double Total { get { return _total; } set { _total = value; RaisePropertyChanged("Total"); } }
        public bool IsShowNulls
        {
            get
            {
                return _isShowNulls;
            }
            set
            {
                if (value == _isShowNulls) { return; }
                _isShowNulls = value;
                RaisePropertyChanged("ViewList");
            }
        }
        public bool IsShowUpdateButton { get; set; }
        public bool IsShowCloseButton { get; set; }
        protected virtual ObservableCollection<T> ViewFilteredList { get { return Reports; } }
        public ObservableCollection<T> ViewList
        {
            get { return ViewFilteredList; }
        }

        public CollectionViewSource View { get; set; }
        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value.ToLower();
                RaisePropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        #endregion

        #region Constructors
        public TableViewModel(List<T> list)
            : this()
        {
            foreach (var item in list)
            {
                ViewList.Add(item);
            }
            IsShowUpdateButton = true;
            IsShowCloseButton = true;
        }
        public TableViewModel()
        {
            Reports = new ObservableCollection<T>();
            ViewList.CollectionChanged += OnViewListCollectionChanged;
        }

        private void OnViewListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("ViewList");
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Total");
        }

        #endregion

        #region Internal methods


        //Base
        protected override void Initialize()
        {
            Items = new ObservableCollection<ProductOrderModelBase>();
            Items.CollectionChanged += OnItemsChanged;
            View = new CollectionViewSource { Source = Items };
            View.View.Filter = Filter;
            Title = "Ապրանքների դիտում ըստ պահեստների";
        }
        protected virtual bool CanExport(ExportImportEnum o) { return ViewList != null && ViewList.Any(); }
        protected virtual void OnExport(ExportImportEnum o)
        {
            //if (!CanExport(o)) { return; }
        }



        private void TimerElapsed(object obj)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                View.View.Refresh();
                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("TotalRows");
                RaisePropertyChanged("TotalCount");
                RaisePropertyChanged("Total");
                RaisePropertyChanged("View");
            });
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
        protected bool Filter(object item)
        {
            if (string.IsNullOrEmpty(FilterText)) return true;
            var productOrder = item as ProductOrderModelBase;
            if (productOrder == null || productOrder.Product == null) return false;
            return productOrder.Product.Description.ToLower().Contains(FilterText.ToLower()) || productOrder.Product.Code.ToLower().Contains(FilterText);
        }
        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Quantity");
            RaisePropertyChanged("CostPrice");
            RaisePropertyChanged("Price");
            RaisePropertyChanged("ItemsView");
            RaisePropertyChanged("ViewList");
            RaisePropertyChanged("TotalRows");
            RaisePropertyChanged("TotalCount");
            RaisePropertyChanged("Total");
            RaisePropertyChanged("View");
        }
        //protected void Update() { OnUpdate(); }
        public void Update()
        {
            if (IsLoading) return;
            new Thread(UpdateAsync).Start();
        }

        protected virtual void UpdateAsync()
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                IsLoading = true;
                ViewList.Clear();
            });
        }

        protected virtual void UpdateCompleted(bool isSuccess = true)
        {
            RaisePropertyChanged("ViewList");
            IsLoading = false;
        }
        protected void SetResult(List<T> reports)
        {
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                UpdateStopped();
                return;
            }
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                foreach (var report in reports)
                {
                    Reports.Add(report);
                }
                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("TotalRows");
                RaisePropertyChanged("TotalCount");
                RaisePropertyChanged("Total");
            });

        }

        protected void UpdateStopped()
        {
            UpdateCompleted(false);
        }
        protected virtual bool CanPrint(object o) { return o != null && ViewList != null && ViewList.Any(); }
        protected virtual void OnPrint(object o)
        {
            //if (!CanPrint(o)) { return; }
        }
        #endregion

        #region External methods

        #endregion

        #region Commands

        private ICommand _exportCommand;
        public ICommand ExportCommand { get { return _exportCommand ?? (_exportCommand = new RelayCommand<ExportImportEnum>(OnExport, CanExport)); } }
        private ICommand _printCommand;
        public ICommand PrintCommand { get { return _printCommand ?? (_printCommand = new RelayCommand(OnPrint, CanPrint)); } }
        private ICommand _updateCommand;
        public ICommand UpdateCommand { get { return _updateCommand ?? (_updateCommand = new RelayCommand(Update)); } }
        #endregion
    }

}
