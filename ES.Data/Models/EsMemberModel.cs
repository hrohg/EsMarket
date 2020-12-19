using System.ComponentModel;

namespace ES.Data.Models
{
    public class EsMemberModel:INotifyPropertyChanged
    {
        /// <summary>
        /// Properties
        /// </summary>
        #region Properties
        private const string IdProperty = "Id";
        private const string NameProperty = "Name";
        private const string CntractNumberProperty = "ContractNumber";
        #endregion
        /// <summary>
        /// Private properties
        /// </summary>
        #region Private properties
        private int _id;
        private string _name;
        private string _contractNumber;
        #endregion
        /// <summary>
        /// Public properties
        /// </summary>
        #region Public properties
        public int Id { get { return _id; } set { _id=value; OnPropertyChanged(IdProperty);} }
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(NameProperty); } }
        public string ContractNumber { get { return _contractNumber; } set { _contractNumber = value; OnPropertyChanged(CntractNumberProperty); } }
        #endregion
        /// <summary>
        /// Property changed
        /// </summary>
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
    public class EsMembersAccountsModel
    {
        #region External properties
        public long MemberId { get; set; }
        public decimal TotalScore { get; set; }
        public EsMembersAccountsModel(long memberId)
        {
            MemberId = memberId;
            TotalScore = 0;
        }
        public EsMembersAccountsModel(int memberId, decimal totalScore)
        {
            MemberId = memberId;
            TotalScore = totalScore;
        }
        #endregion
    }
}
