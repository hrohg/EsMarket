using System.Collections.ObjectModel;
using ES.Business.Models;
using ES.Common.ViewModels.Base;
using Xceed.Wpf.AvalonDock.Layout;

namespace UserControls.ViewModels.Logs
{
    public class LogViewModel : ToolsViewModel
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        public ObservableCollection<MessageModel> Logs { get; set; } 
        #endregion External properties

        #region Constructors

        public LogViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Ստացված հաղորդագրություններ";
            CanFloat = true;
            IsClosable = false;
            AnchorSide = AnchorSide.Bottom;
            Logs = new ObservableCollection<MessageModel>();
        }
        #endregion Internal methods

        #region External methods

        public void AddLog(MessageModel log)
        {
            Logs.Add(log);
        }
        #endregion External methods

        #region Commands
        #endregion Commands
    }
}
