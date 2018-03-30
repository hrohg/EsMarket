﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Helpers;
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
            ApplicationManager.Instance.CashProvider.UpdatePartners(false);
            Partners = new ObservableCollection<PartnerModel>(ApplicationManager.Instance.CashProvider.GetPartners);
            RaisePropertyChanged("Partners");
        }
        //    private bool CheckLuhn(string purportedCC)
        //    {
        //        if (string.IsNullOrEmpty(purportedCC)) return false;
        //        int sum = 0;
        //        int nDigits = purportedCC.Length;
        //        int parity = nDigits%2;
        //for(int i=0;i<nDigits;++i){
        //    int digit = integer([i]);
        //    if (i%2 == parity)
        //        digit = digit*2;
        //    if (digit > 9)
        //        digit = digit - 9;
        //    sum = sum + digit;
        //}
        //        return (sum%10) == 0;
        //    }
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
                    ApplicationManager.CashManager.UpdatePartners();
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

        #endregion Commands

    }
}
