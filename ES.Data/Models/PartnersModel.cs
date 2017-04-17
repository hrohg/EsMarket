﻿using System;
using System.ComponentModel;
using ES.Data.Model;

namespace ES.Data.Models
{
    public class PartnerModel:INotifyPropertyChanged
    {
        #region EsUserModel properties

        private const string IdProperty = "Id";
        private const string EsMemberIdProperty = "EsMemberId";
        private const string PartnersTypeIdProperty = "PartnerTypeId";
        private const string EsUserIdProperty = "EsUserId";
        private const string ClubSixteenIdProperty = "ClubSixteenId";
        private const string FullNameProperty = "FullName";
        private const string FirstnameProperty = "Firstname";
        private const string LastNameProperty = "LastName";
        private const string MobileProperty = "Mobile";
        private const string EmailProperty = "Email";
        private const string AddressProperty = "Address";
        private const string DiscountProperty = "Discount";
        private const string DebitProperty = "Debit";
        private const string CreditProperty = "Credit";
        private const string MinBalanceProperty = "MaxDebit";
        private const string TINProperty = "TIN";
        private const string PasportDataProperty = "PasportData";
        private const string JuridicalAddressProperty = "JuridicalAddress";
        private const string BankProperty = "Bank";
        private const string BankAccountProperty = "BankAccount";
        private const string NotesProperty = "Notes";
        #endregion
        /// <summary>
        /// EsUsermodel private properties
        /// </summary>
        #region Private properties
        private Guid _id=Guid.NewGuid();
        private long _esMemberId;
        private long? _partnersTypeId;
        private PartnerTypeModel _partnerType;
        private long? _esUserId;
        private EsUserModel _esUser;
        private string _clubSixteenId;
        private string _fullName;
        private string _firstName;
        private string _lastName;
        private string _mobile;
        private string _email;
        private string _address;
        private decimal? _discount;
        private decimal _debit;
        private decimal _credit;
        private decimal? _maxDebit;
        private string _tin;
        private string _pasportData;
        private string _juridicalAddress;
        private string _bank;
        private string _bankAccount;
        private string _notes;
        #endregion
        /// <summary>
        /// EsUserModel public properties
        /// </summary>
        #region Public properties
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public long EsMemberId {get { return _esMemberId; }set {_esMemberId= value; OnPropertyChanged(EsMemberIdProperty); }}
        public EsMemberModel EsMember { get; set; }
        public long? PartnersTypeId
        {
            get { return _partnersTypeId; }
            set { _partnersTypeId = value; OnPropertyChanged(PartnersTypeIdProperty); }
        }
        public PartnerTypeModel PartnersType { get { return _partnerType; } set { _partnerType = value; } }
        public long? EsUserId { get { return _esUserId; } set { _esUserId = value; } }
        public EsUserModel EsUser { get { return _esUser; } set { _esUser = value; } }
        public string ClubSixteenId { get { return _clubSixteenId; } set { _clubSixteenId = value; OnPropertyChanged(ClubSixteenIdProperty); } }
        public string FullName{get { return _fullName; }set { _fullName = value; OnPropertyChanged(FullNameProperty); }}
        public string PartnerFull { get {return ((FullName ?? string.Empty) + (FullName ?? string.Empty) + (LastName ?? string.Empty) + (Mobile ?? string.Empty));} }
        public string Description { get { return string.Format("{0} {1}", FullName, MobileByFormating);} }
        public string FirstName { get { return _firstName; } set { _firstName = value; OnPropertyChanged(FirstnameProperty); } }
        public string LastName {get { return _lastName; } set { _lastName = value; OnPropertyChanged(LastNameProperty); }}
        public string Mobile { get { return _mobile; } set
        {
            if(value!=null)
            {value = value.Replace(" ", string.Empty);
            value = value.Replace("-", string.Empty);}
            _mobile = value; OnPropertyChanged(MobileProperty); } }
        public string MobileByFormating { get
        {
            return Mobile==null || Mobile.Length != 12? Mobile: string.Format("({0} {1}) {2} {3} {4}", 
                Mobile.Substring(0, 4), 
                Mobile.Substring(4, 2),
                Mobile.Substring(6, 2), 
                Mobile.Substring(8, 2), 
                Mobile.Substring(10, 2));
        } }
        public string Email{get { return _email; }set { _email = value; OnPropertyChanged(EmailProperty); }}
        public string Address{get { return _address; }set { _address= value; OnPropertyChanged(AddressProperty);}}
        public decimal? Discount{get { return _discount; }set { _discount = value; OnPropertyChanged(DiscountProperty); }}
        public decimal Debit {get { return _debit; }set { _debit= value; OnPropertyChanged(DebitProperty); }}

        public decimal Credit
        {
            get
            {
                return _credit;
            }
            set
            {
                _credit = value;
                OnPropertyChanged(CreditProperty);
            }
        }

        public decimal? MaxDebit { get { return _maxDebit; } set { _maxDebit = value; OnPropertyChanged(MinBalanceProperty); } }
        public string TIN { get { return _tin; } set { _tin = value; OnPropertyChanged(TINProperty); } }
        public string PasportData { get { return _pasportData; } set { _pasportData = value; OnPropertyChanged(PasportDataProperty); } }
        public string JuridicalAddress { get { return _juridicalAddress; } set { _juridicalAddress = value; OnPropertyChanged(JuridicalAddressProperty); } }
        public string Bank { get { return _bank; } set { _bank = value; OnPropertyChanged(BankProperty); } }
        public string BankAccount {get { return _bankAccount; } set { _bankAccount = value; OnPropertyChanged(BankAccountProperty); }}
        public string Notes { get { return _notes; } set { _notes = value; OnPropertyChanged(NotesProperty); } }
        #endregion
        public PartnerModel()
        {
            
        }
        public PartnerModel(long memberId)
        {
            EsMemberId = memberId;
        }
        /// <summary>
        /// EsUserModel methods
        /// </summary>

        #region private methods
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
