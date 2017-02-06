using System.ComponentModel;
using System.Windows.Input;

namespace UserControls.Interfaces
{
    public interface ITabItem : INotifyPropertyChanged
    {
        string Title { get; set; }
        string Description { get; set; }
        bool IsModified { get; set; }
        bool IsLoading { get; set; }
        ICommand CloseCommand { get; }
    }
}
