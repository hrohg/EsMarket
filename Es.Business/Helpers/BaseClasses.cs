using System.Drawing;

namespace ES.Business.Helpers
{
    public class BaseClasses
    {

    }
    #region Items for choose
    public class ItemsForChoose
    {
        private bool _isChecked;
        private OnChecked _onCheckedCallback;
        public delegate void OnChecked(ItemsForChoose e);
        public object Value { get; set; }
        public object Data { get; set; }
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked && value)
                {
                    _isChecked = false;
                }
                else
                {
                    _isChecked = value;
                }

                if (_onCheckedCallback != null) _onCheckedCallback(this);
            }
        }

        public ItemsForChoose() { }
        public ItemsForChoose(OnChecked onCheckedCallback)
        {
            _onCheckedCallback = onCheckedCallback;
        }
    }

    #endregion

    #region ItemProperties

    public class ItemProperty
    {
        public object Key { get; set; }
        public object Value { get; set; }
    }
    #endregion    
}
