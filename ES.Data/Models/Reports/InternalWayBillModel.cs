using System;

namespace ES.Data.Models.Reports
{
    public class InternalWayBillDetilesModel
    {
        #region Internal properties
        #endregion

        #region External properties
        public string Invoice { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string FromStock { get; set; }
        public string ToStock { get; set; }
        public string Direction { get { return string.Format("{0} -> {1}", FromStock, ToStock); } }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal? Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal Amount { get { return (Quantity ?? 0) * (Price ?? 0); } }
        public string Notes { get; set; }
        #endregion

        #region Constructors

        public InternalWayBillDetilesModel()
        {

        }
        #endregion

        #region Internal methods
        #endregion

        #region External methods

        #endregion
    }

    public class InternalWayBillModel
    {
        #region Internal properties
        #endregion

        #region External properties
        public string Invoice { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string FromStock { get; set; }
        public string ToStock { get; set; }
        public string Direction { get { return string.Format("{0} -> {1}", FromStock, ToStock); } }
        public decimal Amount { get; set; } 
        public string Notes { get; set; }
        #endregion

        #region Constructors

        public InternalWayBillModel()
        {

        }
        #endregion

        #region Internal methods
        #endregion

        #region External methods

        #endregion
    }
}
