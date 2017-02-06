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
        public AccountingRecordsModel AccountingRecord { get { return ((AccountingRecordsViewModel) this.DataContext).AccountingRecord; } }
        public CtrlAccountingRecords(AccountingRecordsViewModel accountingRecords, bool debitIsEnable=false, bool creditIdEnable=false )
        {
            InitializeComponent();
            DataContext = accountingRecords;
            CmbDebit.IsEnabled = debitIsEnable;
            CmbCredit.IsEnabled = creditIdEnable;
        }
        public CtrlAccountingRecords(long memberId, long registerId)
        {
            InitializeComponent();
            DataContext = new AccountingRecordsViewModel();
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
