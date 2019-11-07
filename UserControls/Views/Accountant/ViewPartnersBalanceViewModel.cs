using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using Shared.Helpers;
using UserControls.Helpers;

namespace UserControls.Views.Accountant
{
    public class ViewPartnersBalanceViewModel : DocumentViewModel
    {
        #region Internal properties

        private List<PartnerBalanceModel> _items;

        #endregion Internal properties

        #region External properties

        #region Dates

        DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; RaisePropertyChanged("StartDate"); RaisePropertyChanged("EndDate"); }
        }

        DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; RaisePropertyChanged("EndDate"); RaisePropertyChanged("Title"); }
        }

        #endregion Dates

        #region Filters

        Timer _timer;
        private string _filterText;

        public string Filter
        {
            get { return _filterText; }
            set
            {
                _filterText = value.ToLower();
                RaisePropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }

        private List<string> Filters
        {
            get
            {
                return string.IsNullOrEmpty(_filterText)
                    ? new List<string>()
                    : _filterText.Split(',').Select(s => s.Trim()).ToList();
            }
        }

        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private void TimerElapsed(object obj)
        {
            RaisePropertyChanged("Items");
            DisposeTimer();
        }

        #endregion Filters

        public List<PartnerBalanceModel> Items
        {
            get
            {
                return _items.Where(s =>
                        !Filters.Any() || Filters.Contains(s.Description) || Filters.Contains(s.BalanceTypeDescription))
                    .ToList();
            }
        }

        public override string Title
        {
            get { return string.Format("Դեբիտորական պարտք {0} - {1}", StartDate, EndDate); }

        }
        public override string Description
        {
            get
            {
                return string.Format("Պատվիրատուների կրեդիտորական պարտքեր {0} - {1} ժամանակահատվածի համար", StartDate, EndDate);
            }

        }
        #endregion External properties

        #region Consructors

        public ViewPartnersBalanceViewModel()
        {
            Initialize();
        }

        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            _items = new List<PartnerBalanceModel>();
        }

        private void OnUpdateAsync()
        {
            List<Guid> guidIds = new List<Guid>();

            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                guidIds = SelectItemsManager.SelectPartners(true).Select(s => s.Id).ToList();

                var dates = UIHelper.Managers.SelectManager.GetDateIntermediate();
                if (dates == null) return;
                StartDate = dates.Item1;
                EndDate = dates.Item2;
            });
            var partnersDebit = PartnersManager.GetPartnersDebit(guidIds, StartDate, EndDate);
            var partners = CashManager.Instance.GetPartners.Where(s => guidIds.Contains(s.Id)).ToList();

            _items.Clear();
            _items.AddRange(partnersDebit.Select(s => new PartnerBalanceModel()
            {
                Date = s.Date,
                Description = string.Format("{0} ({1})", partners.Single(p => p.Id == s.PartnerId).FullName, s.Notes),
                Type = BalanceTypeEnum.Credit,
                Amount = (double)s.Amount,
                Paid = (double)(s.PaidAmount ?? 0),
                ExpairDate = s.ExpairyDate
            }).ToList());
            OnUpdate();
        }

        private void OnUpdate()
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                RaisePropertyChanged("Items");
                RaisePropertyChanged("Title");
                RaisePropertyChanged("Description");
            });
        }

        #endregion Internal methods

        #region External methods

        public void Update()
        {
            new Thread(OnUpdateAsync).Start();
        }

        #endregion External methods

        #region Commands

        private ICommand _exportCommand;

        public ICommand ExportCommand
        {
            get { return _exportCommand ?? (_exportCommand = new RelayCommand<ExportImportEnum>(OnExportToExcel)); }
        }

        private void OnExportToExcel(ExportImportEnum obj)
        {
            ExcelExportManager.ExportList(Items);
        }

        private ICommand _updateCommand;
        private DateTime _startDate;
        private DateTime _endDate;

        public ICommand UpdateCommand
        {
            get { return _updateCommand ?? (_updateCommand = new RelayCommand(o => Update())); }
        }

        #endregion Commands
    }

    public class PartnerBalanceModel
    {
        public DateTime Date { get; set; }
        public DateTime? ExpairDate { get; set; }
        public string Description { get; set; }
        public BalanceTypeEnum Type { get; set; }

        public string BalanceTypeDescription
        {
            get
            {
                switch (Type)
                {
                    case BalanceTypeEnum.Debit:
                        return "Դեբետ";
                        break;
                    case BalanceTypeEnum.Credit:
                        return "Կրեդիտ";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public double Amount { get; set; }
        public double Paid { get; set; }

        public double Balance
        {
            get { return Amount - Paid; }
        }
    }

    public enum BalanceTypeEnum
    {
        Debit,
        Credit
    }
}
