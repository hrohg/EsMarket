using System;
using System.ComponentModel;

namespace ES.Business.Models
{
    public class SubAccountingPlanModel:INotifyPropertyChanged
    {
        #region ptoprties

        private const string IdProperty = "Id";
        private const string AccountingPlanIdProperty = "AccountingPlanId";
        private const string SubAccountingPlanIdProperty = "SubAccountingPlanId";
        private const string NameProperty = "Name";
        private const string DescriptionProperty = "Description";
        private const string AmountProperty = "Amount";
        private const string MemberidProperty = "MemberId";
        private const string IsActiveProperty = "IsActive";
        #endregion

        #region private properties
        private short _accountingPlanId;
        private short? _subAccountingPlanId;
        private Guid _id = Guid.NewGuid();
        private string _name;
        private string _description;
        private decimal _amount;
        private int _memberId;
        private bool _isActive;
        #endregion

        #region public properties
        public Guid Id { get { return _id; } set { _id = value; OnPropertyChanged(IdProperty);}}
        public short AccountingPlanId { get { return _accountingPlanId; } set { _accountingPlanId = value; OnPropertyChanged(AccountingPlanIdProperty); } }
        public short? SubAccountingPlanId { get { return _subAccountingPlanId; } set { _subAccountingPlanId = value; OnPropertyChanged(SubAccountingPlanIdProperty); } }
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(NameProperty); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty);} }
        public decimal Amount { get { return _amount; } set { _amount = value; OnPropertyChanged(AmountProperty); } }
        public int MemberId { get { return _memberId; } set { _memberId = value; OnPropertyChanged(MemberidProperty);} }
        public bool IsActive { get { return _isActive; } set { _isActive = value; OnPropertyChanged(IsActiveProperty); } }
        #endregion

        public SubAccountingPlanModel()
        {
            
        }
        #region private methods

        #endregion

        #region public methods

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
