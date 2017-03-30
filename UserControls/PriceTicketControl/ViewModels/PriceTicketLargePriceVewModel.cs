using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserControls.PriceTicketControl.ViewModels
{
    public class PriceTicketLargePriceVewModel:PriceTicketViewModelBase
    {
        public PriceTicketLargePriceVewModel(string code, string barcode, string description, decimal? price, decimal? oldPrice) : base(code, barcode, description, price, oldPrice)
        {

        }
    }
}
