using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.ViewModels.Base;
using Shared.Helpers;
using UserControls.Interfaces;

namespace UserControls.ViewModels
{
    public abstract class TableViewModelBase : DocumentViewModel
    {
        protected TableViewModelBase() { Init(); }
        protected void Init() { Initialize(); }
        protected abstract void Initialize();
    }

    public class TableViewModel<T> : TableViewModelBase, ITableViewModel
    {
        #region Constants

        #endregion

        #region Internal properties

        private bool _isShowNulls;
        private double _totalCount;
        private double _total;
        protected ObservableCollection<T> Reports { get; private set; }

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

        }
        protected virtual bool CanExport(ExportImportEnum o) { return ViewList != null && ViewList.Any(); }
        protected virtual void OnExport(ExportImportEnum o)
        {
            //if (!CanExport(o)) { return; }
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
