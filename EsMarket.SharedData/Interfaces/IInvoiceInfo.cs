using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EsMarket.SharedData.Enums;

namespace EsMarket.SharedData.Interfaces
{
    public interface IInvoiceInfo
    {
        InvoiceTypeEnum Type { get; set; }
        string InvoiceNumber { get; set; }
    }
}
