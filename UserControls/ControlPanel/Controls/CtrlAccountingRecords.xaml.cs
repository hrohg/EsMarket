using System;
using System.Windows;
using ES.Business.Models;
using UserControls.ViewModels;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for CtrlAccountingRecords.xaml
    /// </summary>
    public partial class CtrlAccountingRecords : Window
    {
        public bool Result = false;
        public AccountingRecordsModel AccountingRecord { get { return ((AccountingRecordsViewModel)this.DataContext).AccountingRecord; } }

        #region Constructors
        public CtrlAccountingRecords(AccountingRecordsViewModel accountingRecords, bool debitIsEnable = false, bool creditIdEnable = false)
        {
            InitializeComponent();
            DataContext = accountingRecords;
            
            //Create bindable properties
            //CmbDebit.IsEnabled = debitIsEnable;
            //CmbCredit.IsEnabled = creditIdEnable;
        }        
        public CtrlAccountingRecords()
        {
            InitializeComponent();
            DataContext = new AccountingRecordsViewModelBase();
        }
        #endregion Constructors

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Result = false;
            Close();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {

            Result = true;
            Close();
        }
    }
}
