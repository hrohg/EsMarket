using System.ComponentModel;
using ES.Common.Enumerations;
using ES.Data.Models.EsModels;
using UserControls.Enumerations;

namespace UserControls.Interfaces
{
    public interface IShellViewModel: INotifyPropertyChanged
    {
        void OnGetInvoices(object o);
        void OnGetReport(ReportTypes type);
        void OnTools(ToolsEnum toolsEnum);
        void OnSetCategory(EsCategoriesModel category);
        void OnOpenCalc(object obj);
    }
}