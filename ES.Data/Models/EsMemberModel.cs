using System.ComponentModel;

namespace ES.Data.Model
{
    public class EsMemberModel:INotifyPropertyChanged
    {
        /// <summary>
        /// Properties
        /// </summary>
        #region Properties
        private const string IdProperty = "Id";
        private const string FullNameProperty = "FullName";
        private const string EmailProperty = "Email";
        private const string ClubSixteenIdProperty = "ClubSisteenId";
        #endregion
        /// <summary>
        /// Private properties
        /// </summary>
        #region Private properties
        private long _id=0;
        private string _fullName;
        private string _email;
        private string _clubSixteenId;
        #endregion
        /// <summary>
        /// Public properties
        /// </summary>
        #region Public properties
        public long Id { get { return _id; } set { _id=value; OnPropertyChanged(IdProperty);} }
        public string FullName { get { return _fullName; } set { _fullName = value; OnPropertyChanged(FullNameProperty); } }
        public string Email { get { return _email; } set { _email = value; OnPropertyChanged(EmailProperty); } }
        public string ClubSixteenId { get { return _clubSixteenId; } set { _clubSixteenId = value; OnPropertyChanged(ClubSixteenIdProperty); } }
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
        public long TotalScore { get; set; }
        public EsMembersAccountsModel(long memberId)
        {
            MemberId = memberId;
            TotalScore = 0;
        }
        public EsMembersAccountsModel(long memberId, long totalScore)
        {
            MemberId = memberId;
            TotalScore = totalScore;
        }
        #endregion
    }
}
