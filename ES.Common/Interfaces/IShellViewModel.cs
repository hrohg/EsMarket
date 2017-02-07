using System.ComponentModel;
using ES.Common.Enumerations;

namespace ES.Common.Interfaces
{
    public interface IShellViewModel: INotifyPropertyChanged
    {
        void OnGetInvoices(object o);
        void OnGetReport(ReportTypes type);
    }
}