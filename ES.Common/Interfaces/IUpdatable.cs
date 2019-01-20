using System.Windows.Input;

namespace ES.Common.Interfaces
{
    internal interface IUpdatable
    {
        ICommand UpdateCommand { get; }
    }
}