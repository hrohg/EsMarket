using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using UserControls.Commands;

namespace UserControls.ViewModels
{
    public class ServicesViewModel : INotifyPropertyChanged
    {
        #region Properties
        private const string ServiceProperty = "Service";
        private const string FilterTextProperty = "FilterText";
        #endregion
        #region Private properties
        private ServicesModel _service;
        private ObservableCollection<ServicesModel> _services= new ObservableCollection<ServicesModel>();
        private string _filterText;
        #endregion
        #region Public properties
        public ServicesModel Service { get { return _service; } set { _service = value; OnPropertyChanged(ServiceProperty); } }
        public ObservableCollection<ServicesModel> Services { get { return new ObservableCollection<ServicesModel>(_services.Where(s => (s.Code + s.Description + s.Price).Contains(FilterText))); } set { _services = value; } }
        public ServicesModel SelectedService { get; set; }
        public string FilterText { get { return _filterText; } set { _filterText = value; OnPropertyChanged(FilterTextProperty); } }
        public string EditButtonContent { get { return Services.SingleOrDefault(s => s.Id == Service.Id) == null ? "Ավելացնել" : "Փոփոխել"; } }
        #endregion
        public ServicesViewModel()
        {
            Service = new ServicesModel(ApplicationManager.Member.Id, true);
            SetCommands();
        }
        #region Private methods

        private void SetCommands()
        {
            NewServiceCommad = new NewServicesCommand(this);
            EditServiceCommand = new EditServicesCommand(this);
            RemoveServiceCommand = new RemoveServicesCommand(this);
        }
        #endregion
        #region Public methods

        public bool CanCreateNewService()
        {
            return Services.SingleOrDefault(s => s.Id == Service.Id) == null;
        }
        public void CreateNewService()
        {
            Service=new ServicesModel(ApplicationManager.Member.Id, true);
        }

        public bool CanEditService()
        {
            return !string.IsNullOrEmpty(Service.Code) && !string.IsNullOrEmpty(Service.Description);
        }

        public void EditService()
        {
            
        }

        public bool CanRemoveService()
        {
            return SelectedService != null;
        }

        public void RemoveService()
        {
            
        }
        #endregion
        #region Commands
        public ICommand NewServiceCommad { get; private set; }
        public ICommand EditServiceCommand { get; private set; }
        public ICommand RemoveServiceCommand { get; private set; }
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
