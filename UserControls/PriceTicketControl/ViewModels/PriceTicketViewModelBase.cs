using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ES.Common.ViewModels.Base;

namespace UserControls.PriceTicketControl.ViewModels
{
    public class PriceTicketViewModelBase : ViewModelBase
    {
        #region Internal properties

        #endregion Internal properties

        #region External properties

        #region Title
        private string _title = "Price ticket";
        public string Title
        {
            get { return _title; }
            set
            {
                if (value.Equals(_title)) return;
                _title = value;
                RaisePropertyChanged("Title");
            }
        }
        #endregion Title

        #region Price
        private decimal? _price;
        private decimal? _oldPrice;

        public double? OldPrice
        {
            get { return (double?)_oldPrice; }
            set { _oldPrice = (decimal?)value; RaisePropertyChanged("OldPrice"); }
        }

        public double Price
        {
            get { return (double)(_price ?? 0); }
            set { _price = (decimal)value; RaisePropertyChanged("Price"); }
        }
        #endregion

        #region Barcode
        private string _barcode = "123456789";
        public string Barcode
        {
            get { return _barcode; }
            set
            {
                if (value == _barcode) return;
                _barcode = value;
                RaisePropertyChanged("Barcode");
            }
        }
        #endregion Barcode

        #region Code
        private string _code;
        public string Code
        {
            get { return _code; }
            set
            {
                if (value == _code) return;
                _code = value;
                RaisePropertyChanged("Code");
            }
        }
        #endregion Code

        #region Descritpion
        private string _description;

        public string Description
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

        #endregion External properties

        #region Contructors
        public PriceTicketViewModelBase(string code, string barcode, string description, decimal? price, decimal? oldPrice)
        {
            _code = code;
            _barcode = barcode;
            _description = description;
            _price = price;
            _oldPrice = oldPrice;
        }
        #endregion Contructors
    }
}
