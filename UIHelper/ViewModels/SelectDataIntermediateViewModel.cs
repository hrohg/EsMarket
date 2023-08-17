using System;
using UIHelper.Interfaces;

namespace UIHelper.ViewModels
{
    public class SelectDataIntermediateViewModel : ViewModelBase, ISelectable
    {
        #region Internal fields
        private DateTime _endDate;
        #endregion Internal fields

        #region Internal properties
        #endregion Internal properties

        #region External properties
        public string StartDateTooltip { get; set; }
        public string EndDateTooltip { get; set; }
        public bool IsEndDateEnabled { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value.AddHours(23).AddMinutes(59).AddSeconds(59); RaisePropertyChanged("EndDate"); }
        }

        #endregion External properties

        #region Constructors

        public SelectDataIntermediateViewModel()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            StartDateTooltip = "Սկիզբ";
            EndDateTooltip = "Ավարտ";
            IsEndDateEnabled = true;
        }

        public SelectDataIntermediateViewModel(DateTime? startDate)
            : this()
        {
            StartDate = startDate ?? DateTime.Now;
            IsEndDateEnabled = false;
        }

        public SelectDataIntermediateViewModel(DateTime? startDate, DateTime? endDate)
            : this(startDate)
        {
            EndDate = endDate ?? DateTime.Now;
            IsEndDateEnabled = true;
        }
        #endregion Constructors
    }
}
