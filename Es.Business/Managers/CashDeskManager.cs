using System;
using System.Collections.Generic;
using System.Linq;
using AccountingTools.Enums;
using ES.Business.Helpers;
using ES.Common.Managers;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class CashDeskManager : BaseManager
    {
        #region CashDesk

        #region Public methods

        public static CashDesk GetCashDesk(Guid id)
        {
            return TryGetCashDesks(new List<Guid> { id }).SingleOrDefault();
        }
        public static List<CashDesk> GetCashDesks(List<Guid> ids)
        {
            return TryGetCashDesks(ids);
        }
        public static List<CashDesk> GetCashDesks(bool? isCash)
        {
            return TryGetCashDesks(isCash);
        }

        public static List<CashDesk> GetCashDesks()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.CashDesk.Where(s => s.MemberId == ApplicationManager.Member.Id && s.IsActive).ToList();
                }
                catch (Exception)
                {
                    return new List<CashDesk>();
                }
            }
        }
        public static List<CashDesk> GetAllCashDesks()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.CashDesk.Where(s => s.MemberId == ApplicationManager.Member.Id).ToList();
                }
                catch (Exception)
                {
                    return new List<CashDesk>();
                }
            }
        }

        public static Dictionary<Guid?, decimal> GetDebitByPartners(DateTime startDate)
        {

            return TryGetDebitByPartners(startDate);
        }
        #endregion

        #region Private methods
        private static List<CashDesk> TryGetCashDesks(List<Guid> ids)
        {
            var memberId = ApplicationManager.Instance.GetMember.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.CashDesk.Where(s => ids.Contains(s.Id) && s.MemberId == memberId).ToList();
                }
                catch (Exception)
                {
                    return new List<CashDesk>();
                }
            }
        }

        private static List<CashDesk> TryGetCashDesks(bool? isCash)
        {
            var memberId = ApplicationManager.Instance.GetMember.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.CashDesk.Where(s => s.MemberId == memberId && s.IsActive && (isCash == null || s.IsCash == isCash)).ToList();
                }
                catch (Exception)
                {
                    return new List<CashDesk>();
                }
            }
        }

        private static Dictionary<Guid?, decimal> TryGetDebitByPartners(DateTime startDate)
        {

            var memberId = ApplicationManager.Instance.GetMember.Id;
            using (var db = GetDataContext())
            {
                try
                {

                    var reg = db.AccountingRecords.Where(s => s.MemberId == memberId && s.RegisterDate >= startDate && (s.Credit == (int)AccountingPlanEnum.AccountingReceivable || s.Debit == (int)AccountingPlanEnum.AccountingReceivable)).ToList();

                    Dictionary<Guid?, decimal> data = reg
                        .GroupBy(s => s.Credit == (int)AccountingPlanEnum.AccountingReceivable ? s.CreditGuidId : s.DebitGuidId)
                        .ToDictionary(x => x.Key, x => x.Sum(s => (s.Credit == (int)AccountingPlanEnum.AccountingReceivable ? -1 : 1) * s.Amount));

                    //foreach (var accountingRecordse in reg)
                    //{
                    //    //var key = accountingRecordse.Credit == (int)AccountingPlanEnum.AccountingReceivable ? accountingRecordse.CreditGuidId : accountingRecordse.DebitGuidId;
                    //    //if (key == null) continue;
                    //    //if (!data.ContainsKey((Guid)key)) data.Add((Guid)key, 0);
                    //    data[(Guid)key] +=;
                    //}

                    return data;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        #endregion

        #endregion

        public static bool ManageCashDesk(CashDesk item)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var exItem = db.CashDesk.SingleOrDefault(s => s.Id == item.Id);
                    if (exItem == null)
                    {
                        db.CashDesk.Add(item);
                    }
                    else
                    {
                        exItem.Description = item.Description;
                        exItem.IsActive = item.IsActive;
                        exItem.IsCash = item.IsCash;
                        exItem.Name = item.Name;
                        exItem.Notes = item.Notes;
                    }
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage("Գործողության ընդհատում");
                    MessageManager.OnMessage(ex.ToString());
                    return false;
                }
            }
        }
    }

}
