using System.ComponentModel;

namespace ES.Data.Model
{
    public class StockModel:INotifyPropertyChanged
    {
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
        #region Properties
        private const string IdProperty = "Id";
        private const string ParentIdProperty = "ParemtId";
        private const string StoreKeeperIdProperty = "StorekeeperId";
        private const string NameProperty = "Name";
        private const string DescriptionProperty = "Description";
        private const string AddressPropert = "Address";
        private const string SpecialCodeProperty = "SpecialCode";
        private const string IsEnableProperty = "IsEnable";
        private const string MemberIdProperty = "MemberId";
        #endregion
        #region Private properties
        private long _id;
        private long? _parentId;
        private long? _storekeeperId;
        private string _name;
        private string _description;
        private string _address;
        private string _specialCode;
        private bool _isEnable;
        private long _memberId;
        #endregion
        #region Public properties
        public long Id { get{return _id;} set { _id = value;OnPropertyChanged(IdProperty); }}
        public long? ParentId { get { return _parentId; } set { _parentId = value; OnPropertyChanged(ParentIdProperty);} }
        public  StockModel ParentStock { get; set; }
        public long? StorekeeperId { get { return _storekeeperId; } set { _storekeeperId = value; OnPropertyChanged(StoreKeeperIdProperty); } }
        public EsUserModel Storekeeper { get; set; }
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(NameProperty); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public string Address { get { return _address; } set { _address = value; OnPropertyChanged(AddressPropert); } }
        public string SpecialCode { get { return _specialCode; } set { _specialCode = value; OnPropertyChanged(SpecialCodeProperty); } }
        public bool IsEnable { get { return _isEnable; } set { _isEnable = value; OnPropertyChanged(IsEnableProperty); } }
        public long MemberId { get { return _memberId; } set { _memberId = value; OnPropertyChanged(MemberIdProperty); } }
        public EsMemberModel Member { get; set; }
        public string FullName { get { return string.Format("{0},{1} ({2} {3})",Name, Address, Storekeeper!=null? Storekeeper.LastName:string.Empty,Storekeeper!=null? Storekeeper.FirstName: string.Empty);} }
        #endregion
        public StockModel()
        {

        }
        #region Private properties
        #endregion
        #region Public methods
        #endregion
        
    }
}
