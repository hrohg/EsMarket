﻿using System;
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
using UserControls.ControlPanel.Controls;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Reports
{
    public abstract class ItemsDataViewModelBase<T> : DocumentViewModel
    {
        public ObservableCollection<T> Items { get; set; }

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
        public ICommand ExportCommand { get { return _exportCommand ?? (_exportCommand = new RelayCommand<ExportImportEnum>(OnExport, CanExport)); } }



        protected virtual bool CanExport(ExportImportEnum obj)
        {
            return Items.Any();
        }
        protected virtual void OnExport(ExportImportEnum obj)
        {
            ExcelExportManager.ExportList(Items.Cast<object>().ToList());
        }
    }

    public class FallowProductsViewModel : ItemsDataViewModelBase<ProductModel>
    {
        private int _daysCount = 180;
        public FallowProductsViewModel()
        {
            Initialize();
        }

        protected override sealed void Initialize()
        {
            base.Initialize();
            Title = "Կորդ մնացորդի վերլուծություն";
        }
        protected override void Update()
        {
            var win = new SelectCount(new UserControls.SelectCountModel(_daysCount, "Մուտքագրել օրերի քանակը"), Visibility.Collapsed);
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
            UpdateCompleted(true);
        }

        protected override void OnExport(ExportImportEnum obj)
        {
            ExcelExportManager.ExportList(Items.Select(s => new { Կոդ = s.Code, Անվանում = s.Description, Չմ = s.Mu, Առկա = s.ExistingQuantity }));
        }
    }

    public class CheckProductsRemainderViewModel : ItemsDataViewModelBase<ProductModel>
    {
        private StockModel _stock;
        private int _daysCount=120;
        public CheckProductsRemainderViewModel()
        {
            Initialize();
        }

        protected override sealed void Initialize()
        {
            base.Initialize();
            Title = "Բաժինների անբավարար մնացորդի ստուգում";
        }

        protected override void Update()
        {
            var selectedStock = SelectItemsManager.SelectStocks(false);
            if (selectedStock == null || !selectedStock.Any()) return;
            _stock = selectedStock.FirstOrDefault();
            var days = SelectItemsManager.GetDays(_daysCount);
            if(days==null) return;
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
            UpdateCompleted(true);
        }


    }

    public class CheckProductsRemainderByStockViewModel : TableViewModel<ProductModel>
    {
        private StockModel _stock;
        
        public CheckProductsRemainderByStockViewModel()
        {
            Initialize();
        }

        protected void Initialize()
        {
           Title = Description = "Բաժինների անբավարար մնացորդի ստուգում";
            OnUpdate();
        }

        protected override void OnUpdate()
        {
            var selectedStock = SelectItemsManager.SelectStocks(false);
            if (selectedStock == null || !selectedStock.Any()) return;
            _stock = selectedStock.FirstOrDefault();
            
            base.OnUpdate();
            var thread = new Thread(UpdateAsync);
            thread.Start();
        }

        protected void UpdateAsync()
        {
            if (_stock == null)
            {
                //UpdateCompleted(false); 
                return;
            }
            IsLoading = true;
            var items = ProductsManager.CheckProductRemainderByStockItems(_stock.Id);
            if (items != null)
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                {
                    foreach (var item in items)
                    {
                        var nextItem = item;
                        ViewList.Add(nextItem);
                    }
                });
            //UpdateCompleted(true);
            IsLoading = false;
        }


    }

}
