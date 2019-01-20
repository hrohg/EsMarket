using System;
using UIHelper.ViewModels;
using UIHelper.Windows;

namespace UIHelper.Managers
{
    public class SelectManager
    {
        #region Date and time
        public static Tuple<DateTime, DateTime> GetDateIntermediate()
        {
            var vm = new SelectDataIntermediateViewModel();
            var window = new SelectWindow(vm);
            window.ShowDialog();
            if (window.DialogResult == null || !(bool)window.DialogResult)
            {
                return null;
            }
            return new Tuple<DateTime, DateTime>(vm.StartDate, vm.EndDate);
        }

        public static DateTime? GetDate(DateTime? date=null)
        {
            var vm = new SelectDataIntermediateViewModel(date);
            var window = new SelectWindow(vm);
            window.ShowDialog();
            if (window.DialogResult == null || !(bool)window.DialogResult)
            {
                return null;
            }
            return vm.StartDate;
        }

        public static Tuple<DateTime, DateTime> GetDateIntermediate(DateTime startDate, DateTime endDate)
        {
            var vm = new SelectDataIntermediateViewModel(startDate, endDate);
            var window = new SelectWindow(vm);
            window.ShowDialog();
            if (window.DialogResult == null || !(bool)window.DialogResult)
            {
                return null;
            }
            return new Tuple<DateTime, DateTime>(vm.StartDate, vm.EndDate);
        }
        #endregion Date and time


    }
}
