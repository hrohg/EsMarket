using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;
using Xceed.Wpf.AvalonDock.Layout;

namespace UserControls.ViewModels.Settings
{
    public class ManageUsersViewModel : ToolsViewModel
    {
        #region Internal properties
        #endregion //Internal properties

        #region External properties
        
        #region User mail or phone
        private string _userMailOrPhone;
        public string UserEmailOrPhone { get { return _userMailOrPhone; } set { _userMailOrPhone = value; RaisePropertyChanged("UserEmailOrPhone"); } }
        #endregion User mail or phone

        public bool IsEnabledEditMode { get { return SelectedEsUser != null; } }

        #region Users
        private List<EsUserModel> _esUsers;
        public ObservableCollection<EsUserModel> EsUsers
        {
            get
            {
                return new ObservableCollection<EsUserModel>(_esUsers != null ? _esUsers.Where(s => s.IsActive).OrderBy(s => s.FullName).ToList() : new List<EsUserModel>());
            }
            set
            {
                _esUsers = value.ToList();
                RaisePropertyChanged("EsUsers");
            }
        }
        #endregion Users

        #region Roles
        private List<UserRole> _roles;
        public List<UserRole> Roles
        {
            get { return _roles ?? new List<UserRole>(); }
            set { _roles = value; RaisePropertyChanged("Roles"); }
        }
        #endregion Roles

        #region Selected user
        private EsUserModel _selectedEsUser;
        public EsUserModel SelectedEsUser
        {
            get { return _selectedEsUser; }
            set
            {
                _selectedEsUser = value;
                foreach (var role in Roles)
                {
                    role.IsSelected = UsersRoles.Any(s =>s.EsUser.UserId==SelectedEsUser.UserId && s.Role.Id == role.Role.Id);
                }
                RaisePropertyChanged("SelectedEsUser");
                RaisePropertyChanged("IsEnabledEditMode");
                RaisePropertyChanged("Roles");
            }
        }
        #endregion Selected user

        #region Users roles
        private List<UsersRolesModel> _usersRoleses;
        private List<UsersRolesModel> UsersRoles { get { return _usersRoleses; } set { _usersRoleses = value; RaisePropertyChanged("UsersRoles"); } }
        #endregion Users roles

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
            AnchorSide = AnchorSide.Right;
            LoadEsUserCommand = new RelayCommand<string>(OnLoadUser, CanLoadUser);
            RemoveEsUserCommand = new RelayCommand<EsUserModel>(OnRemoveUser, CanRemoveUser);
            EditUserRoleCommand = new RelayCommand(OnEditUser, CanEditRole);
            Load();
        }

        private void Load()
        {
            SelectedEsUser = null;
            EsUsers = new ObservableCollection<EsUserModel>(UsersManager.GetEsUsers(ApplicationManager.Instance.GetMember.Id));
            UsersRoles = UsersManager.GetUsersRoles();
            Roles = UsersManager.GetMemberRoles().Select(s => new UserRole(s)).ToList();
        }
        private bool CanLoadUser(string userEmailOrMobile)
        {
            return !string.IsNullOrEmpty(userEmailOrMobile);
        }
        private void OnLoadUser(string userEmailOrMobile)
        {
            var user = UsersManager.LoadEsUserByEmail(userEmailOrMobile);
            if (user != null && EsUsers.All(s => s.UserId != user.UserId))
            {
                _esUsers.Add(user);
                UserEmailOrPhone = string.Empty;
                RaisePropertyChanged("EsUsers");
            }
        }
        private bool CanRemoveUser(EsUserModel user)
        {
            return user != null;
        }
        private void OnRemoveUser(EsUserModel user)
        {
            if (CanRemoveUser(user) && UsersManager.RemoveEsUser(user.UserId)) Load();
        }
        private bool CanEditRole(object o)
        {
            return SelectedEsUser != null && Roles.Any(s => s.IsSelected);
        }
        private void OnEditUser(object o)
        {
            var memberId = ApplicationManager.Instance.GetMember.Id;
            if (UsersManager.EditUser(SelectedEsUser, Roles.Where(s => s.IsSelected).Select(s =>
                new MemberUsersRoles
                {
                    Id = Guid.NewGuid(),
                    EsUserId = SelectedEsUser.UserId,
                    MemberRoleId = s.Role.Id,
                    MemberId = memberId
                }).ToList(), memberId))
            {
                UsersRoles = UsersManager.GetUsersRoles();
                MessageManager.OnMessage("Օգտագործողների խմբագրումն իրականացել է հաջողությամբ:");
            }
            else
            {
                MessageManager.OnMessage("Օգտագործողների խմբագրումը ձախողվել է:");
            }
        }
        #endregion //Internal methods

        #region External methods
        #endregion //External methods

        #region Commands
        public ICommand LoadEsUserCommand { get; private set; }
        public ICommand EditUserRoleCommand { get; private set; }
        public ICommand RemoveEsUserCommand { get; private set; }

        #endregion //Commands

    }

    public class UserRole:ViewModelBase
    {
        #region External properties
        public string Alias { get { return Role != null ? Role.Name : string.Empty; } }
        public string Description { get { return Role != null ? Role.Description : string.Empty; } }
        public MemberRolesModel Role { get; set; }

        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        #endregion External properties

        #region Contructors
        public UserRole(MemberRolesModel role)
        {
            Role = role;
        }
        #endregion Constructors
    }
}
