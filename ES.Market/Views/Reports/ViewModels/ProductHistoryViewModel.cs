using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Data.Models;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;
using UserControls.Interfaces;

namespace ES.Shop.Views.Reports.ViewModels
{
    public class ProductHistoryViewModel : ITabItem
    {
        #region Internal properties

        private ObservableCollection<InvoiceItemsModel> _items;
        #endregion
        #region External properties
        public string Title { get; set; }
        public bool IsLoading
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public bool IsModified { get; set; }
        public ObservableCollection<InvoiceItemsModel> InvoiceItems { get { return _items; } set { _items = value; OnPropertyChanged("InvoiceItems"); } }
        public InvoiceItemsModel SelectedItem { get; set; }
        #endregion
        public ProductHistoryViewModel()
        {
            Title = "Ապրանքաշրջանառություն";
            var date = SelectManager.GetDateIntermediate();
            var products = UserControls.Helpers.SelectItemsManager.SelectProductByCheck(ApplicationManager.Instance.GetEsMember.Id, true);
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetProductHistory(products.Select(s => s.Id).ToList(), date.Item1, date.Item2, ApplicationManager.Instance.GetEsMember.Id));
        }
        #region Internal methods

        private void OnClose(object o)
        {
            ApplicationManager.OnTabItemClose(o as TabItem);
        }
        #endregion
        #region Commands
        public ICommand CloseCommand { get { return new RelayCommand(OnClose); } }
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
