using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;
using UserControls.ViewModels;

namespace ES.Shop.Users.ViewModels
{
    public class ManageUsersViewModel : TabItemViewModel
    {
        #region Internal properties

        private List<EsUserModel> _esUsers;
        private List<MemberRolesModel> _roles;
        private List<UsersRolesModel> _usersRoleses;
        private EsUserModel _selectedEsUser;
        private MemberRolesModel _selectedRole;
        private UsersRolesModel _userRole;
        private string _newUserEmail;
        #endregion //Internal properties

        #region External properties

        public ObservableCollection<EsUserModel> EsUsers
        {
            get
            {
                return new ObservableCollection<EsUserModel>(_esUsers != null ? _esUsers.Where(s => s.IsActive).OrderBy(s => s.FullName).ToList() : new List<EsUserModel>());
            }
            set
            {
                _esUsers = value.ToList();
                OnPropertyChanged("EsUsers");
            }
        }
        public List<MemberRolesModel> Roles
        {
            get { return _roles ?? new List<MemberRolesModel>(); }
            set { _roles = value; OnPropertyChanged("Roles"); }
        }
        public List<UsersRolesModel> UsersRoles
        {
            get { return _usersRoleses != null ? _usersRoleses.Where(s => s.EsUser.IsActive).ToList() : new List<UsersRolesModel>(); }
            set { _usersRoleses = value; OnPropertyChanged("UsersRoles"); }
        }
        public EsUserModel SelectedEsUser
        {
            get { return _selectedEsUser; }
            set
            {
                _selectedEsUser = value;
                UserRole = new UsersRolesModel { Id = Guid.NewGuid(), EsUser = SelectedEsUser, Role = new MemberRolesModel() };
                OnPropertyChanged("SelectedEsUser");
            }
        }
        public MemberRolesModel SelectedRole { get { return _selectedRole; } set { _selectedRole = value; OnPropertyChanged("SelectedRole"); } }
        public UsersRolesModel UserRole
        {
            get { return _userRole; }
            set { _userRole = value; OnPropertyChanged("UserRole"); }
        }
        public string NewUserEmail
        {
            get { return _newUserEmail; }
            set
            {
                _newUserEmail = value;
                OnPropertyChanged("NewUserEmail");
            }
        }
        #endregion //External properties

        #region Constants

        #endregion //Constants

        #region Constructors

        public ManageUsersViewModel()
        {
            Initialize();
        }
        #endregion //Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Օգտագործողների խմբագրում";
            LoadEsUserCommand = new RelayCommand<string>(OnLoadUser, CanLoadUser);
            RemoveEsUserCommand = new RelayCommand<EsUserModel>(OnRemoveUser, CanRemoveUser);
            AddUserRoleCommand = new RelayCommand(OnAddUserRoles, CanInsertRole);
            RemoveUserRoleCommand = new RelayCommand(OnRemoveRole, CanRemoveRole);
            Load();
        }

        private void Load()
        {
            EsUsers = new ObservableCollection<EsUserModel>(UsersManager.GetEsUsers(ApplicationManager.GetEsMember.Id));
            Roles = UsersManager.GetMemberRoles(ApplicationManager.GetEsMember.Id);
            UsersRoles = UsersManager.GetUsersRoles(ApplicationManager.GetEsMember.Id);
        }
        private bool CanLoadUser(string userId)
        {
            return !string.IsNullOrEmpty(userId);
        }
        private void OnLoadUser(string userId)
        {
            var user = UsersManager.LoadEsUserByEmail(userId);
            if (EsUsers.All(s => s.UserId != user.UserId))
            {
                _esUsers.Add(user);
                OnPropertyChanged("EsUsers");
            }
        }
        private bool CanRemoveUser(EsUserModel user)
        {
            return user != null;
        }
        private void OnRemoveUser(EsUserModel user)
        {
            if (UsersManager.RemoveEsUser(user.UserId)) Load();
        }
        private bool CanInsertRole(object o)
        {
            return SelectedEsUser != null && SelectedRole != null;
        }
        private void OnAddUserRoles(object o)
        {
            var role = new MemberUsersRoles
            {
                MemberId = ApplicationManager.GetEsMember.Id,
                EsUserId = SelectedEsUser.UserId,
                MemberRoleId = SelectedRole.Id
            };
            if (UsersManager.InsertUserRole(role)) UsersRoles = UsersManager.GetUsersRoles(ApplicationManager.GetEsMember.Id); ;
        }
        private bool CanRemoveRole(object o)
        {
            return UserRole != null;
        }

        private void OnRemoveRole(object o)
        {
            if (UsersManager.RemoveUserRole(UserRole)) UsersRoles = UsersManager.GetUsersRoles(ApplicationManager.GetEsMember.Id);
        }
        #endregion //Internal methods

        #region External methods
        #endregion //External methods

        #region Commands
        public ICommand LoadEsUserCommand { get; private set; }
        public ICommand AddUserRoleCommand { get; private set; }
        public ICommand RemoveUserRoleCommand { get; private set; }
        public ICommand RemoveEsUserCommand { get; private set; }

        #endregion //Commands

    }
}
