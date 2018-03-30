using System.Xml.Serialization;
using ES.Common.Helpers;

namespace ES.Common.ViewModels.Base
{
    public class DocumentViewModel : PaneViewModel
    {
        #region Internal properties

        #endregion Internal properties

        #region External properties

        #region Description

        private string _description;
        [XmlIgnore]
        public virtual string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value == _description) return;
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        #endregion Description

        #region IsActive
        [XmlIgnore]
        public override bool IsActive
        {
           set
            {
                if (IsActive != value)
                {
                    base.IsActive = value;
                    if (IsActive) OnActiveTabChanges(this);
                }
            }
        }

        #endregion
        [XmlIgnore]

        public virtual int TotalRows { get; set; }
        [XmlIgnore]

        public virtual double TotalCount { get; set; }
        [XmlIgnore]

        public virtual double Total { get; set; }
        [XmlIgnore]

        public virtual double TotalAmount { get; set; }

        #endregion External properties

        #region Constructors

        public DocumentViewModel()
        {
            IsClosable = true;
        }
        #endregion Constructors

        #region Events

        #region Active tab changing

        public delegate void ActiveTabChanged(DocumentViewModel document, ActivityChangedEventArgs e);
        public event ActiveTabChanged ActiveTabChangedEvent;
        private void OnActiveTabChanges(DocumentViewModel document)
        {
            if (ActiveTabChangedEvent != null) ActiveTabChangedEvent(document, new ActivityChangedEventArgs(IsActive));
        }
        #endregion Active tab changing

        #endregion Events

        #region Internal methods
        #endregion Internal methods

    }
}
