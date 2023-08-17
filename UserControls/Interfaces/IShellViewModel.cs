using System;
using System.ComponentModel;
using System.Windows.Input;
using AccountingTools.Enums;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Data.Models.EsModels;
using UserControls.Enumerations;

namespace UserControls.Interfaces
{
    public interface IShellViewModel: INotifyPropertyChanged
    {
        bool CanGetInvoices(Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount> tuple);
        void OnGetInvoices(Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount> o);
        void OnGetReport(ReportTypes type);
        void OnTools(ToolsEnum toolsEnum);
        void OnSetCategory(EsCategoriesModel category);
        void OnOpenCalc(object obj);
        bool CanExecuteAccountingAction(AccountingPlanEnum accountingPlan);
        void OnAccountingAction(AccountingPlanEnum accountingPlan);
        bool CanOpenTools(ToolsEnum obj);

        ICommand ManageProductsCommand { get; }
    }
}