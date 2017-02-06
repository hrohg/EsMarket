using System;
using System.Linq;
using System.Transactions;
using ES.DataAccess.Models;
using ES.DataUpdate.Helper;

namespace ES.DataUpdate.Managers
{
    public class EslUpdateManager : BaseManager
    {
        #region Private methods

        private static bool TryUpdateEsMemberWithData(long memberId)
        {
            #region Add base
            if (!TryUpdateMainProvisions()) { return false; }
            using (var dbServer = GetServerDataContext())
            {
                using (var dbLocal = GetDataContext())
                {
                    try
                    {
                        #region Add member
                        var exMember = dbLocal.EsMembers.SingleOrDefault(s => s.Id == memberId);
                        if (exMember == null)
                        {
                            var esMember = dbServer.EsMembers.SingleOrDefault(s => s.Id == memberId);
                            var newItem = ConvertMember(esMember);
                            dbLocal.EsMembers.Add(newItem);
                            dbLocal.SaveChanges();
                        }
                        #endregion
                        #region EsUsers, MembersUsersRoles
                        var esMembersUsersRoles = dbServer.MemberUsersRoles.Where(s => s.MemberId == memberId);
                        if (esMembersUsersRoles == null || !esMembersUsersRoles.Any()) return false;
                        var esUsers = dbServer.EsUsers.Where(s => esMembersUsersRoles.Select(t => t.EsUserId).Distinct().Contains(s.UserId));
                        var exUsers = dbLocal.EsUsers.ToList();
                        foreach (var item in esUsers.Where(s => exUsers.FirstOrDefault(t => t.UserId == s.UserId) == null))
                        {
                            var newItem = ConvertEsUser(item);
                            dbLocal.EsUsers.Add(newItem);
                        }
                        dbLocal.SaveChanges();
                        var exMembersUsersRoles = dbLocal.MemberUsersRoles.Where(s => s.MemberId == memberId);
                        foreach (var item in esMembersUsersRoles.Where(s => exMembersUsersRoles.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            var newItem = new MemberUsersRoles
                            {
                                Id = item.Id,
                                MemberRoleId = item.MemberRoleId,
                                MemberId = item.MemberId,
                                EsUserId = item.EsUserId
                            };
                            dbLocal.MemberUsersRoles.Add(newItem);
                        }
                        #endregion
                        dbLocal.SaveChanges();

                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            #endregion
            #region Products
            using (var scope = new TransactionScope())
            {
                using (var dbServer = GetServerDataContext())
                {
                    using (var dbLocal = GetDataContext())
                    {
                        try
                        {
                            #region Es Product
                            var exProducts = dbLocal.Products.Where(s => s.EsMemberId == memberId);
                            var esProducts = dbServer.Products.Where(s => s.EsMemberId == memberId);
                            foreach (var item in esProducts.Where(s => exProducts.FirstOrDefault(t => t.Id == s.Id) == null))
                            {
                                dbLocal.Products.Add(item);
                            }
                            #endregion
                            dbLocal.SaveChanges();
                            scope.Complete();
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
            using (var scope = new TransactionScope())
            {
                using (var dbServer = GetServerDataContext())
                {
                    using (var dbLocal = GetDataContext())
                    {
                        try
                        {
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
                            dbLocal.SaveChanges();
                            scope.Complete();

                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
            using (var scope = new TransactionScope())
            {
                using (var dbServer = GetServerDataContext())
                {
                    using (var dbLocal = GetDataContext())
                    {
                        try
                        {
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
                            dbLocal.SaveChanges();
                            scope.Complete();

                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
            using (var scope = new TransactionScope())
            {
                using (var dbServer = GetServerDataContext())
                {
                    using (var dbLocal = GetDataContext())
                    {
                        try
                        {
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

                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private static bool ExecutedUpdateDataFromServer(long memberId)
        {
            var dbServer = GetServerDataContext();
            #region Executed
            #region Products

            using (var dbLocal = GetDataContext())
            {
                try
                {
                    #region Es Product
                    var exProducts = dbLocal.Products.Where(s => s.EsMemberId == memberId).ToList();
                    var esProducts = dbServer.Products.Where(s => s.EsMemberId == memberId).ToList();
                    foreach (var item in esProducts.Where(s => exProducts.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        var newItem = ConvertProduct(item);
                        dbLocal.Products.Add(newItem);
                    }
                    #endregion
                    dbLocal.SaveChanges();

                }
                catch (Exception)
                {
                    return false;
                }

            }

            #endregion

            var esCashdesk = dbServer.CashDesk.Where(s => s.MemberId == memberId).ToList();

            using (var dbLocal = GetDataContext())
            {
                try
                {
                    #region Add Cashdesk
                    var exCashdesk = dbLocal.CashDesk.Where(s => s.MemberId == memberId).ToList();

                    foreach (var item in esCashdesk.Where(s => exCashdesk.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        var newItem = new CashDesk
                        {
                            Id = item.Id,
                            MemberId = item.MemberId,
                            Total = item.Total,
                            Name = item.Name,
                            Description = item.Description,
                            Notes = item.Notes,
                            IsCash = item.IsCash,
                            IsActive = item.IsActive
                        };
                        dbLocal.CashDesk.Add(newItem);
                    }
                    dbLocal.SaveChanges();
                    #endregion
                    #region Es Stocks
                    var exStocks = dbLocal.EsStock.Where(s => s.EsMemberId == memberId).ToList();
                    var esStocks = dbServer.EsStock.Where(s => s.EsMemberId == memberId).ToList();
                    foreach (var item in esStocks.Where(s => exStocks.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        var newItem = item;
                        dbLocal.EsStock.Add(newItem);
                    }
                    dbLocal.SaveChanges();
                    #endregion
                    #region Es Defaults
                    var exDefaults = dbLocal.EsDefaults.Where(s => s.MemberId == memberId).ToList();
                    var esDefaults = dbServer.EsDefaults.Where(s => s.MemberId == memberId).ToList();
                    foreach (var item in esDefaults.Where(s => exDefaults.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        dbLocal.EsDefaults.Add(item);
                    }
                    dbLocal.SaveChanges();
                    #endregion
                    #region Es Partners
                    var exPartners = dbLocal.Partners.Where(s => s.EsMemberId == memberId).ToList();
                    var esPartners = dbServer.Partners.Where(s => s.EsMemberId == memberId).ToList();
                    foreach (var item in esPartners.Where(s => exPartners.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        var newItem = ConvertPartner(item);
                        dbLocal.Partners.Add(newItem);
                    }
                    dbLocal.SaveChanges();
                    #endregion



                }
                catch (Exception)
                {
                    return false;
                }
            }

            #endregion executed
            //using (var scope = new TransactionScope())
            //{

            using (var dbLocal = GetDataContext())
            {
                try
                {
                    //Product
                    #region Es Product
                    //var exProducts = dbLocal.Products.Where(s => s.EsMemberId == memberId);
                    //var esProducts = dbServer.Products.Where(s => s.EsMemberId == memberId);
                    //foreach (var item in exProducts.Where(s => esProducts.FirstOrDefault(t => t.Id == s.Id) == null))
                    //{
                    //    dbLocal.Products.Add(item);
                    //}
                    #endregion
                    #region Es ProductItems
                    var exProductItems = dbLocal.ProductItems.Where(s => s.MemberId == memberId).ToList();
                    var esProductItems = dbServer.ProductItems.Where(s => s.MemberId == memberId).ToList();
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
                    var exAccountingRecords = dbLocal.AccountingRecords.Where(s => s.MemberId == memberId).ToList();
                    var esAccountingRecords = dbServer.AccountingRecords.Where(s => s.MemberId == memberId).ToList();
                    foreach (var item in exAccountingRecords.Where(s => esAccountingRecords.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        dbLocal.AccountingRecords.Add(item);
                    }
                    #endregion
                    #region AccountingRecords
                    var exAccountsReceivable = dbLocal.AccountsReceivable.Where(s => s.MemberId == memberId).ToList();
                    var esAccountsReceivable = dbServer.AccountsReceivable.Where(s => s.MemberId == memberId).ToList();
                    foreach (var item in exAccountsReceivable.Where(s => esAccountsReceivable.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        dbLocal.AccountsReceivable.Add(item);
                    }
                    #endregion
                    dbLocal.SaveChanges();
                    //scope.Complete();

                }
                catch (Exception)
                {
                    return false;
                }
                // }

            }
            using (var scope = new TransactionScope())
            {

                using (var dbLocal = GetDataContext())
                {
                    try
                    {
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

                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

            }
            return true;
        }

        private static bool TryUpdateDataFromServer(long memberId)
        {
            using (var dbServer = GetServerDataContext())
            { 
           
            using (var dbLocal = GetDataContext())
            {
                try
                {
                    //Product
                    #region Es Product
                    //var exProducts = dbLocal.Products.Where(s => s.EsMemberId == memberId);
                    //var esProducts = dbServer.Products.Where(s => s.EsMemberId == memberId);
                    //foreach (var item in exProducts.Where(s => esProducts.FirstOrDefault(t => t.Id == s.Id) == null))
                    //{
                    //    dbLocal.Products.Add(item);
                    //}
                    #endregion
                    #region Es ProductItems
                    var exProductItems = dbLocal.ProductItems.Where(s => s.MemberId == memberId).ToList();
                    var esProductItems = dbServer.ProductItems.Where(s => s.MemberId == memberId).ToList();
                    foreach (var item in esProductItems.Where(s => exProductItems.FirstOrDefault(t => t.Id == s.Id) == null))
                    {
                        dbLocal.ProductItems.Add(ConvertProductItem(item));
                    }
                    dbLocal.SaveChanges();
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
                    var exAccountingRecords = dbLocal.AccountingRecords.Where(s => s.MemberId == memberId).ToList();
                    var esAccountingRecords = dbServer.AccountingRecords.Where(s => s.MemberId == memberId).ToList();
                    foreach (var item in esAccountingRecords.Where(s => exAccountingRecords.FirstOrDefault(t => t.Id == s.Id) == null))
                    {

                        dbLocal.AccountingRecords.Add(ConvertAccountingRecords(item));
                    }
                    #endregion
                    #region AccountingRecords
                    var exAccountsReceivable = dbLocal.AccountsReceivable.Where(s => s.MemberId == memberId).ToList();
                    var esAccountsReceivable = dbServer.AccountsReceivable.Where(s => s.MemberId == memberId).ToList();
                    foreach (var item in esAccountsReceivable.Where(s => exAccountsReceivable.FirstOrDefault(t => t.Id == s.Id) == null))
                    {

                        dbLocal.AccountsReceivable.Add(item);
                    }
                    #endregion
                    //dbLocal.SaveChanges();
                    

                }
                catch (Exception)
                {
                    return false;
                }
                 

            }
            var esInvoices = dbServer.Invoices.Where(s => s.MemberId == memberId).ToList();
            var esInvoiceItems = dbServer.InvoiceItems.Where(s => s.Invoices.MemberId == memberId).ToList();
            //using (var scope = new TransactionScope())
            //{

                using (var dbLocal = GetDataContext())
                {
                    try
                    {
                        //Invoice
                        #region Invoices
                        var exInvoices = dbLocal.Invoices.Where(s => s.MemberId == memberId).ToList();

                        foreach (var item in esInvoices.Where(s => exInvoices.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.Invoices.Add(ConvertInvoice(item));
                        }
                        #endregion
                        #region InvoiceItems
                        var exInvoiceItems = dbLocal.InvoiceItems.Where(s => s.Invoices.MemberId == memberId).ToList();

                        foreach (var item in esInvoiceItems.Where(s => exInvoiceItems.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.InvoiceItems.Add(ConvertInvoiceItem(item));
                        }
                        #endregion
                        dbLocal.SaveChanges();
                       // scope.Complete();

                    }
                    catch (Exception)
                    {
                        return false;
                    }
                //}
            }
            }
            return true;
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
                {
                    using (var dbLocal = GetDataContext())
                    {
                        #region Update accounting plan
                        var esAccountingPlans = dbServer.AccountingPlan.ToList();
                        var exAccountingPlans = dbLocal.AccountingPlan.ToList();
                        foreach (var item in esAccountingPlans.Where(s => exAccountingPlans.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            dbLocal.AccountingPlan.Add(item);
                        }
                        dbLocal.SaveChanges();
                        #endregion
                        #region Update EsInvoiceTypes
                        var esInvoiceTypes = dbServer.EsInvoiceTypes.ToList();
                        var exInvoiceTypes = dbLocal.EsInvoiceTypes.ToList();
                        foreach (var item in esInvoiceTypes.Where(s => exInvoiceTypes.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            var newItem = new EsInvoiceTypes { Description = item.Description, Name = item.Name };
                            dbLocal.EsInvoiceTypes.Add(newItem);
                        }
                        dbLocal.SaveChanges();
                        #endregion
                        #region Update EsPartnersTypes
                        var esPartnerTypes = dbServer.EsPartnersTypes.ToList();
                        var exPartnerTypes = dbLocal.EsPartnersTypes.ToList();
                        foreach (var item in esPartnerTypes.Where(s => exPartnerTypes.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            var newItem = new EsPartnersTypes { Description = item.Description };
                            dbLocal.EsPartnersTypes.Add(newItem);
                        }
                        dbLocal.SaveChanges();
                        #endregion
                        #region Update UserRoles
                        var esUserRoles = dbServer.ESUserRoles.ToList();
                        var exUserRoles = dbLocal.ESUserRoles.ToList();
                        foreach (var item in esUserRoles.Where(s => exUserRoles.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            var newItem = new ESUserRoles { Description = item.Description, Name = item.Name };
                            //dbLocal.ESUserRoles.Add(newItem);
                        }
                        dbLocal.SaveChanges();
                        #endregion
                        #region Update MembersRoles
                        var esMembersRoles = dbServer.MembersRoles.ToList();
                        var exMembersRoles = dbLocal.MembersRoles.ToList();
                        foreach (var item in esMembersRoles.Where(s => exMembersRoles.FirstOrDefault(t => t.Id == s.Id) == null))
                        {
                            var newItem = new MembersRoles { Description = item.Description, RoleName = item.RoleName };
                            //dbLocal.MembersRoles.Add(item);
                        }
                        dbLocal.SaveChanges();
                        #endregion


                        return true;
                    }
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
        public static bool GetMemberFromServerWithData(long memberId)
        {
            return TryUpdateEsMemberWithData(memberId);
        }
        public static bool UpdateDataFromServer(long memberId)
        {
            return TryUpdateDataFromServer(memberId);
        }
        public static bool UpdateMainProvisions()
        {
            return TryUpdateMainProvisions();
        }
        #endregion
        #region converter
        public static EsMembers ConvertMember(EsMembers item)
        {
            if (item == null) return null;
            var newItem = new EsMembers
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.Email,
                ClubSixteenId = item.ClubSixteenId
            };
            return newItem;
        }
        public static EsUsers ConvertEsUser(EsUsers item)
        {
            if (item == null) return null;
            return new EsUsers
            {
                UserId = item.UserId,
                UserName = item.UserName,
                Password = item.Password,
                Email = item.Email,
                Mobile = item.Mobile,
                ClubSixteenId = item.ClubSixteenId,
                LastActivityDate = item.LastActivityDate,
                IsActive = item.IsActive
            };
        }
        public static Products ConvertProduct(Products item)
        {
            if (item == null) return null;
            var exItem = new Products();
            exItem.Id = item.Id;
            exItem.Code = item.Code;
            exItem.Barcode = item.Barcode;
            exItem.Description = item.Description;
            exItem.Mu = item.Mu;
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerPrice;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ImagePath = item.ImagePath;
            exItem.IsEnable = item.IsEnable;
            exItem.BrandId = item.BrandId;
            //exItem.Brand = item.Brand;
            exItem.EsMemberId = item.EsMemberId;
            exItem.LastModifierId = item.LastModifierId;
            return exItem;
        }
        //private static EsStock ConvertStock(EsStock item)
        //{
        //    if (item == null) return null;
        //    return new EsStock()
        //    {
        //        Id = item.Id,
        //        ParentStockId = item.ParentStockId,
        //        StorekeeperId = item.StorekeeperId,
        //        Name = item.Name,
        //        Description = item.Description,
        //        Address = item.Address,
        //        SpecialCode = item.SpecialCode,
        //        IsEnable = item.IsEnable,
        //        EsMemberId = item.EsMemberId
        //    };
        //}
        public static Partners ConvertPartner(Partners item)
        {
            if (item == null) return null;
            return new Partners()
            {
                Id = item.Id,
                EsMemberId = item.EsMemberId,
                EsPartnersTypeId = item.EsPartnersTypeId,
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

        private static ProductItems ConvertProductItem(ProductItems item)
        {
            return new ProductItems
            {
                Id = item.Id,
                ProductId = item.ProductId,
                DeliveryInvoiceId = item.DeliveryInvoiceId,
                StockId = item.StockId,
                Quantity = item.Quantity,
                CostPrice = item.CostPrice,
                CoordinateX = item.CoordinateX,
                CoordinateY = item.CoordinateY,
                CoordinateZ = item.CoordinateZ,
                Description = item.Description,
                ReservedById = item.ReservedById,
                MemberId = item.MemberId
            };
        }

        private static AccountingRecords ConvertAccountingRecords(AccountingRecords item)
        {
            return new AccountingRecords
            {
                Id=item.Id,
                RegisterDate = item.RegisterDate,
                Description = item.Description,
                Amount = item.Amount,
                Debit=item.Debit,
                Credit = item.Credit,
                MemberId = item.MemberId,
                RegisterId = item.RegisterId,
                DebitGuidId = item.DebitGuidId,
                CreditGuidId = item.CreditGuidId,
                DebitLongId = item.DebitLongId,
                CreditLongId = item.CreditLongId    
            };
        }

        private static Invoices ConvertInvoice(Invoices invoice)
        {
            if (invoice == null) return null;
            return new Invoices
            {
                Id = invoice.Id,
                MemberId = invoice.MemberId,
                InvoiceTypeId = invoice.InvoiceTypeId,
                InvoiceIndex = invoice.InvoiceIndex,
                InvoiceNumber = invoice.InvoiceNumber,
                FromStockId = invoice.FromStockId,
                ToStockId = invoice.ToStockId,
                CreatorId = invoice.CreatorId,
                Creator = invoice.Creator,
                CreateDate = invoice.CreateDate,
                ApproveDate = invoice.ApproveDate,
                ApproverId = invoice.ApproverId,
                Approver = invoice.Approver,
                AcceptDate = invoice.AcceptDate,
                AccepterId = invoice.AccepterId,
                PartnerId = invoice.PartnerId,
                ProviderName = invoice.ProviderName,
                ProviderJuridicalAddress = invoice.ProviderJuridicalAddress,
                ProviderAddress = invoice.ProviderAddress,
                ProviderBank = invoice.ProviderBank,
                ProviderBankAccount = invoice.ProviderBankAccount,
                ProviderTaxRegistration = invoice.ProviderTaxRegistration,
                RecipientName = invoice.RecipientName,
                RecipientJuridicalAddress = invoice.RecipientJuridicalAddress,
                RecipientAddress = invoice.RecipientAddress,
                RecipientBank = invoice.RecipientBank,
                RecipientBankAccount = invoice.RecipientBankAccount,
                RecipientTaxRegistration = invoice.RecipientTaxRegistration,
                Discount = invoice.Discount,
                Summ = invoice.Summ,
                Notes = invoice.Notes
            };
        }
        private static InvoiceItems ConvertInvoiceItem(InvoiceItems item)
        {
            if (item == null) return null;
            return new InvoiceItems
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                ProductId = item.ProductId,
                ProductItemId = item.ProductItemId,
                Code = item.Code,
                Description = item.Description,
                Mu = item.Mu,
                Quantity = item.Quantity,
                Price = item.Price,
                CostPrice = item.CostPrice,
                Discount = item.Discount,
                Note = item.Note
            };
        }
        #endregion
    }
}
