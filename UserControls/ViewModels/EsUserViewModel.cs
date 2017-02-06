using System.Windows.Input;
using ES.Data.Model;
using UserControls.Commands;

namespace UserControls.ViewModels
{
    public class EsUserViewModel
    {
        /// <summary>
        /// Initalize a new instance of the EsUserViewModel class.
        /// </summary>
        private EsUserModel _esUser ;

        public EsUserModel EsUser
        {
            get { return _esUser; }
            set { _esUser = value; }
        }

        public EsUserViewModel(EsUserModel esUser)
        {
            _esUser = esUser;
            EditCommand = new EsUserChangeCommand(this);
        }

        public bool CanEdit
        {
            get { return EsUser.NewPassword==EsUser.ConfirmPassword && !string.IsNullOrEmpty(EsUser.NewPassword); }
        }

        public ICommand EditCommand
        {
            get; private set; }
        public void ChangePassword()
        { }
    }
}
