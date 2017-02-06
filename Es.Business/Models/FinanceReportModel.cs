using System;
using System.Collections.Generic;
using ES.Data.Models;

namespace ES.Business.Models
{
    public class FinanceReportModel
    {
        #region Internal properties
        #endregion

        #region External properties
        public List<InvoiceItemsModel> InvocieItems { get; set; }
        public List<ProductResidue> ProductResidues { get; set; }

        
        #endregion

        #region Constructors

        public FinanceReportModel()
        {
            InvocieItems = new List<InvoiceItemsModel>();
            ProductResidues = new List<ProductResidue>();
        }
        #endregion

        #region Internal methods
        #endregion

        #region External methods
        #endregion
    }

    public class ProductResidue
        {
            public Guid ProductId { get; set; }
            public decimal Quantity { get; set; }
        }
}
