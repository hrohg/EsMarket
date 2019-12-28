using ES.Business.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;

namespace ES.Business.Helpers
{
    public class ApplicationSettingsViewModel : ViewModelBase
    {
        private bool _isEcrActivated;
        private bool _isOfflineMode;
        private bool _isPrintSaleTicket;

        #region Internal fields

        #endregion Internal fields

        #region External properties
        public SettingsContainer SettingsContainer { get; set; }

        public bool IsOfflineMode
        {
            get { return _isOfflineMode; }
            set { _isOfflineMode = value; RaisePropertyChanged("IsOfflineMode"); }
        }
        public bool IsEcrActivated
        {
            get { return _isEcrActivated; }
            set
            {
                _isEcrActivated = value;
                RaisePropertyChanged("IsEcrActivated");
                RaisePropertyChanged("EcrButtonTooltip");
            }
        }
        public string EcrButtonTooltip { get { return IsEcrActivated ? "ՀԴՄ ակտիվ է" : "ՀԴՄ պասիվ է"; } }

        public bool IsPrintSaleTicket
        {
            get { return _isPrintSaleTicket; }
            set
            {
                _isPrintSaleTicket = value;
                RaisePropertyChanged("IsPrintSaleTicket");
            }
        }
        public BranchModel Branch { get; private set; }
        #endregion External properties

        #region Constructors
        public ApplicationSettingsViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            OnReloadSettings();
        }

        private void OnReloadSettings()
        {
            SettingsContainer = new SettingsContainer();
            SettingsContainer.LoadMemberSettings();
            IsEcrActivated = SettingsContainer.MemberSettings.IsEcrActivated;
            IsPrintSaleTicket = SettingsContainer.MemberSettings.IsPrintSaleTicket;
            IsOfflineMode = SettingsContainer.MemberSettings.IsOfflineMode;

            Branch = SettingsContainer.MemberSettings.BranchSettings ?? new BranchModel(ApplicationManager.Member.Id);
        }
        #endregion Internal methods

        #region External methods

        public void LoadMemberSettings()
        {
            SettingsContainer.LoadMemberSettings();
        }
        public void Reload()
        {
            OnReloadSettings();
        }
        #endregion External methods
    }
}
