using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CashReg.Helper;
using ES.Business.Helpers;
using ES.Common.Managers;
using ES.Data.Models;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class ReportManager : BaseManager
    {
        public static List<InvoiceModel> GetShortInvoiceReport(DateTime startDate, DateTime endDate)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return
                        db.Invoices.Where(s => s.MemberId == ApplicationManager.Member.Id && s.ApproveDate != null && s.ApproveDate >= startDate && s.ApproveDate <= endDate)
                            .Select(s => new InvoiceModel
                            {
                                Id = s.Id,
                                InvoiceTypeId = (short)s.InvoiceTypeId,
                                CreateDate = s.CreateDate,
                                ApproveDate = s.ApproveDate.Value,
                                InvoiceNumber = s.InvoiceNumber,
                                //CostPrice = s.InvoiceItems.Sum(i => i.Quantity * i.CostPrice) ?? 0,
                                Total = s.Summ,                                
                                Notes = s.Notes,
                                PartnerId = s.PartnerId,
                                ApproverId = s.ApproverId.Value,
                                MemberId = s.MemberId,
                                Approver = s.Approver,
                                Creator = s.Creator
                            }).ToList();
                }
                catch (Exception)
                {
                    return new List<InvoiceModel>();
                }
            }
        }
    }
}
