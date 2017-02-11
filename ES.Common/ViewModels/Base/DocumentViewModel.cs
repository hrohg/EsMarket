namespace ES.Common.ViewModels.Base
{
    public class DocumentViewModel : PaneViewModel
    {
        #region Internal properties

        #endregion Internal properties

        #region External properties
        #region Description

        private string _description;

        public virtual string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if(value==_description) return;
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        #endregion Description
        #endregion External properties

        #region Constructors

        public DocumentViewModel()
        {
            IsClosable = true;
        }
        #endregion Constructors

        #region Internal methods
        #endregion Internal methods

    }
}
