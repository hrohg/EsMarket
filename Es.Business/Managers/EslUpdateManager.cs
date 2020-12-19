using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ES.Business.Helpers;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class UpdateManager : BaseManager
    {
        #region Private methods

        private static bool TryGetEsMemberWithData(int memberId)
        {
            if(!TryUpdateMainProvisions()){return false;}
            using (var scope = new TransactionScope())
            {
                using (var dbServer = GetServerDataContext())
                {
                    using (var dbLocal = GetDataContext())
                    {
                        try
                        {
                            
                            #region Add member
                            var exMember = dbLocal.EsMembers.SingleOrDefault(s => s.Id == memberId);
                            if (exMember == null) return false;
                            var esMember = dbServer.EsMembers.SingleOrDefault(s => s.Id == memberId);
                            dbLocal.EsMembers.Add(esMember);
                            #endregion
                            #region EsUsers, MembersUsersRoles
                            var esMembersUsersRoles = dbServer.MemberUsersRoles.Where(s => s.MemberId == memberId);
                            if (esMembersUsersRoles == null || !esMembersUsersRoles.Any()) return false;
                            var esUsers = dbServer.EsUsers.Where(s => esMembersUsersRoles.Select(t => t.EsUserId).Distinct().Contains(s.UserId));
                            var exUsers = dbLocal.EsUsers.ToList();
                            foreach (var item in exUsers.Where(s => exUsers.FirstOrDefault(t => t.UserId == s.UserId) == null))
                            {
                                dbLocal.EsUsers.Add(item);
                            }
                            var exMembersUsersRoles = dbLocal.MemberUsersRoles.Where(s => s.MemberId == memberId);
                            foreach (var item in esMembersUsersRoles.Where(s => exMembersUsersRoles.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.MemberUsersRoles.Add(item);
                            }
                            #endregion
                            #region Add Cashdesk
                            var exCashdesk = dbLocal.CashDesk.Where(s => s.MemberId == memberId);
                            if (exCashdesk == null) return false;
                            var esCashdesk = dbServer.CashDesk.Where(s => s.MemberId == memberId);
                            foreach (var item in exCashdesk.Where(s => esCashdesk.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.CashDesk.Add(item);
                            }
                            #endregion
                            #region Es Stocks
                            var exStocks = dbLocal.EsStock.Where(s => s.EsMemberId == memberId);
                            if (exStocks == null) return false;
                            var esStocks = dbServer.EsStock.Where(s => s.EsMemberId == memberId);
                            foreach (var item in exStocks.Where(s => esStocks.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.EsStock.Add(item);
                            }
                            #endregion
                            #region Es Defaults
                            var exDefaults = dbLocal.EsDefaults.Where(s => s.MemberId == memberId);
                            if (exDefaults == null) return false;
                            var esDefaults = dbServer.EsDefaults.Where(s => s.MemberId == memberId);
                            foreach (var item in exDefaults.Where(s => esDefaults.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.EsDefaults.Add(item);
                            }
                            #endregion
                            #region Es Partners
                            var exPartners = dbLocal.Partners.Where(s => s.EsMemberId == memberId);
                            var esPartners = dbServer.Partners.Where(s => s.EsMemberId == memberId);
                            foreach (var item in exPartners.Where(s => esPartners.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.Partners.Add(item);
                            }
                            #endregion
                            //Product
                            #region Es Product
                            var exProducts = dbLocal.Products.Where(s => s.EsMemberId == memberId);
                            var esProducts = dbServer.Products.Where(s => s.EsMemberId == memberId);
                            foreach (var item in exProducts.Where(s => esProducts.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.Products.Add(item);
                            }
                            #endregion
                            #region Es ProductItems
                            var exProductItems = dbLocal.ProductItems.Where(s => s.MemberId == memberId);
                            var esProductItems = dbServer.ProductItems.Where(s => s.MemberId == memberId);
                            foreach (var item in exProductItems.Where(s => esProductItems.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.ProductItems.Add(item);
                            }
                            #endregion
                            #region Es ProductCategories does not working
                            //var exroductCategories = dbLocal.ProductCategories.Where(s => s == memberId);
                            //var esroductCategories = dbServer.ProductCategories.Where(s => s.MemberId == memberId);
                            //foreach (var item in exroductCategories.Where(s => esroductCategories.FirstOrDefault(t => t.Id == s.Id) == null))
                            //{
                            //    dbLocal.ProductCategories.Add(item);
                            //}
                            #endregion
                            #region AccountingRecords
                            var exAccountingRecords = dbLocal.AccountingRecords.Where(s => s.MemberId == memberId);
                            var esAccountingRecords = dbServer.AccountingRecords.Where(s => s.MemberId == memberId);
                            foreach (var item in exAccountingRecords.Where(s => esAccountingRecords.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.AccountingRecords.Add(item);
                            }
                            #endregion
                            #region AccountingRecords
                            var exAccountsReceivable = dbLocal.AccountsReceivable.Where(s => s.MemberId == memberId);
                            var esAccountsReceivable = dbServer.AccountsReceivable.Where(s => s.MemberId == memberId);
                            foreach (var item in exAccountsReceivable.Where(s => esAccountsReceivable.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.AccountsReceivable.Add(item);
                            }
                            #endregion
                            //Invoice
                            #region Invoices
                            var exInvoices = dbLocal.Invoices.Where(s => s.MemberId == memberId);
                            var esInvoices = dbServer.Invoices.Where(s => s.MemberId == memberId);
                            foreach (var item in exInvoices.Where(s => esInvoices.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.Invoices.Add(item);
                            }
                            #endregion
                            #region InvoiceItems
                            var exInvoiceItems = dbLocal.InvoiceItems.Where(s => s.Invoices.MemberId == memberId);
                            var esInvoiceItems = dbServer.InvoiceItems.Where(s => s.Invoices.MemberId == memberId);
                            foreach (var item in exInvoiceItems.Where(s => esInvoiceItems.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.InvoiceItems.Add(item);
                            }
                            #endregion
                            dbLocal.SaveChanges();
                            scope.Complete();
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        private static bool TryGetEsUsers()
        {
            using (var dbServer = GetServerDataContext())
            {
                using (var dbLocal = GetDataContext())
                {
                    try
                    {
                        var esUsersInserver = dbServer.EsUsers.ToList();
                        var exUsers = dbLocal.EsUsers.ToList();
                        foreach (var esUser in esUsersInserver.Where(esUser => exUsers.FirstOrDefault(u => u.UserId == esUser.UserId) == null))
                        {
                            dbLocal.EsUsers.Add(esUser);
                        }
                        dbLocal.SaveChanges();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }

        private static bool TryUpdateMainProvisions()
        {
            using (var dbServer = GetServerDataContext())
            {
                try
                    {using (var dbLocal = GetDataContext())
                    {
                        TryUpdateBrands();
                        #region Update accounting plan
                        var esAccountingPlans = dbServer.AccountingPlan.ToList();
                        var exAccountingPlans = dbLocal.AccountingPlan.ToList();
                        foreach (var item in esAccountingPlans.Where(s => exAccountingPlans.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.AccountingPlan.Add(new AccountingPlan{Id = item.Id, CreditId = item.CreditId, DebitId = item.DebitId});
                        }
                        #endregion
                        #region Update EsInvoiceTypes
                        var esInvoiceTypes = dbServer.EsInvoiceTypes.ToList();
                        var exInvoiceTypes = dbLocal.EsInvoiceTypes.ToList();
                        foreach (var item in esInvoiceTypes.Where(s => exInvoiceTypes.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.EsInvoiceTypes.Add(new EsInvoiceTypes{Id = item.Id, Name =item.Name, Description = item.Description});
                        }
                        #endregion
                        #region Update EsPartnersTypes
                        var esPartnerTypes = dbServer.EsPartnersTypes.ToList();
                        var exPartnerTypes = dbLocal.EsPartnersTypes.ToList();
                        foreach (var item in esPartnerTypes.Where(s => exPartnerTypes.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.EsPartnersTypes.Add(new EsPartnersTypes{Id= item.Id, Description = item.Description});
                        }
                        #endregion
                        #region Update UserRoles
                        var esUserRoles = dbServer.ESUserRoles.ToList();
                        var exUserRoles = dbLocal.ESUserRoles.ToList();
                        foreach (var item in esUserRoles.Where(s => exUserRoles.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.ESUserRoles.Add(new ESUserRoles{Id = item.Id, Name = item.Name, Description = item.Description});
                        }
                        #endregion
                        #region Update MembersRoles
                        var esMembersRoles = dbServer.MembersRoles.ToList();
                        var exMembersRoles = dbLocal.MembersRoles.ToList();
                        foreach (var item in esMembersRoles.Where(s => exMembersRoles.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.MembersRoles.Add( new MembersRoles{Id = item.Id, RoleName = item.RoleName, Description = item.Description});
                        }
                        #endregion

                        dbLocal.SaveChanges();
                        return true;
                    }}
                    catch (Exception)
                    {
                        return false;
                    }
                
            }
        }
        private static List<Brands> TryGetServerBrands()
        {
            using (var db = GetServerDataContext())
            {
                return db.Brands.Where(s=>s.IsActive!=null && (bool)s.IsActive).OrderBy(s => s.Name).ToList();
            }
        }
        private static bool TryUpdateBrands()
        {
            var serverBrands = TryGetServerBrands();
            var serverBrandIds = serverBrands.Select(s => s.Id).ToList();
            using (var db = GetDataContext())
            {
                var removedBrands = db.Brands.Where(s => !serverBrandIds.Contains(s.Id)).ToList();
                foreach (var removedBrand in removedBrands)
                {
                    removedBrand.IsActive = false;
                }
                foreach (var serverBrand in serverBrands)
                {
                    var exBrand = db.Brands.SingleOrDefault(s => s.Id == serverBrand.Id);
                    if (exBrand == null)
                    {
                        db.Brands.Add(new Brands
                        {
                            Id = serverBrand.Id, 
                            Name = serverBrand.Name, 
                            Description = serverBrand.Description, 
                            IsActive = serverBrand.IsActive
                        });
                    }
                    else
                    {
                        exBrand.Name = serverBrand.Name;
                        exBrand.Description = serverBrand.Description;
                        exBrand.IsActive = serverBrand.IsActive;
                    }
                }
                try
                {
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
        #region needed update
        //accounting records
        //accounting receivable
        //invoiceitems
        //invoice



        //brands
        //Categories
        //UserInRole
        #endregion

        #region Public methods
        public static bool GetUsersFromServer()
        {
            return TryGetEsUsers();
        }
        public static bool GetMemberFromServerWithData(int memberId)
        {
            return TryGetEsMemberWithData(memberId);
        }
        public static bool UpdateMainProvisions()
        {
            return TryUpdateMainProvisions();
        }
        #endregion
    }
}
