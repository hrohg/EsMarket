using ES.Business.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES.Business.Helpers
{
    public class EnumHelper
    {
        public static string GetInvoiceTypeNames(InvoiceType invoiceType)
        {
            string invoiceName = null;
            switch (invoiceType)
            {
                case InvoiceType.PurchaseInvoice:
                    invoiceName = "Գնում";
                    break;
                case InvoiceType.SaleInvoice:
                    invoiceName = "Վաճառք";
                    break;
                case InvoiceType.ProductOrder:
                    break;
                case InvoiceType.MoveInvoice:
                    invoiceName = "Տեղափոխում";
                    break;
                case InvoiceType.InventoryWriteOff:
                    invoiceName = "Դուրսգրում";
                    break;
                case InvoiceType.ReturnFrom:
                    invoiceName = "Վերադարձ";
                    break;
                case InvoiceType.ReturnTo:
                    invoiceName = "Վերադարձ մատակարարին";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("invoiceType", invoiceType, null);
            }
            return invoiceName;
        }
    }
}
