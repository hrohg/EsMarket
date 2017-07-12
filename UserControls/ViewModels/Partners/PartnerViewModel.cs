using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using ES.DataAccess.Models;
using UserControls.Commands;

namespace UserControls.ViewModels.Partners
{
    public class PartnerViewModel : DocumentViewModel
    {
        #region Properties
        private const string PartnerProperties = "Partner";
        private const string PartnersProperty = "Partners";
        
        #endregion

        #region Private properties
        private string _filter;
        #endregion

        #region Public properties
        #region Partners
        private PartnerModel _partner;
        private ObservableCollection<PartnerModel> _partners;
        private List<EsPartnersTypes> _partnersTypes;
        public PartnerModel Partner
        {
            get { return _partner; }
            set { _partner = value; RaisePropertyChanged(PartnerProperties); }
        }

        public ObservableCollection<PartnerModel> Partners
        {
            get { return (string.IsNullOrEmpty(_filter) ? _partners : new ObservableCollection<PartnerModel>(_partners.Where(s => s.PartnerFull.ToLower().Contains(_filter.ToLower())).ToList())); }
            set { _partners = value; }
        }
        public List<EsPartnersTypes> PartnersTypes
        {
            get { return _partnersTypes; }
            private set
            {
                _partnersTypes = value; RaisePropertyChanged("PartnersTypes");
            }
        }
        #endregion Partners
        public string FilterText { get { return _filter; } set { _filter = value; RaisePropertyChanged(PartnersProperty); } }
        #endregion

        #region Constructors
        public PartnerViewModel()
        {
            Initialize();
        }
        #endregion

        #region Private methods

        private void Initialize()
        {
            Title = "Պատվիրատուների խմբագրում";
            PartnersTypes = ApplicationManager.Instance.CashProvider.GetPartnersTypes;
            GetPartners();
            GetNewPartner();

            InitializeCommands();
        }
        private void GetPartners()
        {
            Partners = new ObservableCollection<PartnerModel>(ApplicationManager.Instance.CashProvider.GetPartners);
            RaisePropertyChanged("Partners");
        }
        private void InitializeCommands()
        {
            NewPartnerCommand = new PartnerNewCommand(this);
            AddPartnerCommand = new PartnerAddCommand(this);
            EditPartnerCommand = new PartnerEditCommand(this);
            RemovePartnerCommand = new PartnerRemoveCommand(this);
            SetDefaultPartnerCommand = new RelayCommand<PartnerModel>(OnSetDefault, CanSetDefault);
        }
        #region Command methods
        private bool CanSetDefault(PartnerModel partner)
        {
            return partner != null;
        }
        private void OnSetDefault(PartnerModel partner)
        {
            if (CanSetDefault(partner))
            {
                if (PartnersManager.SetDefault(partner))
                {
                    MessageManager.OnMessage("Գործողության բարեհաջող ավարտ։", MessageTypeEnum.Success);
                }
                else
                {
                    MessageManager.OnMessage("Գործողության ձախողում։", MessageTypeEnum.Warning);
                }
            }
        }
        #endregion
        #endregion

        #region public Methods
        public bool CanNewPartner()
        {
            return Partner == null || _partners.FirstOrDefault(s => s.Id == Partner.Id) != null;
        }

        public void GetNewPartner()
        {
            Partner = new PartnerModel(ApplicationManager.Instance.GetMember.Id);
            RaisePropertyChanged(PartnerProperties);
        }
        public bool CanAddPartner()
        {
            return Partner != null
                && _partners.FirstOrDefault(s => s.Id == Partner.Id) == null
                && !string.IsNullOrEmpty(Partner.FullName)
                && !string.IsNullOrEmpty(Partner.Mobile)
                && Partner.PartnersTypeId > 0;
        }

        public void AddPartner()
        {
            if (!CanAddPartner()) return;
            if (PartnersManager.AddPartner(Partner))
            {
                GetPartners();
                GetNewPartner();
            }
        }
        public bool CanEditPartner()
        {
            return Partner != null && _partners.FirstOrDefault(s => s.Id == Partner.Id) != null
                && !string.IsNullOrEmpty(Partner.FullName)
                && !string.IsNullOrEmpty(Partner.Mobile)
                && Partner.PartnersTypeId > 0;
        }

        public void EditPartner()
        {
            if (!CanEditPartner()) return;
            PartnersManager.EditPartner(Partner);
            GetPartners();
        }

        public bool CanRemovePartner()
        {
            return false;
        }

        public void RemovePartner()
        {

        }
        #endregion

        #region ICommands
        public ICommand NewPartnerCommand { get; private set; }
        public ICommand AddPartnerCommand { get; private set; }
        public ICommand EditPartnerCommand { get; private set; }
        public ICommand RemovePartnerCommand { get; private set; }
        public ICommand SetDefaultPartnerCommand { get; private set; }
        #endregion

    }
}
