using System;
using System.ComponentModel;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Data.Models.EsModels;
using UserControls.Enumerations;

namespace UserControls.Interfaces
{
    public interface IShellViewModel: INotifyPropertyChanged
    {
        void OnGetInvoices(Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount> o);
        void OnGetReport(ReportTypes type);
        void OnTools(ToolsEnum toolsEnum);
        void OnSetCategory(EsCategoriesModel category);
        void OnOpenCalc(object obj);
        bool CanExecuteAccountingAction(AccountingPlanEnum accountingPlan);
        void OnAccountingAction(AccountingPlanEnum accountingPlan);
    }
}