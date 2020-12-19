using System;
using System.ComponentModel;

namespace ES.Business.Models
{
    public class ServicesModel : INotifyPropertyChanged
    {
        #region Properties

        private const string CodeProperty = "Code";
        private const string DescriptionProperty = "Description";
        private const string MuProperty = "Mu";
        private const string NoteProperty = "Note";
        private const string CostPriceProperty = "CostPrice";
        private const string DealerPrcentageProperty = "DealerPercentage";
        private const string DealerPriceProperty = "DealerPrice";
        private const string PricePercentageProperty = "PricePercentage";
        private const string PriceProperty = "Price";
        private const string DiscountProperty = "Discount";
        
        #endregion
        #region Private properties

        private Guid _id=Guid.NewGuid();
        private string _code;
        private string _description;
        private string _mu;
        private decimal? _costPrice;
        private decimal? _dealerPrice;
        private decimal? _price;
        private decimal? _discount;
        private string _note;
        private int _memberId;
        private bool? _isActive;
        #endregion
        #region Public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public string Code { get { return _code; } set { _code = value; OnPropertyChanged(CodeProperty);} }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public string Mu { get { return _mu; } set { _mu = value; OnPropertyChanged(MuProperty); } }
        public string Note { get { return _note; } set { _note = value; OnPropertyChanged(NoteProperty); } }

        public decimal? CostPrice
        {
            get
            {
                return _costPrice;
            }
            set
            {
                _costPrice = value;
                OnPropertyChanged(CostPriceProperty);
                OnPropertyChanged(DealerPrcentageProperty);
                OnPropertyChanged(PricePercentageProperty);
            }
        }

        public decimal? DealerPrice { get { return _dealerPrice; } set { _dealerPrice = value; OnPropertyChanged(DealerPriceProperty);OnPropertyChanged(DealerPrcentageProperty); } }
        public decimal? DealerPercentage { 
            get { return CostPrice == null || CostPrice == 0 ? null : (DealerPrice - CostPrice) * 100 / CostPrice; } 
            set { DealerPrice = CostPrice + CostPrice*value/100;  } }
        public decimal? Price { get { return _price; } set { _price = value; OnPropertyChanged(PriceProperty); OnPropertyChanged(PricePercentageProperty); } }
        public decimal? PricePercentage
        {
            get { return CostPrice == null || CostPrice == 0 ? null : (Price - CostPrice) * 100 / CostPrice; }
            set { Price = CostPrice + CostPrice * value / 100; }
        }
        public decimal? Discount { get { return _discount; } set { _discount = value; OnPropertyChanged(DiscountProperty);}}
        public int MemberId { get { return _memberId; } set { _memberId = value; } }
        public bool? IsActive { get { return _isActive; } set { _isActive = value;} }
        #endregion

        public ServicesModel(int memberId, bool isActive = true)
        {
            MemberId = memberId;
            IsActive = isActive;
        }

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
