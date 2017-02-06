namespace ES.Business.Models
{
    public class ECRModel
    {
         #region Private properties
        private string _ecrServerIp = "217.113.7.68";
        private int _ecrConnectionPort = 9999;
        private string _ecrPk = "HS0FBZZZ";
        private int _cashierId = 3;
        private string _cashierPas = "3";
        #endregion
        #region Public properties
        public string ECRServerIP { get { return _ecrServerIp; } set { _ecrServerIp = value; } }
        public int ECRConnectionPort { get { return _ecrConnectionPort; } set { _ecrConnectionPort = value; } }
        public string ECRPrimaiyKey { get { return _ecrPk; } set { _ecrPk = value; } }
        public int CashierId { get { return _cashierId; } set { _cashierId = value; } }
        public string CashierPas { get { return _cashierPas; } set { _cashierPas = value; } }
        #endregion
        public ECRModel()
        {
            
        }
    }
}
