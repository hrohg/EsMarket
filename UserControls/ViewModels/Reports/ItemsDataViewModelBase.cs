using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using Shared.Helpers;
using ProductModel = ES.Data.Models.Products.ProductModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Reports
{
    public abstract class ItemsDataViewModelBase<T> : DocumentViewModel
    {
        public ObservableCollection<T> Items
        {
            get { return _items; }
            set { _items = value; RaisePropertyChanged("Items"); }
        }

        protected ItemsDataViewModelBase()
        {
            Init();
        }
        private void Init() { Initialize(); }
        protected virtual void Initialize()
        {
            Items = new ObservableCollection<T>();
            OnUpdate(null);
        }

        protected virtual void Update()
        {
            new Thread(UpdateAsync).Start();
        }
        protected abstract void UpdateAsync();

        protected void OnUpdate(object o)
        {
            if (IsLoading) return;
            lock (Sync)
            {
                if (IsLoading) return;
                IsLoading = true;
                Items.Clear();
            }
            Update();
        }

        protected virtual void UpdateCompleted(bool isSuccess)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { IsLoading = false; });
        }

        private ICommand _updateCommand;
        public ICommand UpdateCommand { get { return _updateCommand ?? (_updateCommand = new RelayCommand(OnUpdate, CanUpdate)); } }

        private bool CanUpdate(object o)
        {
            return !IsLoading;
        }

        private ICommand _exportCommand;
        private ObservableCollection<T> _items;
        public ICommand ExportCommand { get { return _exportCommand ?? (_exportCommand = new RelayCommand<ExportImportEnum>(OnExport, CanExport)); } }



        protected virtual bool CanExport(ExportImportEnum obj)
        {
            return Items.Any();
        }
        protected virtual void OnExport(ExportImportEnum obj)
        {
            ExcelExportManager.ExportList(Items.Cast<object>().ToList());
        }

        protected void AddNewItem(T item)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                Items.Add(item);
            });
        }
    }

    public class FallowProductsViewModel : ItemsDataViewModelBase<ProductModel>
    {
        private int _daysCount = 180;

        protected override void Initialize()
        {
            base.Initialize();
            Title = "Կորդ մնացորդի վերլուծություն";
        }
        protected override void Update()
        {
            var win = new SelectCount(new SelectCountModel(_daysCount, "Մուտքագրել օրերի քանակը"), Visibility.Collapsed);
            win.ShowDialog();
            if (!win.DialogResult.HasValue || !win.DialogResult.Value) { UpdateCompleted(false); return; }
            _daysCount = (int)win.SelectedCount;
            base.Update();
        }
        protected override void UpdateAsync()
        {
            var items = ProductsManager.GetFallowProductItems(_daysCount);
            if (items != null)
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                {
                    foreach (var item in items)
                    {
                        var nextItem = item;
                        Items.Add(nextItem);
                    }
                });
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(true); });
        }

        protected override void OnExport(ExportImportEnum obj)
        {
            ExcelExportManager.ExportList(Items.Select(s => new
            {
                Կոդ = s.Code,
                Անվանում = s.Description,
                Չմ = s.Mu,
                Գին = s.Price,
                Առկա = s.ExistingQuantity,
                Մատակարար = s.Provider != null ? s.Provider.FullName : string.Empty
            }));
        }
    }

    public class CheckProductsRemainderViewModel : ItemsDataViewModelBase<ProductModel>
    {
        private StockModel _stock;
        private int _daysCount = 120;

        protected override void Initialize()
        {
            base.Initialize();
            Title = "Բաժինների անբավարար մնացորդի ստուգում";
        }

        protected override void Update()
        {
            var selectedStock = SelectItemsManager.SelectStocks();
            if (selectedStock == null || !selectedStock.Any()) return;
            _stock = selectedStock.FirstOrDefault();
            var days = SelectItemsManager.GetDays(_daysCount);
            if (days == null) return;
            _daysCount = days.Value;
            base.Update();
        }

        protected override void UpdateAsync()
        {
            if (_stock == null) { UpdateCompleted(false); return; }
            var items = ProductsManager.CheckProductRemainderItems(_stock.Id, _daysCount);
            if (items != null)
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                {
                    foreach (var item in items)
                    {
                        var nextItem = item;
                        Items.Add(nextItem);
                    }
                });
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(true); });
        }


    }

    public class CheckProductsRemainderByStockViewModel : TableViewModel<ProductModel>
    {
        private StockModel _stock;

        protected override void Initialize()
        {
            Title = Description = "Բաժինների անբավարար մնացորդի ստուգում";
            base.Initialize();
        }


        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            SetStock();
            if (_stock == null)
            {
                UpdateCompleted(false);
                return;
            }

            var items = ProductsManager.CheckProductRemainderByStockItems(_stock.Id);
            SetResult(items);
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        private void SetStock()
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                var selectedStock = SelectItemsManager.SelectStocks();
                if (selectedStock == null || !selectedStock.Any()) return;
                _stock = selectedStock.FirstOrDefault();
            });

        }
    }

}
