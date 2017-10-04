using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            return new PartnerModel(item.EsMemberId)
            {
                Id = item.Id,
                EsMemberId = item.EsMemberId,
                //EsMember = MembersManager.ConvertMember(item.EsMembers),
                PartnersTypeId = item.EsPartnersTypeId,
                PartnersType = Convert(item.EsPartnersTypes),
                EsUserId = item.EsUserId,
                //EsUser = UsersManager.ConvertEsUser(item.EsUsers),
                ClubSixteenId = item.ClubSixteenId,
                FullName = item.FullName,
                FirstName = item.FirstName,
                LastName = item.LastName,
                Mobile = item.Mobile,
                Email = item.Email,
                Address = item.Address,
                Discount = item.Discount,
                Debit = item.Debit??0,
                Credit = item.Credit??0,
                MaxDebit = item.MaxDebit,
                TIN = item.TIN,
                PasportData = item.PasportData,
                JuridicalAddress = item.JuridicalAddress,
                Bank = item.Bank,
                BankAccount = item.BankAccount,
                Notes = item.Notes
            };
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
        public static PartnerTypeModel Convert(EsPartnersTypes item)
        {
            if (item == null) return null;
            return new PartnerTypeModel
            {
                Id = item.Id,
                Description = item.Description
            };
        }
        #endregion

        #region Partners public methods

        public static List<EsPartnersTypes> GetPartnersTypes(long memberId)
        {
            return TryGetPartnersTypes(memberId);
        }
        public static PartnerModel GetPartner(Guid? id, long esMemberId)
        {
            return Convert(TryGetPartner(id, esMemberId));
        }
        public static List<PartnerModel> GetPartners()
        {
            return TryGetPartners().Select(Convert).OrderBy(s => s.FullName).ToList();
        }
        public static List<PartnerModel> GetPartners(List<Guid> ids)
        {
            return TryGetPartners(ids).Select(Convert).OrderBy(s => s.FullName).ToList();
        }
        public static PartnerModel GetDefaultPartner(PartnerType partnerTypeId)
        {
            return TryGetPartners(partnerTypeId).Select(Convert).FirstOrDefault();
        }
        public static PartnerModel GetDefaultParnerByInvoiceType(long esMemberId, InvoiceType invoiceTypeId)
        {
            switch (invoiceTypeId)
            {
                case InvoiceType.SaleInvoice:
                    return Convert(TryGetPartner(DefaultsManager.GetDefaultValueGuid(DefaultControls.Customer, esMemberId), esMemberId));
                case InvoiceType.PurchaseInvoice:
                    return Convert(TryGetPartner(DefaultsManager.GetDefaultValueGuid(DefaultControls.Provider, esMemberId), esMemberId));
                default:
                    return null;
            }
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

        public static bool SetDefault(PartnerModel partner)
        {
            if(partner==null || partner.PartnersTypeId==null) return false;
            string controlKey;
            switch (partner.PartnerTypeEnum)
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
                    throw new ArgumentOutOfRangeException("type", partner.PartnersTypeId, null);
                    return false;
            }
            return DefaultsManager.SetDefault(controlKey, partner.EsMemberId, null, partner.Id );
        }
        #endregion

        #region Partners private methods
        private static List<EsPartnersTypes> TryGetPartnersTypes(long memberId)
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
        private static Partners TryGetPartner(Guid? id, long memberId)
        {
            if (id == null) return null;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Partners.Include(s => s.EsMembers)
                                        .Include(s => s.EsPartnersTypes)
                                        .SingleOrDefault(s => s.Id == id && s.EsMemberId == memberId);
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
                    return db.Partners.Include(s => s.EsMembers)
                                        .Include(s => s.EsPartnersTypes)
                                        .Where(s => s.EsMemberId == memberId).ToList();
                }
                catch (Exception)
                {
                    return new List<Partners>();
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
                    return db.Partners
                                        .Include(s => s.EsMembers)
                                        .Include(s => s.EsPartnersTypes)
                                        .Where(s => s.EsMemberId == memberId && (partnerType ==PartnerType.None || s.EsPartnersTypeId == (long)partnerType))
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
                    return db.Partners.Include(s => s.EsMembers)
                                       .Include(s => s.EsPartnersTypes)
                                       .Where(s => partnerIds.Contains(s.Id))
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
                    return db.Partners.Include(s => s.EsMembers)
                                        .Include(s => s.EsPartnersTypes)
                                        .Where(s =>ids.Contains(s.Id)).ToList();
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
    }
}
