using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using ES.Common.Helpers;

namespace ES.Common.ViewModels.Base
{
    public class DocumentViewModelBase : PaneViewModel
    {
        #region Internal fields
        private string _description;
        #endregion Internal fields

        #region External fields
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
                RaisePropertyChanged(() => Description);
            }
        }
        #endregion External fields

        public DocumentViewModelBase()
        {
            IsClosable = true;
            CanFloat = true;
        }
    }
    
    public class DocumentViewModel : DocumentViewModelBase
    {
        #region Internal properties
        private bool _isPrintPreviewMode;
        #endregion Internal properties

        #region External properties  



        [XmlIgnore]
        public virtual int TotalRows { get; private set; }

        [XmlIgnore]
        public virtual double TotalCount { get; set; }

        [XmlIgnore]
        public virtual double Total { get; set; }

        [XmlIgnore]
        public virtual double TotalAmount { get; set; }

        [XmlIgnore]
        public bool IsPrintPreviewMode { get { return _isPrintPreviewMode; } set { _isPrintPreviewMode = value; RaisePropertyChanged(() => IsPrintPreviewMode); } }
        #endregion External properties

        #region Constructors

        public DocumentViewModel()
        {
            IsClosable = true;
            CanFloat = true;
        }
        #endregion Constructors

        #region Events

        #region Active tab changing      


        public virtual void SetExternalText(ExternalTextImputEventArgs e)
        {
            if (!e.Handled) SendTextToFocusedElement(e.Text);
        }
        #endregion Active tab changing

        #endregion Events

        #region Internal methods
        protected void SendTextToFocusedElement(string text, bool useReturnKey = true)
        {
            var target = Keyboard.FocusedElement;
            var routedEvent = TextCompositionManager.TextInputEvent;

            target.RaiseEvent(
              new TextCompositionEventArgs(
                InputManager.Current.PrimaryKeyboardDevice,
                new TextComposition(InputManager.Current, target, text))
              { RoutedEvent = routedEvent }
            );
            if (useReturnKey)
            {
                KeyEventArgs ke = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 50, Key.Enter)
                {
                    RoutedEvent = UIElement.KeyDownEvent,
                    Handled = true
                };

                InputManager.Current.ProcessInput(ke);
            }

        }
        #endregion Internal methods

        [XmlIgnore]
        public override bool IsActive
        {
            get { return base.IsActive; }
            set
            {
                if (IsActive != value)
                {
                    base.IsActive = value;
                    if (IsActive) OnActiveTabChanges(this);
                }
            }
        }
        public delegate void ActiveTabChanged(DocumentViewModel document, ActivityChangedEventArgs e);
        public event ActiveTabChanged ActiveTabChangedEvent;
        private void OnActiveTabChanges(DocumentViewModel document)
        {
            if (ActiveTabChangedEvent != null) ActiveTabChangedEvent(document, new ActivityChangedEventArgs(IsActive));
        }
    }
}
