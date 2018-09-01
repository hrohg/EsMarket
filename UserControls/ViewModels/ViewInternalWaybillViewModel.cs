using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;

namespace UserControls.ViewModels
{
    public class InternalWayBillDetileViewModel : TableViewModel<InternalWayBillDetilesModel>
    {

        #region Internal properties
        private List<InternalWayBillDetilesModel> _items = new List<InternalWayBillDetilesModel>();
        private Tuple<DateTime, DateTime> _dateIntermediate;
        #endregion

        #region External properties
        public override ObservableCollection<InternalWayBillDetilesModel> ViewList
        {
            get
            {
                return new ObservableCollection<InternalWayBillDetilesModel>(_items);
            }
            protected set
            {
                _items = value.ToList();
                RaisePropertyChanged("ViewList");
            }
        }
        #endregion

        #region Constructors
        public InternalWayBillDetileViewModel(object o)
            : base()
        {
        }
        #endregion

        #region Internal methods
        protected override void Initialize()
        {
            IsShowUpdateButton = true;
            base.Initialize();
            Title = Description = "Տեղափոխություն";
        }
        protected override void UpdateAsync()
        {
            if(_dateIntermediate==null) {IsLoading=false; return;}
            RaisePropertyChanged(IsInProgressProperty);
            ViewList = new ObservableCollection<InternalWayBillDetilesModel>(InvoicesManager.GetWillBillByDetile(_dateIntermediate.Item1, _dateIntermediate.Item2, ApplicationManager.Instance.GetMember.Id));
            TotalRows = _items.Count;
            TotalCount = (double)_items.Sum(s => s.Quantity ?? 0);
            Total = (double)_items.Sum(i => (i.Quantity ?? 0) * (i.Price ?? 0));
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
        }
        protected override void OnUpdate()
        {
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("Տեղափոխություն {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            base.OnUpdate();
        }
        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null) { return; }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }
        #endregion
    }

    public class ViewInternalWayBillViewModel : TableViewModel<InternalWayBillModel>
    {

        #region Internal properties
        private List<InternalWayBillModel> _items = new List<InternalWayBillModel>();
        #endregion

        #region External properties
        public override ObservableCollection<InternalWayBillModel> ViewList
        {
            get
            {
                return new ObservableCollection<InternalWayBillModel>(_items);
            }
            protected set
            {
                _items = value.ToList();
                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("Count");
                RaisePropertyChanged("Total");
            }
        }
        public int Count { get { return _items.Count; } }
        public override double Total { get { return (double)_items.Sum(s => s.Amount); } }
        #endregion

        #region Constructors
        public ViewInternalWayBillViewModel(ViewInvoicesEnum o)
            : base()
        {
            Title = Description = "Տեղափոխություն";
            IsShowUpdateButton = true;
            Initialize(o);
        }
        #endregion

        #region Internal methods
        private void Initialize(object o)
        {
            OnUpdate();
        }
        private void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            IsLoading = true;
            RaisePropertyChanged(IsInProgressProperty);
            ViewList = new ObservableCollection<InternalWayBillModel>(InvoicesManager.GetWillBill(dateIntermediate.Item1, dateIntermediate.Item2, ApplicationManager.Instance.GetMember.Id));
            TotalRows = _items.Count;
            //TotalCount = (double)_items.Sum(s => s.Quantity ?? 0);
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;

            Description = string.Format("Տեղափոխություն {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            var thread = new Thread(() => Update(dateIntermediate));
            thread.Start();
        }
        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null) { return; }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }
        #endregion
    }

}
