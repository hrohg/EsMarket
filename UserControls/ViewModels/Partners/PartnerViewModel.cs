﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using ES.DataAccess.Models;
using UserControls.Commands;
using UserControls.Interfaces;

namespace UserControls.ViewModels.Partners
{
    public class PartnerViewModel:DocumentViewModel
    {
        #region Properties
        private const string PartnerProperties = "Partner";
        private const string PartnersProperty = "Partners";
        private const string FilterProperty = "Filter";
        #endregion

        #region Private properties

        private long _memberId;
        private PartnerModel _partner;
        private ObservableCollection<PartnerModel> _partners;
        private List<EsPartnersTypes> _partnersTypes;
        private string _filter;
        #endregion

        #region Public properties
        public string Title { get; set; }
        public bool IsLoading
        {
            get;
            set;
        }

        public string Description { get; set; }
        public bool IsModified { get; set; }

        public PartnerModel Partner { get { return _partner; } 
            set { _partner = value; OnPropertyChanged(PartnerProperties); } }

        public ObservableCollection<PartnerModel> Partners
        {
            get { return (string.IsNullOrEmpty(_filter)? _partners: new ObservableCollection<PartnerModel>(_partners.Where(s =>s.PartnerFull.ToLower().Contains(_filter.ToLower())).ToList())); } set { _partners = value; }
        }
        public List<EsPartnersTypes> PartnersTypes { get { return _partnersTypes; }  }
        public string FilterText { get { return _filter; } set { _filter = value; OnPropertyChanged(PartnersProperty); } }
        #endregion

        #region Constructors
        public PartnerViewModel(long memberId)
        {
            _memberId = memberId;
            _partnersTypes = PartnersManager.GetPartnersTypes(memberId);
            GetPartners();
            GetNewPartner();
            Initialize();
        }
        #endregion

        #region Private methods

        private void Initialize()
        {
            Title = "Պատվիրատուների խմբագրում";
            InitializeCommands();
        }
        private void GetPartners()
        {
            Partners = new ObservableCollection<PartnerModel>(PartnersManager.GetPartners(_memberId));
            OnPropertyChanged("Partners");
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
        private void OnClose(object o)
        {
            ApplicationManager.OnTabItemClose(o as TabItem);
        }

        private bool CanSetDefault(PartnerModel partner)
        {
            return partner != null;
        }
        private void OnSetDefault(PartnerModel partner)
        {
            if (CanSetDefault(partner))
            {
                ApplicationManager.MessageManager.OnNewMessage(PartnersManager.SetDefault(partner)
                    ? new MessageModel("Գործողության բարեհաջող ավարտ։", MessageModel.MessageTypeEnum.Success)
                    : new MessageModel("Գործողության ձախողում։", MessageModel.MessageTypeEnum.Warning));
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
            Partner=new PartnerModel(_memberId);
            OnPropertyChanged(PartnerProperties);
        }
        public bool CanAddPartner()
        {
            return Partner != null 
                && _partners.FirstOrDefault(s => s.Id == Partner.Id) == null 
                && !string.IsNullOrEmpty(Partner.FullName) 
                && !string.IsNullOrEmpty(Partner.Mobile)
                && Partner.PartnersTypeId>0;
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
                && Partner.PartnersTypeId>0;
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
        public ICommand CloseCommand { get { return new RelayCommand(OnClose);} }
        public ICommand SetDefaultPartnerCommand { get; private set; } 
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
