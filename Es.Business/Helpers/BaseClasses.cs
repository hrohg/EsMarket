namespace ES.Business.Helpers
{
    public class BaseClasses
    {

    }
    #region Items for choose
    public class ItemsForChoose
    {
        #region External properties

        private bool _isChecked;
        #endregion
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
            }
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
