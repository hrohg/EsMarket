using System;
using System.ComponentModel;

namespace ES.Data.Models
{
    public class EsUserModel : INotifyPropertyChanged
    {

        #region EsUserModel properties
        private const string UserNameProperty = "UserName";
        private const string EmailProperty = "Email";
        private const string MobileProperty = "Mobile";
        private const string CardNumberProperty = "CardNumber";
        private const string IsActiveProperty = "IsActive";
        private const string PasswordProperty = "Password";
        private const string NewPasswordProperty = "NewPassword";
        private const string ConfirmPasswordProperty = "ConfirmPassword";
        private const string FirstNameProperty = "FirstName";
        private const string LastNameProperty = "LastName";
        #endregion
        /// <summary>
        /// EsUsermodel private properties
        /// </summary>
        #region Private properties
        private int _userId;
        private string _userName;
        private string _email;
        private string _mobile;
        private bool _isActive;
        private string _essClubId;
        private string _password;
        private string _newPassword;
        private string _confirmPassword;
        private DateTime _lastActivityDate;

        private string _firstName;
        private string _lastName;
        #endregion
        /// <summary>
        /// EsUserModel public properties
        /// </summary>
        #region Public properties
        public int UserId
        {
            get { return _userId; }
            private set { _userId = value; }
        }
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(UserNameProperty); }
        }
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; OnPropertyChanged(FirstNameProperty); }
        }
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; OnPropertyChanged(LastNameProperty); }
        }
        public string FullName
        {
            get
            {
                return UserName; //FirstName + " " + LastName;
            }
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; OnPropertyChanged(EmailProperty); }
        }

        public string Mobile
        {
            get { return _mobile; }
            set { _mobile = value; OnPropertyChanged(MobileProperty); }
        }

        public string EssClubId
        {
            get { return _essClubId; }
            set { _essClubId = value; OnPropertyChanged(CardNumberProperty); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; OnPropertyChanged(IsActiveProperty); }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(PasswordProperty); }
        }
        public string NewPassword
        {
            get { return _newPassword; }
            set { _newPassword = value; OnPropertyChanged(NewPasswordProperty); }
        }
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { _confirmPassword = value; OnPropertyChanged(ConfirmPasswordProperty); }
        }

        public DateTime LastActivityDate
        {
            get { return _lastActivityDate; }
            set { _lastActivityDate = value; }
        }

        #endregion
        /// <summary>
        /// EsUserModel Constructors
        /// </summary>
        #region EsUserModel Constructors
        public EsUserModel(int userId)
        {
            UserId = userId;
        }
        #endregion
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
