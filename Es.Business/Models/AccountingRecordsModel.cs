using System;
using System.Collections.Generic;
using System.Linq;
using ES.Business.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;

namespace ES.Business.Models
{
    public class AccountingRecordsModel : ViewModelBase
    {
        #region Properties

        private readonly string AmountProperty = "Amount";
        private readonly string DescriptionProperty = "Description";
        #endregion

        #region Private properties
        private Guid _id = Guid.NewGuid();
        private DateTime _registerDate = DateTime.Now;
        private string _description;
        private decimal _amount;
        private long _debit;
        private long _credit;
        private long _memberId;
        private long _registerId;
        private Guid? _debitGuidId;
        private Guid? _creditGuidId;
        private long? _debitLongId;
        private long? _creditLongId;
        #endregion

        #region public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public DateTime RegisterDate { get { return _registerDate; } set { _registerDate = value; } }
        public string Description { get { return _description; } set { _description = value; RaisePropertyChanged(DescriptionProperty); } }
        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; RaisePropertyChanged(AmountProperty); }
        }
        public long Debit
        {
            get { return _debit; }
            set { _debit = value; }
        }
        public long Credit { get { return _credit; } set { _credit = value; } }
        public long MemberId { get { return _memberId; } set { _memberId = value; } }
        public long RegisterId { get { return _registerId; } set { _registerId = value; } }
        public Guid? DebitGuidId { get { return _debitGuidId; } set { _debitGuidId = value; } }
        public Guid? CreditGuidId { get { return _creditGuidId; } set { _creditGuidId = value; } }
        public long? DebitLongId { get { return _debitLongId; } set { _debitLongId = value; } }
        public long? CreditLongId { get { return _creditLongId; } set { _creditLongId = value; } }

        public string DebitDescription { get { return string.Format("{0} ({1})", AccountanttDescriptions.Description((int)Debit), Debit); } }
        public string DebitDetile
        {
            get
            {
                return AccountanttDescriptions.Detile((int)Debit, DebitLongId, DebitGuidId);
            }
        }
        public string CreditDescription { get { return string.Format("{0} ({1})", AccountanttDescriptions.Description((int)Credit), Credit); } }
        public string CreditDetile
        {
            get
            {
                return AccountanttDescriptions.Detile((int)Credit, CreditLongId, CreditGuidId);
            }
        }

        public EsUserModel Cashier { get { return ApplicationManager.Instance.CashProvider.GetUser(RegisterId); } }
        #endregion

        public AccountingRecordsModel(DateTime date, long memberId, long registerId)
        {
            RegisterDate = date;
            MemberId = memberId;
            RegisterId = registerId;
        }
        public AccountingRecordsModel()
        {
            RegisterDate = DateTime.Now;
            MemberId = ApplicationManager.Instance.GetMember.Id;
            RegisterId = ApplicationManager.GetEsUser.UserId;
        }
    }

    public class AccountanttDescriptions
    {
        #region Internal properties
        private string[] _descriptions = new string[911];
        private List<PartnerModel> _partners;
        private List<StockModel> _stocks;
        #region Cashdesk
        private List<CashDesk> _cashDesk;
        public List<CashDesk> CashDesks { get { return _cashDesk ?? (_cashDesk = CashDeskManager.GetCashDesks()); } }
        #endregion Cashdesk
        #endregion Internal properties

        #region External properties
        #endregion External properties

        #region Constructors

        private static AccountanttDescriptions _instance;
        public static AccountanttDescriptions Instance { get { return _instance ?? (_instance = new AccountanttDescriptions()); } }

        public AccountanttDescriptions()
        {
            Initilize();
        }

        #endregion Consructors

        #region Internal methods
        private void Initilize()
        {
            //1 ՈՉ ԸՆԹԱՑԻԿ ԱԿՏԻՎՆԵՐ

            //2 Ընթացիկ ակտիվներ

            _descriptions[211] = "Դեբիտորական պարտքի մարում";
            _descriptions[216] = "Ապրանքի ձեռքբերում";
            _descriptions[221] = "Դեբիտորական պարտքեր վաճառքի գծով";
            _descriptions[224] = "Տրված ընթացիկ կանխավճարներ";

            _descriptions[251] = "Դրամարկղ";

            //3 Սեփական կապիտալ

            //4 Ոչ ընթացիկ պարտավորություններ            
            _descriptions[311] = "Կանոնադրական կապիտալ";

            //5 Ընթացիկ պարտավորություններ
            _descriptions[521] = "Կրեդիտորական պերտքեր գնումների գծով";
            _descriptions[523] = "Ստացված կանխավճարներ";
            _descriptions[527] = "Պարտքեր աշխատավարձի և աշխատակիցների այլ կարճաժամկետ հատուցումների գծով";
            //6 Եկամուտներ
            _descriptions[611] = "Հասույթ";

            //7 Ծախսեր
            _descriptions[711] = "Իրացված արտադրանքի, ապրանքների, աշխատանքների, ծառայությունների ինքնարժեք";
            _descriptions[712] = "Իրացման ծախսեր";
            _descriptions[714] = "Գործառնական այլ ծախսեր";

            //8 Կառավարչական հաշվառման հաշիվներ

            //9 Արտահաշվեկշռային հաշիվներ


            //_partners = ApplicationManager.Instance.CashProvider.GetPartners;
            //_stocks = ApplicationManager.Instance.CashProvider.GetStocks;
            //_cashDesk = ApplicationManager.Instance.CashProvider.GetCashDesk;
        }
        #endregion Internal methods

        #region External methods
        public static string Description(int code)
        {
            return Instance._descriptions[code];
        }
        public static string Detile(int code, long? longId, Guid? guidId)
        {
            if (longId == null && guidId == null) return string.Empty;
            string detile = null;
            PartnerModel partner = null;
            switch (code)
            {

                case 216:
                    var stock = ApplicationManager.Instance.CashProvider.GetStocks.Single(s => s.Id == longId);
                    detile = stock != null ? stock.Name : "Unknown";
                    break;
                case 221:
                    partner = ApplicationManager.Instance.CashProvider.GetPartners.Single(s => s.Id == guidId);
                    detile = partner != null ? partner.Description : "unknown";
                    break;
                case 251:
                    var cashDesk = ApplicationManager.Instance.CashProvider.GetCashDesk.SingleOrDefault(s => s.Id == guidId);
                    detile = cashDesk != null ? cashDesk.Name : "Unknown";
                    break;
                case 521:
                    partner = ApplicationManager.Instance.CashProvider.GetPartners.Single(s => s.Id == guidId);
                    detile = partner != null ? partner.Description : "unknown";
                    break;
                default:
                    break;
            }
            return detile;
        }
        #endregion External methods

    }
}
