using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ES.Business.Helpers;
using ES.Business.Models;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public enum AccountingPlanEnum
    {
        None = 0,
        //1 Ոչ ընթացիկ ակտիվներ
        //2 Ընթացիկ ակտիվներ
        //Ապրանքներ
        Purchase = 216,
        //Դեբիտորական պարտքեր վաճառքի գծով
        AccountingReceivable = 221,
        //Տրված ընթացիկ կանխավճարներ
        Prepayments = 224,
        //Դրամարկղ
        CashDesk = 251,
        //Հաշվարկային հաշիվ
        Accounts = 252,
        //3 Սեփական կապիտալ
        //Կանոնադրական կապիտալ
        EquityBase = 311,
        //4 Ոչ ընթացիկ պարտավորություններ
        //5 Ընթացիկ պարտավորություններ
        //Կրեդիտորական պերտքեր գնումների գծով
        PurchasePayables = 521,
        //Ստացված կանխավճարներ
        ReceivedInAdvance = 523,
        //
        Debit_For_Salary = 527,
        //6 Եկամուտներ
        //Հասույթ
        Proceeds = 611,
        //7 Ծախսեր
        //711 Իրացված արտադրանքի, ապրանքների, աշխատանքների, ծառայությունների ինքնարժեք
        CostPrice = 711,
        //Իրացման ծախսեր
        CostOfSales = 712,
        OtherOperationalExpenses = 714,
        //8 Կառավարչական հաշվառման հաշիվներ
        //9 Արտահաշվեկշռային հաշիվներ

    }
    public class AccountingRecordsManager : BaseManager
    {
        #region Converters
        private static AccountingRecordsModel Convert(AccountingRecords item)
        {
            if (item == null) return null;
            return new AccountingRecordsModel(memberId: item.MemberId, registerId: item.RegisterId, date: item.RegisterDate)
            {
                Id = item.Id,
                Amount = item.Amount,
                Description = item.Description,
                Debit = item.Debit,
                Credit = item.Credit,
                DebitGuidId = item.DebitGuidId,
                CreditGuidId = item.CreditGuidId,
                DebitLongId = item.DebitLongId,
                CreditLongId = item.CreditLongId
            };
        }
        private static AccountingRecords Convert(AccountingRecordsModel item)
        {
            if (item == null) return null;
            return new AccountingRecords
            {
                Id = item.Id, 
                MemberId = item.MemberId,
                RegisterId = item.RegisterId,
                RegisterDate = item.RegisterDate,
                Amount = item.Amount,
                Description = item.Description,
                Debit = item.Debit,
                Credit = item.Credit,
                DebitGuidId = item.DebitGuidId,
                CreditGuidId = item.CreditGuidId,
                DebitLongId = item.DebitLongId,
                CreditLongId = item.CreditLongId
            };
        }
        #endregion

        #region public properties
        public static string GetAccountingRecordsDescription(long key)
        {
            switch (key)
            {
                //1

                //2 Ընթացիկ ակտիվներ
                case (long)AccountingPlanEnum.Purchase:
                    return "Ապրանքի ձեռքբերում";
                    break;
                case (long)AccountingPlanEnum.AccountingReceivable:
                    return "Դեբիտորական պարտքեր վաճառքի գծով";
                    break;
                case (long)AccountingPlanEnum.Prepayments:
                    return "Տրված ընթացիկ կանխավճարներ";
                    break;
                case (long)AccountingPlanEnum.CashDesk:
                    return "Դրամարկղ";
                    break;
                case (long)AccountingPlanEnum.Accounts:
                    return "Հաշվարկային հաշիվ";
                    break;
                //3 Սեփական կապիտալ
                case (long)AccountingPlanEnum.EquityBase:
                    return "Կանոնադրական կապիտալ";
                    break;
                //5 Ընթացիկ պարտավորություններ
                case (long)AccountingPlanEnum.PurchasePayables:
                    return "Կրեդիտորական պերտքեր գնումների գծով";
                    break;
                case (long)AccountingPlanEnum.ReceivedInAdvance:
                    return "Ստացված կանխավճարներ";
                    break;
                case (long)AccountingPlanEnum.Proceeds:
                    return "Հասույթ";
                    break;
                //7
                //711
                case (long)AccountingPlanEnum.CostPrice:
                    return "Իրացված արտադրանքի, ապրանքների, աշխատանքների, ծառայությունների ինքնարժեք";
                    break;
                //712
                case (long)AccountingPlanEnum.CostOfSales:
                    return "Իրացման ծախսեր";
                    break;
                //Unknown
                default:
                    return "Անհայտ գործարք";
                    break;
            }
        }
        public static string GetAccountingRecordsDescription(long key, Guid? guidId, long? longId )
        {
            switch (key)
            {
                //1

                //2 Ընթացիկ ակտիվներ
                case (long)AccountingPlanEnum.Purchase:
                    return "Ապրանքի ձեռքբերում";
                    break;
                case (long)AccountingPlanEnum.AccountingReceivable:
                    return "Դեբիտորական պարտքեր վաճառքի գծով";
                    break;
                case (long)AccountingPlanEnum.Prepayments:
                    return "Տրված ընթացիկ կանխավճարներ";
                    break;
                case (long)AccountingPlanEnum.CashDesk:
                    if (guidId == null) return "Անհայտ դրամարկղ";
                    var cashDesk = CashDeskManager.GetCashDesk(guidId.Value);
                    return cashDesk!=null? cashDesk.Name:"Անհայտ դրամարկղ";
                    break;
                case (long)AccountingPlanEnum.Accounts:
                    return "Հաշվարկային հաշիվ";
                    break;
                //3 Սեփական կապիտալ
                case (long)AccountingPlanEnum.EquityBase:
                    return "Կանոնադրական կապիտալ";
                    break;
                //5 Ընթացիկ պարտավորություններ
                case (long)AccountingPlanEnum.PurchasePayables:
                    return "Կրեդիտորական պերտքեր գնումների գծով";
                    break;
                case (long)AccountingPlanEnum.ReceivedInAdvance:
                    return "Ստացված կանխավճարներ";
                    break;
                case (long)AccountingPlanEnum.Proceeds:
                    return "Հասույթ";
                    break;
                //7
                //711
                case (long)AccountingPlanEnum.CostPrice:
                    return "Իրացված արտադրանքի, ապրանքների, աշխատանքների, ծառայությունների ինքնարժեք";
                    break;
                //712
                case (long)AccountingPlanEnum.CostOfSales:
                    return "Իրացման ծախսեր";
                    break;
                //Unknown
                default:
                    return "Անհայտ գործարք";
                    break;
            }
        }
        public static bool AddAccountingRecords(AccountingRecordsModel accountingRecords)
        {
            return TryAddAccountingRecords(Convert(accountingRecords));
        }
        public static bool SetPartnerPayment(AccountingRecordsModel depositeAccountRecords, AccountingRecordsModel repaymentAccountingRecords)
        {
            return TrySetPartnerPayment(depositeAccountRecords: Convert(depositeAccountRecords),
                repaymentAccountingRecords: Convert(repaymentAccountingRecords));
        }

        public static List<AccountingRecordsModel> GetAccountingRecords(DateTime beginDate, DateTime endDate)
        {
            return TryGetAccountingRecords(beginDate, endDate).Select(Convert).ToList();
        }
        public static List<AccountingRecordsModel> GetAccountingRecords(DateTime beginDate, DateTime endDate, List<int> plans)
        {
            return TryGetAccountingRecords(beginDate, endDate, plans).Select(Convert).ToList();
        }
        public static List<AccountingRecordsModel> GetAccountingRecords(DateTime beginDate, DateTime endDate, long debit, long credit)
        {
            return TryGetAccountingRecords(beginDate, endDate, debit, credit).Select(Convert).ToList();
        }
        public static List<ES.DataAccess.Models.AccountingPlan> GetAccountingPlan()
        {
            return TryGetAccountingPlan();
        }
        public static List<int> GetDebits()
        {
            return TryGetDebits();
        }
        public static List<int> GetDebits(int creditId)
        {
            return TryGetDebits(creditId);
        }
        public static List<int> GetCredits()
        {
            return TryGetCredits();
        }
        public static List<int> GetCredits(int debitId)
        {
            return TryGetCredits(debitId);
        }
        public static bool SetRepaymentOfDebts(AccountingRecordsModel accountingRecords, long memberId)
        {
            return TrySetRepaymentOfDebts(Convert(accountingRecords), memberId);
        }
        public static bool SetCashTransfer(AccountingRecordsModel accountingRecords, long memberId)
        {
            return TrySetCashTransfere(Convert(accountingRecords), memberId);
        }
        public static bool SetEquityBase(AccountingRecordsModel accountingRecords, long memberId)
        {
            return TrySetEquityBase(Convert(accountingRecords), memberId);
        }
        #endregion

        #region private properties
        private static bool TryAddAccountingRecords(AccountingRecords item)
        {
            if (item == null || item.Amount == 0 || item.Debit == 0 || item.Credit == 0) return false;
            using (var db = GetDataContext())
            {
                try
                {
                    db.AccountingRecords.Add(item);
                    db.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        private static bool TrySetRepaymentOfDebts(AccountingRecords accountingRecords, long memberId)
        {
            using (var scope = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    try
                    {
                        var fromCashdesk = db.CashDesk.Single(s => s.Id == accountingRecords.CreditGuidId && s.MemberId == memberId);
                        var partner = db.Partners.Single(s => s.Id == accountingRecords.DebitGuidId && s.EsMemberId == memberId);
                        if (partner.Credit == null) { partner.Credit=0;}
                        fromCashdesk.Total -= accountingRecords.Amount;
                        partner.Credit -= accountingRecords.Amount;
                        db.AccountingRecords.Add(accountingRecords);
                        db.SaveChanges();
                        scope.Complete();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static bool TrySetCashTransfere(AccountingRecords accountingRecords, long memberId)
        {
            using (var scope = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    try
                    {
                        var fromCashdesk = db.CashDesk.Single(s => s.Id == accountingRecords.CreditGuidId && s.MemberId == memberId);
                        var toCashdesk = db.CashDesk.Single(s => s.Id == accountingRecords.DebitGuidId && s.MemberId == memberId);
                        fromCashdesk.Total -= accountingRecords.Amount;
                        toCashdesk.Total += accountingRecords.Amount;
                        db.AccountingRecords.Add(accountingRecords);
                        db.SaveChanges();
                        scope.Complete();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }


                }
            }
            return true;
        }
        private static bool TrySetEquityBase(AccountingRecords accountingRecords, long memberId)
        {
            using (var scope = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    try
                    {
                        var toCashdesk = db.CashDesk.Single(s => s.Id == accountingRecords.DebitGuidId && s.MemberId == memberId);
                        toCashdesk.Total += accountingRecords.Amount;
                        db.AccountingRecords.Add(accountingRecords);
                        db.SaveChanges();
                        scope.Complete();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }


                }
            }
            return true;
        }

        // needs review
        private static bool TrySetPartnerPayment(AccountingRecords depositeAccountRecords, AccountingRecords repaymentAccountingRecords)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var transaction = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    var partnerId = depositeAccountRecords != null? depositeAccountRecords.CreditGuidId: repaymentAccountingRecords != null ? repaymentAccountingRecords.CreditGuidId : null;
                    var cashBoxId = depositeAccountRecords != null? depositeAccountRecords.DebitGuidId: repaymentAccountingRecords != null ? repaymentAccountingRecords.DebitGuidId : null;
                    var exPartner = db.Partners.SingleOrDefault(s => s.EsMemberId == memberId && s.Id == partnerId);
                    var exCashBox = db.CashDesk.SingleOrDefault(s => s.MemberId == memberId && s.Id == cashBoxId);
                    if (exPartner == null || exCashBox==null) return false;
                    if (exPartner.Debit == null) {exPartner.Debit = 0;}
                    if (exPartner.Credit == null) {exPartner.Credit = 0;}
                    
                    if (repaymentAccountingRecords != null)
                    {
                        var accountingReceivable = db.AccountsReceivable
                            .Where(s => s.MemberId == memberId && s.PartnerId == repaymentAccountingRecords.CreditGuidId && s.Amount != s.PaidAmount)
                            .OrderBy(s => s.ExpairyDate)
                            .ToList();

                        var paid = repaymentAccountingRecords.Amount;
                        if (exPartner.Debit < paid) return false;
                        exCashBox.Total += paid;
                        exPartner.Debit -= paid;
                        foreach (var item in accountingReceivable.Where(item => item != null))
                        {
                            if (item.PaidAmount == null) item.PaidAmount = 0;
                            var value = ((item.Amount - item.PaidAmount) > paid)? paid: item.Amount - (decimal)item.PaidAmount;

                            item.PaidAmount += value;
                            paid -= value;
                            if (paid == 0) break;
                        }
                        db.AccountingRecords.Add(repaymentAccountingRecords);
                    }
                    if (depositeAccountRecords != null && depositeAccountRecords.Amount > 0)
                    {
                        exPartner.Credit += depositeAccountRecords.Amount;
                        db.AccountingRecords.Add(depositeAccountRecords);
                        exCashBox.Total += depositeAccountRecords.Amount;
                    }
                    db.SaveChanges();
                    transaction.Complete();
                }
            }
            return true;
        }
        private static List<AccountingRecords> TryGetAccountingRecords(DateTime beginDate, DateTime endDate)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingRecords.Where(s =>
                        s.MemberId == memberId
                        && s.RegisterDate >= beginDate.Date
                        && s.RegisterDate < endDate).OrderBy(s => s.RegisterDate).ToList();
                }
                catch (Exception)
                {
                    return new List<AccountingRecords>();
                }
            }
        }
        private static List<AccountingRecords> TryGetAccountingRecords(DateTime beginDate, DateTime endDate, List<int> plans)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingRecords.Where(s =>
                        s.MemberId == memberId
                        && s.RegisterDate >= beginDate.Date
                        && s.RegisterDate < endDate && (plans.Contains((int)s.Debit) || plans.Contains((int)s.Credit))).OrderBy(s => s.RegisterDate).ToList();
                }
                catch (Exception)
                {
                    return new List<AccountingRecords>();
                }
            }
        }
        private static List<AccountingRecords> TryGetAccountingRecords(DateTime beginDate, DateTime endDate, long debit, long credit)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingRecords.Where(s =>
                        s.MemberId == memberId
                        && s.RegisterDate >= beginDate
                        && s.RegisterDate < endDate
                        && s.Debit == debit
                        && s.Credit == credit).ToList();
                }
                catch (Exception)
                {
                    return new List<AccountingRecords>();
                }
            }
        }
        private static List<ES.DataAccess.Models.AccountingPlan> TryGetAccountingPlan()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingPlan.ToList();
                }
                catch (Exception)
                {
                    return new List<ES.DataAccess.Models.AccountingPlan>();
                }
            }
        }
        private static List<int> TryGetDebits()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingPlan.Select(s => s.DebitId).Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<int>();
                }
            }
        }
        private static List<int> TryGetDebits(int creditId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingPlan.Where(s => s.CreditId == creditId).Select(s => s.DebitId).Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<int>();
                }
            }
        }
        private static List<int> TryGetCredits()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingPlan.Select(s => s.CreditId).Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<int>();
                }
            }
        }
        private static List<int> TryGetCredits(int debitId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.AccountingPlan.Where(s => s.DebitId == debitId).Select(s => s.CreditId).Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<int>();
                }
            }
        }
        #endregion
    }

    public class AccountingAccounts
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }

    public class SubAccountingPlanManager:BaseManager
    {
        #region Converters

        private static SubAccountingPlan Convert(SubAccountingPlanModel item)
        {
            return new SubAccountingPlan
            {
                Id = item.Id,
                AccountingPlanId = item.AccountingPlanId,
                SubAccountingPlanId = item.SubAccountingPlanId,
                Name = item.Name,
                Description = item.Description,
                Amount = item.Amount,
                MemberId = item.MemberId,
                IsActive = item.IsActive
            };
        }
        private static SubAccountingPlanModel Convert(SubAccountingPlan item)
        {
            return new SubAccountingPlanModel
            {
                Id = item.Id,
                AccountingPlanId = item.AccountingPlanId,
                SubAccountingPlanId = item.SubAccountingPlanId,
                Name = item.Name,
                Description = item.Description,
                Amount = item.Amount,
                MemberId = item.MemberId,
                IsActive = item.IsActive
            };
        }
        #endregion
        #region Private methods

        private static List<SubAccountingPlan> GetSubAccountingPlans(long memberId, bool? isActive)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.SubAccountingPlan.Where(s => s.MemberId == memberId && (isActive == null || isActive == s.IsActive)).ToList();
                }
                catch (Exception)
                {
                    return new List<SubAccountingPlan>();
                }
            }
        }
        private static List<SubAccountingPlan> GetSubAccountingPlans(long debit, long credit, long memberId, bool? isActive)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.SubAccountingPlan.Where(s => s.MemberId == memberId && (s.AccountingPlanId == debit || s.AccountingPlanId == credit) && (isActive == null || isActive == s.IsActive)).Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<SubAccountingPlan>();
                }
            }
        }
        private static List<SubAccountingPlan> GetSubAccountingPlans(long plan, long memberId, bool? isActive)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.SubAccountingPlan.Where(s => s.MemberId == memberId && (s.AccountingPlanId == plan) && (isActive == null || isActive == s.IsActive)).Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<SubAccountingPlan>();
                }
            }
        }
        #endregion
        #region Public methods

        public static List<SubAccountingPlanModel> GetSubAccountingPlanModels(long memberId, bool? isActive=null)
        {
            return GetSubAccountingPlans(memberId, isActive).Select(Convert).ToList();
        }
        public static List<SubAccountingPlanModel> GetSubAccountingPlanModels(long debit, long credit, long memberId, bool? isActive = null)
        {
            return GetSubAccountingPlans(debit, credit, memberId, isActive).Select(Convert).ToList();
        }
        public static List<SubAccountingPlanModel> GetSubAccountingPlanModels(long plan, long memberId, bool? isActive = null)
        {
            return GetSubAccountingPlans(plan, memberId, isActive).Select(Convert).ToList();
        }
        #endregion
    }
}
