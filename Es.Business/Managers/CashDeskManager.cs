using System;
using System.Collections.Generic;
using System.Linq;
using ES.Business.Helpers;
using ES.Common;
using ES.Common.Helpers;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class CashDeskManager : BaseManager
    {
        #region CashDesk
        #region Public methods

        public static CashDesk GetCashDesk(Guid? id, long memberId)
        {
            return TryGetCashDesk(id, memberId);    
        }
        public static CashDesk GetDefaultSaleCashDesk(long memberId)
        {
            var xml = new XmlManager();
            var cashDesks = xml.GetItemsByControl(XmlTagItems.SaleCashDesks);
            using (var db=GetDataContext())
            {
                try
                {
                    var cashDeskIds = cashDesks.Select(s => s.Value.ToString()).ToList();
                    return db.CashDesk.FirstOrDefault(s =>s.MemberId==memberId && cashDeskIds.Contains(s.Id.ToString()));
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        #endregion
        #region Private methods
        private static CashDesk TryGetCashDesk(Guid? id, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.CashDesk.SingleOrDefault(s => s.Id == id && s.MemberId == memberId);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static IEnumerable<CashDesk> TryGetCashDesks(IEnumerable<Guid> ids, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.CashDesk.Where(s => s.MemberId == memberId && ids.Contains(s.Id)).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static List<CashDesk> GetDefaultCashDesks(long memberId, bool? isCash)
        {
            var xml = new XmlManager();
            var cashDeskIds = xml.GetItemsByControl(XmlTagItems.SaleCashDesks).Select(s => HgConvert.ToGuid(s.Value));
            var cashDesks = TryGetCashDesks(cashDeskIds, memberId);
            if (cashDesks == null) { return new List<CashDesk>();}
            return (isCash == null) ? cashDesks.ToList() : cashDesks.Where(s => s.IsCash == isCash).ToList();
        }
        public static List<CashDesk> TryGetCashDesk(long memberId, bool? isCash=null)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.CashDesk.Where(s => s.MemberId == memberId && s.IsActive && (isCash==null || s.IsCash==isCash)).ToList();
                }
                catch (Exception)
                {
                    return new List<CashDesk>();
                }
            }
        }

        public static List<CashDesk> TryGetCashDesk(bool? isCash, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return isCash != null
                        ? db.CashDesk.Where(s => s.MemberId == memberId && s.IsActive && s.IsCash == isCash).ToList()
                        : db.CashDesk.Where(s => s.MemberId == memberId && s.IsActive).ToList();
                }
                catch (Exception)
                {
                    return new List<CashDesk>();
                }
            }
        }
        
        #endregion
        #endregion
    }

}
