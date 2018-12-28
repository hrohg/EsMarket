using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.Helpers;

namespace UserControls.ViewModels
{
    public class InternalWayBillDetileViewModel : TableViewModel<InternalWayBillDetilesModel>
    {

        #region Internal properties

        private Tuple<DateTime, DateTime> _dateIntermediate;
        #endregion

        #region External properties
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
            base.UpdateAsync();
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                _dateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
            }));
            if (_dateIntermediate == null) { IsLoading = false; return; }

            SetResult(InvoicesManager.GetWillBillByDetile(_dateIntermediate.Item1, _dateIntermediate.Item2, ApplicationManager.Instance.GetMember.Id));

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            Description = string.Format("Տեղափոխություն {0} - {1}", _dateIntermediate.Item1.Date, _dateIntermediate.Item2.Date);
            TotalCount = (double)ViewList.Sum(s => s.Quantity ?? 0);
            Total = (double)ViewList.Sum(i => (i.Quantity ?? 0) * (i.Price ?? 0));
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
        private Tuple<DateTime, DateTime> _dateIntermediate;
        #endregion

        #region External properties
        public int Count { get { return ViewList.Count; } }
        public override double Total { get { return (double)ViewList.Sum(s => s.Amount); } }
        #endregion

        #region Constructors
        public ViewInternalWayBillViewModel(ViewInvoicesEnum o)
            : base()
        {

            IsShowUpdateButton = true;

        }
        #endregion

        #region Internal methods

        protected override void Initialize()
        {
            base.Initialize();
            Title = Description = "Տեղափոխություն";
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();

            GetDate();
            if (_dateIntermediate == null) { IsLoading = false; return; }


            var items = InvoicesManager.GetWillBill(_dateIntermediate.Item1, _dateIntermediate.Item2, ApplicationManager.Instance.GetMember.Id);

            SetResult(items);
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            Description = string.Format("Տեղափոխություն {0} - {1}", _dateIntermediate.Item1.Date, _dateIntermediate.Item2.Date);
        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null) { return; }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }
        protected void GetDate()
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                _dateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
            }));
        }
        #endregion
    }

}
