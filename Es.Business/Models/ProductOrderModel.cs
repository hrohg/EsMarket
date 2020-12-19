using System;
using System.ComponentModel;

namespace ES.Business.Models
{
    public class ProductOrdersModel:INotifyPropertyChanged
    { 
    
          /// <summary>
        /// Initialize a new instance of the Product class.
        /// </summary>

        #region Properties
        private const string IdProperty = "Id";
        private const string EsMemberIdProperty = "EsMemberId";
        private const string OrderNumberProperty = "OrderNumber";
        private const string CreatorIdProperty = "CreatorId";
        private const string CreateDateProperty = "CreateDate";
        private const string ResponsibleIdProperty="RespnsibleId";
        private const string NotesProperty = "Notes";
        private const string AcceptedProperty = "Accepted";
        private const string InProgressProperty = "InProgress";
        private const string CompletedProperty = "Completed";
        
        #endregion
        #region private properties
        private Guid _id = Guid.NewGuid();
        private int _esMemberId;
        private string _orderNumber;
        private int _creatorId;
        private DateTime _createDate;
        private int? _responsibleId;
        private string _notes;
        private bool? _accepted;
        private bool? _inProgress;
        private bool? _completed;
        #endregion
        #region public properties
        public Guid Id { get { return _id; } set { _id = value; OnPropertyChanged(IdProperty); } }
        public int EsMemberId{get { return _esMemberId; }set{_esMemberId = value;OnPropertyChanged(EsMemberIdProperty);}}
        public string OrderNumber { get { return _orderNumber; } set { _orderNumber = value;OnPropertyChanged(OrderNumberProperty); } }
        public int CreatorId { get { return _creatorId; } set { _creatorId = value; OnPropertyChanged(CreatorIdProperty);} }
        public DateTime CreateDate { get { return _createDate; } set { _createDate = value; OnPropertyChanged(CreateDateProperty); } }
        public int? ResponsibleId { get { return _responsibleId; } set { _responsibleId = value;OnPropertyChanged(ResponsibleIdProperty); } }
        public string Notes { get { return _notes; } set { _notes = value; OnPropertyChanged(NotesProperty); } }
        public bool? Accepted {get { return _accepted; } set {_accepted = value;OnPropertyChanged(AcceptedProperty);}}
        public bool? InProgress { get { return _inProgress; } set { _inProgress = value;OnPropertyChanged(InProgressProperty); } }
        public bool? Completed{ get { return _completed; } set {_completed= value;OnPropertyChanged(CompletedProperty); } }
        #endregion
        #region Constructors

        public ProductOrdersModel()
        {
            
        }
        #endregion
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
