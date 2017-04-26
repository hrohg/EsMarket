namespace UserControls.PriceTicketControl.ViewModels
{
    public class BarcodeViewModel
    {
        #region Properties
        #endregion

        #region Private properties

        private string _code;
        private string _barcode="123456789";
        private string _description;
        private decimal? _price;
        private decimal? _oldPrice;
        #endregion

        #region Public properties
        public string Code { get { return _code; } }
        public string OldPrice { get { return "Նախկին գին։ " + (_oldPrice??0) + " դր․"; } }
        public string Price { get { return "Գին։" + (_price??0) + "դր․"; } }
        public string BarcodeString { get { return _barcode; } }
        public string Description { get { return _description; } }
        #endregion

        public BarcodeViewModel(string code, string barcode, string description, decimal? price, decimal? oldPrice)
        {
            _code = code;
            _barcode = barcode;
            _description = description;
            _price = price;
            _oldPrice = oldPrice;
        }
    }
}
