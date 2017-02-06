using System;
using System.Windows;
using ES.Business.Models;
using UserControls.ViewModels;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for CtrlAccountingRecordsWithSubAccountings.xaml
    /// </summary>
    public partial class CtrlMultipleAccountingRecords : Window
    {
        public CtrlMultipleAccountingRecords()
        {
            InitializeComponent();
        }
        public bool Result = false;
        public AccountingRecordsModel AccountingRecord { get { return ((AccountingRecordsViewModel) this.DataContext).AccountingRecord; } }
        public CtrlMultipleAccountingRecords(AccountingRecordsViewModel accountingRecords, bool debitIsEnable=false, bool creditIdEnable=false )
        {
            InitializeComponent();
            DataContext = accountingRecords;
        }
        public CtrlMultipleAccountingRecords(long memberId, long registerId)
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
