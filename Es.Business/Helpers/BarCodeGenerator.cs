using System;
using System.Windows.Forms;

namespace ES.Business.Helpers
{
    public class BarCodeGenerator
    {
        #region Private properties

        private string _countryCode = "20";
        private string _manufacturerCode = "00000";
        private string _productCode = "10000";
        #endregion
        #region Public properties
        public string CountryCode { get { return _countryCode; } set { _countryCode = value; } }
        public string ManufacturerCode { get { return _manufacturerCode; } set { _manufacturerCode = value; } }
        public string ProductCode { get { return _productCode; } set { _productCode = value; } }
        private string GetChecksum(){return CalculateChecksumDigit();}

        public string Barcode
        {
            get
            {
                var checksum = GetChecksum();
                return string.IsNullOrEmpty(CountryCode) || string.IsNullOrEmpty(ManufacturerCode) ||
                    string.IsNullOrEmpty(ProductCode) || string.IsNullOrEmpty(checksum) ? null : 
                    string.Format("{0}{1}{2}{3}", CountryCode, ManufacturerCode, ProductCode, checksum);
            }
        }

        #endregion
        public BarCodeGenerator(string productCode)
        {
            GenerateBarcode(countryCode:null, manufacturerCode: null, productCode: productCode);
        }
        public BarCodeGenerator(long productCode)
        {
            GenerateBarcode(countryCode: null, manufacturerCode: ((int)productCode/100000).ToString(), productCode: (productCode != 0 ? ((int)(productCode%100000)).ToString() : null));
        }
        #region Private methods

        private void GenerateBarcode(string countryCode, string manufacturerCode, string productCode)
        {
            //Country code
            if (!string.IsNullOrEmpty(countryCode))
            {
                CountryCode = string.Empty;
                foreach (var cc in countryCode)
                {
                    CountryCode += cc;
                    if (CountryCode.Length >= 2)
                    {
                        break;
                    }
                }
            }
            while (CountryCode.Length < 1)
            {
                CountryCode = "0" + CountryCode;
            }
            if (CountryCode.Length < 2) CountryCode = "2" + CountryCode;

            //Manufacture
            if (!string.IsNullOrEmpty(manufacturerCode))
            {
                ManufacturerCode = string.Empty;
                foreach (var mc in manufacturerCode)
                {
                    ManufacturerCode += mc;
                    if (ManufacturerCode.Length >= 5) { break; }
                }
                while (ManufacturerCode.Length < 5)
                {
                    ManufacturerCode = "0" + ManufacturerCode;
                }
            }

            //ProductCode
            if (productCode == null) { return; }
            _productCode = string.Empty;
            var code = productCode;
            foreach (var next in code)
            {
                ProductCode += next;
                if (ProductCode.Length == 5) break;
            }
            while (ProductCode.Length < 4)
            {
                ProductCode = "0" + ProductCode;
            }
            if (ProductCode.Length == 4) ProductCode = 1 + ProductCode;
        }

        private string CalculateChecksumDigit()
        {
            return CalculateEan13ChecksumDigit(CountryCode + ManufacturerCode + ProductCode);
        }

        private static string CalculateEan13ChecksumDigit(string sTemp){
            int iSum = 0;
            int iDigit = 0;

            // Calculate the checksum digit here.
            for (int i = sTemp.Length; i >= 1; i--)
            {
                if (!Int32.TryParse(sTemp.Substring(i - 1, 1), out iDigit))
                {
                    MessageBox.Show("Բարկոդը սխալ է կամ ոչ լիարժեք։", "Բար կոդի սխալ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                 // This appears to be backwards but the 
                // EAN-13 checksum must be calculated
                // this way to be compatible with UPC-A.
                if (i % 2 == 0)
                { // odd  
                    iSum += iDigit * 3;
                }
                else
                { // even
                    iSum += iDigit * 1;
                }
            }
            int iCheckSum = (10 - (iSum % 10)) % 10;
            return iCheckSum.ToString();
        }
        #endregion
        #region Public methods

        #endregion
    }
}
