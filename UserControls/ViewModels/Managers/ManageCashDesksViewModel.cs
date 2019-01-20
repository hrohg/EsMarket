using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.ViewModels.Base;
using ES.DataAccess.Models;

namespace UserControls.ViewModels.Managers
{
    public class ManageCashDesksViewModel : DocumentViewModel
    {
        #region Internal properties
        private CashDesk _selectedItem;
        private string _filterText;

        #endregion Internal properties

        #region External properties
        public ObservableCollection<CashDesk> Items { get; set; }

        public CashDesk SelectedItem
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
        public ManageCashDesksViewModel()
        {
            Initialize();
        }

        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            Title = "Դրամարկղերի խմբագրում";
            Update();
        }

        private void Update()
        {
            Items = new ObservableCollection<CashDesk>(CashDeskManager.GetAllCashDesks() ?? new List<CashDesk>());
            SelectedItem = new CashDesk { MemberId = ApplicationManager.Member.Id , Id = Guid.NewGuid()};
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
            SelectedItem = new CashDesk { Id = Guid.NewGuid(), MemberId = ApplicationManager.Member.Id };
        }

        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(OnAdd, CanAdd)); } }

        private bool CanAdd(object obj)
        {
            return Items.All(s => s.Id != SelectedItem.Id);
        }

        private void OnAdd(object obj)
        {
            if (!CashDeskManager.ManageCashDesk(SelectedItem)) return;
            Update();
            MessageManager.OnMessage("Դրամարկղի ավելացումն իրականացել է հաջողությամբ:");
        }

        private ICommand _editCommand;
        public ICommand EditCommand { get { return _editCommand ?? (_editCommand = new RelayCommand(OnEdit, CanEdit)); } }

        private bool CanEdit(object obj)
        {
            return Items.Any(s => s.Id == SelectedItem.Id);
        }

        private void OnEdit(object obj)
        {
            if (!CashDeskManager.ManageCashDesk(SelectedItem)) return;
            Update();
            MessageManager.OnMessage("Դրամարկղի խմբագրումն իրականացել է հաջողությամբ:");
        }
        #endregion
    }
}
