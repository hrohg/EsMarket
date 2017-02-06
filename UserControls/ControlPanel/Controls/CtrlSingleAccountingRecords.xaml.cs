using System;
using System.Windows;
using ES.Business.Models;
using UserControls.ViewModels;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for CtrlAccountingRecords.xaml
    /// </summary>
    public partial class CtrlSingleAccountingRecords : Window
    {
        public bool Result = false;
        public AccountingRecordsModel AccountingRecord { get { return ((AccountingRecordsViewModel) this.DataContext).AccountingRecord; } }
        public CtrlSingleAccountingRecords(AccountingRecordsViewModel accountingRecords, bool debitIsEnable=false, bool creditIdEnable=false )
        {
            InitializeComponent();
            DataContext = accountingRecords;
            CmbDebits.IsEnabled = debitIsEnable;
            CmbCredits.IsEnabled = creditIdEnable;
        }
        public CtrlSingleAccountingRecords(int? debit, int? credit)
        {
            InitializeComponent();
            DataContext = new AccountingRecordsViewModel(debit,credit);
        }
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
