using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Enumerations;
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
        private List<PartnerModel> _partners;
        private List<EsPartnersTypes> _partnersTypes;
        public PartnerModel Partner
        {
            get { return _partner; }
            set
            {
                _partner = value;
                RaisePropertyChanged(PartnerProperties);
                IsDefault = Partner != null && CashManager.Instance.EsDefaults.Where(s => Partner.PartnerTypeEnum != PartnerType.None && s.Control == PartnersManager.GetControlByPartnerType(Partner.PartnerTypeEnum)).Any(s => s.ValueInGuid == Partner.Id);
                RaisePropertyChanged("IsDefault");
            }
        }

        public List<PartnerModel> Partners
        {
            get { return string.IsNullOrEmpty(_filter) ? _partners : _partners.Where(s => s.PartnerFull.ToLower().Contains(_filter.ToLower())).ToList(); }
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

        private bool _isDefault;
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                _isDefault = value;
                CashManager.Instance.UpdateDefaults();
                RaisePropertyChanged("IsDefault");
            }
        }
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
            PartnersTypes = CashManager.PartnersTypes;
            GetPartners();
            GetNewPartner();

            InitializeCommands();
        }
        private void GetPartners()
        {
            ApplicationManager.CashManager.UpdatePartnersAsync(false);
            _partners = ApplicationManager.CashManager.GetPartners;
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

        private void UpdatePartners()
        {
            ApplicationManager.CashManager.UpdatePartnersAsync();

        }
        #region Command methods

        private bool CanSetDefault(PartnerModel partner)
        {
            return partner != null;
        }
        private void OnSetDefault(PartnerModel partner)
        {
            if (!CanSetDefault(partner)) return;
            if (PartnersManager.SetDefault(partner, IsDefault))
            {
                UpdatePartners();
                MessageManager.OnMessage("Գործողության բարեհաջող ավարտ։", MessageTypeEnum.Success);
            }
            else
            {
                MessageManager.OnMessage("Գործողության ձախողում։", MessageTypeEnum.Warning);
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

        #region Commands
        public ICommand NewPartnerCommand { get; private set; }
        public ICommand AddPartnerCommand { get; private set; }
        public ICommand EditPartnerCommand { get; private set; }
        public ICommand RemovePartnerCommand { get; private set; }
        public ICommand SetDefaultPartnerCommand { get; private set; }

        private ICommand _generateLocalIdCommand;
        public ICommand GenerateLocalIdCommand { get { return _generateLocalIdCommand ?? (_generateLocalIdCommand = new RelayCommand(OnGenerateLocalId, CanGenerateLocalId)); } }
        private bool CanGenerateLocalId(object obj)
        {
            return Partner != null && string.IsNullOrEmpty(Partner.ClubSixteenId);
        }
        private void OnGenerateLocalId(object obj)
        {
            if (!CanGenerateLocalId(obj)) return;
            //Country code
            var code1 = "3741";
            //Application code
            var code2 = "0000";
            //Member code
            var code3 = "0005";
            //Partner code
            var code4 = "3741";
            //var checkSum = BarCodeGenerator.CalculateChecksumDigit(string.Format("{0},{1},{2}", code1, code2, code3));
            Partner.ClubSixteenId = string.Format("{0}{1}{2}{3}", code1, code2, code3, code4);
        }

        private ICommand _synPartnerCommand;
        public ICommand SyncPartnerCommand { get { return _synPartnerCommand ?? (_synPartnerCommand = new RelayCommand(OnSync, CanSync)); } }

        private bool CanSync(object obj)
        {
            return Partner != null && !string.IsNullOrEmpty(Partner.ClubSixteenId);
        }

        private void OnSync(object obj)
        {
            Partner.ClubSixteenId = null;
        }

        private ICommand _importCommand;
        public ICommand ImportCommand { get { return _importCommand ?? (_importCommand = new RelayCommand(OnImport, CanImport)); } }

        private bool CanImport(object obj)
        {
            return Partner != null;
        }

        private void OnImport(object obj)
        {
            var partners = XmlManager.Load<List<PartnerModel>>();
            if (partners == null) return;
            foreach (var partner in partners)
            {
                partner.EsMemberId = ApplicationManager.Member.Id;
                partner.Discount = 0;
                partner.Debit = 0;
                partner.Credit = 0;
                partner.MaxDebit = 0;

                var expartner = Partners.FirstOrDefault(s =>
                        (!string.IsNullOrEmpty(partner.ClubSixteenId) && s.ClubSixteenId == partner.ClubSixteenId) ||
                        (!string.IsNullOrEmpty(partner.Email) && s.Email == partner.Email) ||
                        s.FullName == partner.FullName);
                if (expartner == null)
                {
                    partner.Id = Guid.NewGuid();
                    PartnersManager.AddPartner(partner);
                }
                else
                {
                    partner.Id = expartner.Id;
                    PartnersManager.EditPartner(partner);
                }
            }
        }
        private ICommand _exportCommand;
        public ICommand ExportCommand { get { return _exportCommand ?? (_exportCommand = new RelayCommand(OnExport, CanExport)); } }

        private bool CanExport(object obj)
        {
            return true;
        }

        private void OnExport(object obj)
        {
            //XmlManager.Save(Partner);
            XmlManager.Save(Partners);
        }
        #endregion Commands

    }
}
