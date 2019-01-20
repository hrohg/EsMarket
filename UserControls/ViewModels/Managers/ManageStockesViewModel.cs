using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;

namespace UserControls.ViewModels.Managers
{
    public class ManageStockesViewModel : DocumentViewModel
    {
        #region Internal properties
        private StockModel _selectedItem;
        private string _filterText;

        #endregion Internal properties

        #region External properties
        public ObservableCollection<StockModel> Items { get; set; }

        public StockModel SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged("SelectedItem"); }
        }

        public string FilterText
        {
            get { return _filterText; }
            set { _filterText = value; }
        }
        #endregion External properties

        #region Contructors
        public ManageStockesViewModel()
        {
            Initialize();
        }

        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            Title = "Պահեստների խմբագրում";
            Update();
        }

        private void Update()
        {
            Items = new ObservableCollection<StockModel>(StockManager.GetAllStocks() ?? new List<StockModel>());
            SelectedItem = new StockModel(ApplicationManager.Member.Id);
        }

        #endregion Internal methods

        #region Commands

        private ICommand _newCommand;
        public ICommand NewCommand { get { return _newCommand ?? (_newCommand = new RelayCommand(OnNew, CanNew)); } }

        private bool CanNew(object obj)
        {
            return Items.Any(s => s.Id == SelectedItem.Id);
        }

        private void OnNew(object obj)
        {
            SelectedItem = new StockModel(ApplicationManager.Member.Id);
        }

        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(OnAdd, CanAdd)); } }

        private bool CanAdd(object obj)
        {
            return Items.All(s => s.Id != SelectedItem.Id);
        }

        private void OnAdd(object obj)
        {
            if (!StockManager.ManageStock(SelectedItem)) return;
            Update();
            MessageManager.OnMessage("Պահեստի ավելացումն իրականացել է հաջողությամբ:");
        }

        private ICommand _editCommand;
        public ICommand EditCommand { get { return _editCommand ?? (_editCommand = new RelayCommand(OnEdit, CanEdit)); } }

        private bool CanEdit(object obj)
        {
            return Items.Any(s => s.Id == SelectedItem.Id);
        }

        private void OnEdit(object obj)
        {
            if (!StockManager.ManageStock(SelectedItem)) return;
            ApplicationManager.CashManager.UpdateStocksAsync();
            Update();
            MessageManager.OnMessage("Պահեստի խմբագրումն իրականացել է հաջողությամբ:");
        }
        #endregion
    }
}
