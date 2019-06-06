using System;
using System.Collections.Generic;
using System.Linq;
using ES.Business.Helpers;
using ES.Common;
using ES.Data.Enumerations;
using ES.Data.Models;
using ES.DataAccess.Models;


namespace ES.Business.Managers
{
    public class PartnersManager : BaseManager
    {
        #region Converter
        public static PartnerModel Convert(Partners item)
        {
            if (item == null) return null;
            var partner = new PartnerModel(item.EsMemberId);

            partner.Id = item.Id;
            partner.EsMemberId = item.EsMemberId;
            partner.PartnersTypeId = item.EsPartnersTypeId;
            partner.EsUserId = item.EsUserId;
            partner.ClubSixteenId = item.ClubSixteenId;
            partner.FullName = item.FullName;
            partner.FirstName = item.FirstName;
            partner.LastName = item.LastName;
            partner.Mobile = item.Mobile;
            partner.Email = item.Email;
            partner.Address = item.Address;
            partner.Discount = item.Discount;
            partner.Debit = item.Debit ?? 0;
            partner.Credit = item.Credit ?? 0;
            partner.MaxDebit = item.MaxDebit;
            partner.TIN = item.TIN;
            partner.PasportData = item.PasportData;
            partner.JuridicalAddress = item.JuridicalAddress;
            partner.Bank = item.Bank;
            partner.BankAccount = item.BankAccount;
            partner.Notes = item.Notes;
            return partner;
        }
        public static Partners Convert(PartnerModel item)
        {
            if (item == null) return null;
            return new Partners
            {
                Id = item.Id,
                EsMemberId = item.EsMemberId,
                EsPartnersTypeId = item.PartnersTypeId,
                EsUserId = item.EsUserId,
                ClubSixteenId = item.ClubSixteenId,
                FullName = item.FullName,
                FirstName = item.FirstName,
                LastName = item.LastName,
                Mobile = item.Mobile,
                Email = item.Email,
                Address = item.Address,
                Discount = item.Discount,
                Debit = item.Debit,
                Credit = item.Credit,
                MaxDebit = item.MaxDebit,
                TIN = item.TIN,
                PasportData = item.PasportData,
                JuridicalAddress = item.JuridicalAddress,
                Bank = item.Bank,
                BankAccount = item.BankAccount,
                Notes = item.Notes
            };
        }
        public static EsPartnersTypes Convert(PartnerTypeModel item)
        {
            if (item == null) return null;
            return new EsPartnersTypes
            {
                Id = item.Id,
                Description = item.Description
            };
        }
        public static PartnerTypeModel Convert(long id)
        {
            return new PartnerTypeModel
            {
                Id = id,
                Description = CashManager.PartnersTypes.Where(s => s.Id == id).Select(s => s.Description).FirstOrDefault()
            };
        }
        #endregion

        #region Partners public methods

        public static List<EsPartnersTypes> GetPartnersTypes()
        {
            return TryGetPartnersTypes();
        }
        public static PartnerModel GetPartner(Guid? id)
        {
            return Convert(TryGetPartner(id));
        }
        public static List<PartnerModel> GetPartners()
        {
            return TryGetPartners().Select(Convert).OrderBy(s => s.FullName).ToList();
        }
        public static decimal GetPartnersAmount(bool isDebit)
        {
            return TryGetPartnersAmount(isDebit);
        }
        public static List<PartnerModel> GetPartners(List<Guid> ids)
        {
            return TryGetPartners(ids).Select(Convert).OrderBy(s => s.FullName).ToList();
        }
        public static PartnerModel GetDefaultPartner(PartnerType partnerTypeId)
        {
            return TryGetPartners(partnerTypeId).Select(Convert).FirstOrDefault();
        }
        public static PartnerModel GetDefaultParnerByInvoiceType(InvoiceType type)
        {
            switch (type)
            {
                case InvoiceType.ReturnFrom:
                case InvoiceType.SaleInvoice:
                    return Convert(TryGetPartner(DefaultsManager.GetDefaultValueGuid(DefaultControls.Customer)));
                case InvoiceType.ReturnTo:
                case InvoiceType.PurchaseInvoice:
                    return Convert(TryGetPartner(DefaultsManager.GetDefaultValueGuid(DefaultControls.Provider)));
                case InvoiceType.ProductOrder:
                    break;
                case InvoiceType.MoveInvoice:
                    break;
                case InvoiceType.InventoryWriteOff:
                    break;
                default:
                    return null;
            }
            return null;
        }
        public static List<PartnerModel> GetPartner()
        {
            return TryGetPartners().Select(Convert).OrderBy(s => s.FullName).ToList();
        }
        public static List<PartnerModel> GetPartner(PartnerType partnerType)
        {
            return TryGetPartners(partnerType).Select(Convert).OrderBy(s => s.FullName).ToList();
        }
        public static bool AddPartner(PartnerModel item)
        {
            return TryAddPartners(Convert(item));
        }
        public static bool EditPartner(PartnerModel item)
        {
            return TryEditPartner(Convert(item));
        }

        public static List<AccountsReceivable> GetAccountsReceivable(Guid partnerId)
        {
            using (var db = GetDataContext())
            {
                return db.AccountsReceivable.Where(s => s.PartnerId == partnerId).ToList();
            }
        }

        public static List<PartnerModel> GetPartnersForProducts(List<Guid> productIds)
        {
            return TryGetPartnersForProducts(productIds).Select(Convert).ToList();
        }

        public static string GetControlByPartnerType(PartnerType type)
        {
            string controlKey;
            switch (type)
            {
                case PartnerType.Provider:
                    controlKey = DefaultControls.Provider;
                    break;
                case PartnerType.Dealer:
                    controlKey = DefaultControls.Dealer;
                    break;
                case PartnerType.Customer:
                    controlKey = DefaultControls.Customer;
                    break;
                case PartnerType.Branch:
                    controlKey = DefaultControls.Branch;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("partner", type, null);
            }
            return controlKey;
        }
        public static bool SetDefault(PartnerModel partner, bool isDefault)
        {
            if (partner == null || partner.PartnersTypeId == null) return false;
            string controlKey = GetControlByPartnerType(partner.PartnerTypeEnum);
            return DefaultsManager.SetDefault(controlKey, null, isDefault? partner.Id:(Guid?)null);
        }
        #endregion

        #region Partners private methods
        private static List<EsPartnersTypes> TryGetPartnersTypes()
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.EsPartnersTypes.ToList();
                }
            }
            catch (Exception)
            {
                return new List<EsPartnersTypes>();
            }
        }
        private static Partners TryGetPartner(Guid? id)
        {
            if (id == null) return null;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Partners.SingleOrDefault(s => s.Id == id && s.EsMemberId == ApplicationManager.Member.Id);
                }
                catch (Exception)
                {
                    return null;
                }

            }
        }
        private static List<Partners> TryGetPartners()
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    var partners = db.Partners.Where(s => s.EsMemberId == memberId).ToList();
                    return partners;
                }
                catch (Exception)
                {
                    return new List<Partners>();
                }

            }
        }
        private static decimal TryGetPartnersAmount(bool isDebit)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Partners.Where(s => s.EsMemberId == ApplicationManager.Member.Id).Sum(s => isDebit ? (s.Debit ?? 0) : (s.Credit ?? 0));
                }
                catch (Exception)
                {
                    return 0;
                }

            }
        }
        private static List<Partners> TryGetPartners(PartnerType partnerType)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Partners.Where(s => s.EsMemberId == memberId && (partnerType == PartnerType.None || s.EsPartnersTypeId == (long)partnerType))
                                        .ToList();
                }
                catch (Exception)
                {
                    return new List<Partners>();
                }

            }
        }
        private static List<Partners> TryGetPartnersForProducts(List<Guid> productIds)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var partnerIds = db.InvoiceItems.Where(s => productIds.Contains(s.ProductId)).OrderByDescending(s => s.Invoices.CreateDate).Select(s => s.Invoices.PartnerId).Distinct();
                    return db.Partners.Where(s => partnerIds.Contains(s.Id))
                                       .ToList();
                }
                catch (Exception)
                {
                    return new List<Partners>();
                }

            }
        }
        private static List<Partners> TryGetPartners(List<Guid> ids)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Partners.Where(s => ids.Contains(s.Id)).ToList();
                }
                catch (Exception)
                {
                    return new List<Partners>();
                }

            }
        }
        private static bool TryAddPartners(Partners item)
        {
            if (item == null) return false;
            using (var db = GetDataContext())
            {
                try
                {
                    if (db.Partners.SingleOrDefault(s => s.Id == item.Id && s.EsMemberId == item.EsMemberId) != null) { return false; }
                    db.Partners.Add(item);
                    db.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        private static bool TryEditPartner(Partners item)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var exItem = db.Partners.FirstOrDefault(s => s.Id == item.Id && s.EsMemberId == item.EsMemberId);
                    if (exItem == null) return false;
                    exItem.EsPartnersTypeId = item.EsPartnersTypeId;
                    exItem.EsUserId = item.EsUserId;
                    exItem.ClubSixteenId = item.ClubSixteenId;
                    exItem.FullName = item.FullName;
                    exItem.FirstName = item.FirstName;
                    exItem.LastName = item.LastName;
                    exItem.Mobile = item.Mobile;
                    exItem.Email = item.Email;
                    exItem.Address = item.Address;
                    exItem.Discount = item.Discount;
                    exItem.Debit = item.Debit;
                    exItem.Credit = item.Credit;
                    exItem.MaxDebit = item.MaxDebit;
                    exItem.TIN = item.TIN;
                    exItem.PasportData = item.PasportData;
                    exItem.JuridicalAddress = item.JuridicalAddress;
                    exItem.Bank = item.Bank;
                    exItem.BankAccount = item.BankAccount;
                    exItem.Notes = item.Notes;
                    db.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        #endregion

        public static PartnerModel GetProviderForProduct(ProductModel item)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var partnerid =
                        db.InvoiceItems.Where(s => s.ProductId == item.Id && s.Invoices.InvoiceTypeId == (long)InvoiceType.PurchaseInvoice && s.Invoices.MemberId == ApplicationManager.Member.Id)
                        .OrderByDescending(s => s.Invoices.ApproveDate)
                        .Select(s => s.Invoices.PartnerId).FirstOrDefault();
                    return GetPartner(partnerid);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
