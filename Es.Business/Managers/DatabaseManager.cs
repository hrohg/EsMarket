using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Transactions;
using ES.Business.Helpers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Common.Models;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public enum SyncronizeServersMode
    {
        DownloadMemberData,
        DownloadUserData,
        DownloadBaseData,
        SyncronizeBaseData,
        None
    }
    public class DatabaseManager : BaseManager
    {
        #region Connection Strings
        private static string ServerConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = "bamboo.arvixe.com"; // "93.187.163.33,14033";
                string databaseName = "EsStockDb";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.PersistSecurityInfo = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.UserID = "esstockdb_user"; // "sa";
                sqlBuilder.Password = "esstockdb@)!$"; //"academypbx569280";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
                return entityBuilder.ConnectionString;
            }
        }
        private static string LocalhostConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = @"localhost";
                string databaseName = "EsStockDb";
                string user = "sa";
                string pass = "hhgpas";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.ApplicationName = "EntityFramework";
                //sqlBuilder.UserID = "sa";
                //sqlBuilder.Password = "eslsqlserver@)!$";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";


                //return @"provider=System.Data.SqlClient;provider connection string=&quot;data source=localhost\ESL;initial catalog=ESStockDB;Integrated Security=True; MultipleActiveResultSets=True;App=EntityFramework&quot;";
                return CreateConnectionString(serverName, databaseName);
                //return entityBuilder.ConnectionString;
            }
        }
        private static string DefaultConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = "ESLServer";
                string databaseName = "EsStockDb";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.PersistSecurityInfo = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.UserID = "sa";
                sqlBuilder.Password = "eslsqlserver@)!$";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
                return entityBuilder.ConnectionString;
            }
        }
        private static string CreateConnectionString(string host, string catalog)
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = host,
                InitialCatalog = catalog,
                PersistSecurityInfo = true,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,

                UserID = "",
                Password = "",
            };

            // assumes a connectionString name in .config of MyDbEntities
            var entityConnectionStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuilder.ConnectionString,
                Metadata = "res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl",
            };

            return entityConnectionStringBuilder.ConnectionString;
        }
        public static string CreateConnectionString(DataServer server)
        {
            if (server != null)
            {
                switch (server.Description.ToLower())
                {
                    case "esserver":
                        return ServerConnectionString;
                        break;
                    case "default":
                        return DefaultConnectionString;
                        break;
                    case "local":
                        return LocalhostConnectionString;
                        break;
                    default:
                        if (string.IsNullOrEmpty(server.Name) || string.IsNullOrEmpty(server.Database)) { return ServerConnectionString; }
                        var sqlBuilder = new SqlConnectionStringBuilder();
                        // Set the properties for the data source.
                        sqlBuilder.DataSource = string.Format("{0}{1}{2}",
                            server.Name,
                            !string.IsNullOrEmpty(server.Instance) ? string.Format(@"\{0}", server.Instance) : string.Empty,
                            server.Port != null && server.Port != 0 ? string.Format(",{0}", server.Port) : string.Empty);
                        sqlBuilder.InitialCatalog = server.Database;
                        sqlBuilder.IntegratedSecurity = server.IntegratedSecurity;
                        sqlBuilder.PersistSecurityInfo = server.PersistSecurityInfo;
                        sqlBuilder.MultipleActiveResultSets = server.MultipleActiveResultSets;
                        sqlBuilder.UserID = server.Login ?? string.Empty;
                        sqlBuilder.Password = server.Password ?? string.Empty;

                        // Build the SqlConnection connection string.
                        string providerString = sqlBuilder.ToString();

                        // Initialize the EntityConnectionStringBuilder.
                        EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

                        //Set the provider name.
                        entityBuilder.Provider = server.ProviderName;

                        // Set the provider-specific connection string.
                        entityBuilder.ProviderConnectionString = providerString;

                        // Set the Metadata location.
                        entityBuilder.Metadata = server.ConnectionMetadata;
                        return entityBuilder.ConnectionString;
                        break;
                }
            }
            else
            {
                return ServerConnectionString;
            }
        }
        #endregion

        #region Converters
        public static ESSharedProducts Convert(Products item)
        {
            if (item == null) return null;
            var exItem = new ESSharedProducts();
            exItem.Id = Guid.NewGuid();
            exItem.OriginalId = item.Id;
            exItem.Code = item.Code;
            exItem.Barcode = item.Barcode;
            exItem.HCDCS = item.HCDCS;
            exItem.Description = item.Description;
            exItem.Mu = item.Mu;
            exItem.IsWeight = item.IsWeight;
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerPrice;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ExpiryDays = item.ExpiryDays;
            exItem.ImagePath = item.ImagePath;
            exItem.IsEnable = item.IsEnable;
            exItem.BrandId = item.BrandId;
            exItem.EsMemberId = item.EsMemberId;
            exItem.LastModifierId = item.LastModifierId;
            return exItem;
        }

        private static Products Convert(ESSharedProducts item, long userId, long memberId)
        {
            if (item == null) return null;
            var exItem = new Products();
            exItem.Id = Guid.NewGuid();
            exItem.Code = item.Code;
            exItem.Barcode = item.Barcode;
            exItem.HCDCS = item.HCDCS;
            exItem.Description = item.Description;
            exItem.Mu = item.Mu;
            exItem.IsWeight = item.IsWeight;
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerPrice;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ExpiryDays = item.ExpiryDays;
            exItem.ImagePath = item.ImagePath;
            exItem.IsEnable = item.IsEnable ?? true;
            exItem.BrandId = item.BrandId;
            exItem.BrandId = item.BrandId;
            exItem.EsMemberId = memberId;
            exItem.LastModifierId = userId;
            return exItem;
        }

        #endregion

        #region Internal methods
        private static bool TryDownloadMemberData(long memberId)
        {
            var server = SelectItemsManager.SelectServer(DataServerSettings.GetDataServers());
            if (server == null) return false;
            using (var db = GetDataContext(CreateConnectionString(server)))
            {
                var dbServer = GetServerDataContext();
                try
                {
                    #region Download Member Data
                    var memberOnServer = dbServer.EsMembers.SingleOrDefault(s => s.Id == memberId);
                    if (memberOnServer == null) return false;
                    var exMember = db.EsMembers.SingleOrDefault(s => s.Id == memberOnServer.Id);
                    if (exMember == null)
                    {
                        exMember = new EsMembers { Id = memberId };
                        db.EsMembers.Add(exMember);
                    }
                    exMember.FullName = memberOnServer.FullName;
                    exMember.Email = memberOnServer.Email;
                    exMember.ClubSixteenId = memberOnServer.ClubSixteenId;

                    db.SaveChanges();
                    #endregion
                }
                catch (Exception)
                {
                    return false;
                }
                dbServer.Dispose();
                return true;
            }
        }
        private static bool TryDownloadUserData(long memberId)
        {
            var server = SelectItemsManager.SelectServer(DataServerSettings.GetDataServers());
            if (server == null) return false;
            using (var db = GetDataContext(CreateConnectionString(server)))
            {
                var dbServer = GetServerDataContext();
                try
                {
                    #region Download User Data
                    var usersOnServer = dbServer.MemberUsersRoles.Where(s => s.MemberId == memberId).Select(s => s.EsUsers).Distinct();
                    foreach (var userOnServer in usersOnServer)
                    {
                        var exUser = db.EsUsers.SingleOrDefault(s => s.UserId == userOnServer.UserId);
                        if (exUser == null)
                        {
                            exUser = new EsUsers
                            {
                                UserId = userOnServer.UserId
                            };
                            db.EsUsers.Add(exUser);
                        }
                        exUser.UserName = userOnServer.UserName;
                        exUser.Password = userOnServer.Password;
                        exUser.Mobile = userOnServer.Mobile;
                        exUser.Email = userOnServer.Email;
                        exUser.ClubSixteenId = userOnServer.ClubSixteenId;
                        exUser.LastActivityDate = userOnServer.LastActivityDate;
                        exUser.IsActive = userOnServer.IsActive;
                    }
                    db.SaveChanges();
                    #endregion
                    #region Download User roles
                    var membersRoles = dbServer.MembersRoles;
                    foreach (var item in membersRoles)
                    {
                        var exMembersRole = db.MembersRoles.SingleOrDefault(s => s.Id == item.Id);
                        if (exMembersRole == null)
                        {
                            exMembersRole = new MembersRoles
                            {
                                Id = item.Id,
                                Description = item.Description,
                                RoleName = item.RoleName
                            };
                            db.MembersRoles.Add(exMembersRole);
                        }
                        else
                        {
                            exMembersRole.Description = item.Description;
                            exMembersRole.RoleName = item.RoleName;
                        }
                    }
                    db.SaveChanges();
                    #endregion
                    #region Download user member role
                    var membersUsersRolesOnServer = dbServer.MemberUsersRoles.Where(s => s.MemberId == memberId);
                    foreach (var item in membersUsersRolesOnServer)
                    {
                        if (db.MemberUsersRoles.Any(s =>
                                    s.MemberId == item.MemberId && s.MemberRoleId == item.MemberRoleId &&
                                    s.EsUserId == item.EsUserId))
                        {
                            continue;
                        }
                        db.MemberUsersRoles.Add(new MemberUsersRoles()
                        {
                            Id = item.Id,
                            EsUserId = item.EsUserId,
                            MemberRoleId = item.MemberRoleId,
                            MemberId = item.MemberId
                        });
                    }
                    db.SaveChanges();
                    #endregion
                }
                catch (Exception)
                {
                    return false;
                }
                dbServer.Dispose();
                MessageManager.OnMessage("Օգտագործողների տեսակները բեռնվել են հաջողությամբ:");

                return true;
            }
        }
        private static bool TryDownloadMemberBaseData(long memberId)
        {
            using (var db = GetDataContext())
            {
                var dbServer = GetServerDataContext();
                try
                {
                    #region Download AccountingPlan
                    var accountingPlans = dbServer.AccountingPlan;
                    foreach (var item in accountingPlans)
                    {
                        var exItem = db.AccountingPlan.SingleOrDefault(s => s.Id == item.Id);
                        if (exItem == null)
                        {
                            db.AccountingPlan.Add(item);
                        }
                        else
                        {
                            continue;
                            exItem.DebitId = item.DebitId;
                            exItem.CreditId = item.CreditId;
                        }
                    }
                    db.SaveChanges();
                    MessageManager.OnMessage("Հաշվառման պլանը բեռնվել է հաջողությամբ:");
                    #endregion Download AccountingPlan

                    #region Download Invoice types
                    MessageManager.OnMessage("Downloading invoices data");

                    var invoiceTypes = dbServer.EsInvoiceTypes.ToList();
                    foreach (var item in invoiceTypes)
                    {
                        var exItem = db.EsInvoiceTypes.FirstOrDefault(s => s.Id == item.Id);
                        if (exItem == null)
                        {
                            exItem = new EsInvoiceTypes
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Description = item.Description
                            };
                            
                            db.EsInvoiceTypes.Add(exItem); 
                            db.SaveChanges();
                        }
                    }
                   
                    MessageManager.OnMessage("Ապրանքագրերի տեսակները բեռնվել են հաջողությամբ:");
                    #endregion Download Invoice types

                    #region Download Partner types
                    MessageManager.OnMessage("Downloading partners data");

                    var partnerTypes = dbServer.EsPartnersTypes.ToList();
                    foreach (var item in partnerTypes)
                    {
                        var exItem = db.EsPartnersTypes.SingleOrDefault(s => s.Id == item.Id);
                        if (exItem == null)
                        {
                            var partnerType = new EsPartnersTypes
                            {
                                Id = item.Id,
                                Description = item.Description
                            };
                            db.EsPartnersTypes.Add(partnerType);
                        }
                    }
                    db.SaveChanges();
                    MessageManager.OnMessage("Պատվիրատուների տեսակները բեռնվել են հաջողությամբ:");
                    #endregion Download Partner types

                    #region Download Roles
                    var roles = dbServer.MembersRoles;
                    foreach (var item in roles)
                    {
                        var exItem = db.MembersRoles.SingleOrDefault(s => s.Id == item.Id);
                        if (exItem == null)
                        {
                            exItem = new MembersRoles
                            {
                                Id = item.Id,
                                Description = item.Description,
                                RoleName = item.RoleName
                            };
                            db.MembersRoles.Add(exItem);
                        }
                        else
                        {
                            exItem.RoleName = item.RoleName;
                            exItem.Description = item.Description;
                        }
                    }
                    db.SaveChanges();
                    #endregion Download roles
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage(ex.InnerException != null && ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.Message, MessageTypeEnum.Warning);
                    return false;
                }
                dbServer.Dispose();
                return true;
            }
        }
        private static bool TrySyncronizeMemberBaseData(long memberId)
        {
            using (var db = GetDataContext())
            {
                var dbServer = GetServerDataContext();
                try
                {
                    #region Syncronize User Data
                    var users = db.EsUsers;
                    var usersOnServer = dbServer.EsUsers;
                    foreach (var user in users)
                    {
                        var userOnServer = usersOnServer.SingleOrDefault(s => s.UserId == user.UserId);
                        if (userOnServer == null)
                        {
                            // ToDo
                            continue;
                        }
                        if (user.LastActivityDate == userOnServer.LastActivityDate)
                        {
                            continue;
                        }
                        if (user.LastActivityDate > userOnServer.LastActivityDate)
                        {
                            userOnServer.UserName = user.UserName;
                            userOnServer.Password = user.Password;
                            userOnServer.Email = user.Email;
                            userOnServer.Mobile = user.Mobile;
                            userOnServer.LastActivityDate = user.LastActivityDate;
                            userOnServer.ClubSixteenId = user.ClubSixteenId;
                        }
                        else
                        {
                            user.UserName = userOnServer.UserName;
                            user.Password = userOnServer.Password;
                            user.Email = userOnServer.Email;
                            user.Mobile = userOnServer.Mobile;
                            user.IsActive = userOnServer.IsActive;
                            user.LastActivityDate = userOnServer.LastActivityDate;
                            user.ClubSixteenId = userOnServer.ClubSixteenId;
                        }
                    }
                    dbServer.SaveChanges();
                    db.SaveChanges();
                    #endregion

                    #region Syncronize user member role
                    //foreach (var user in users)
                    //{
                    //    var userOnServer = usersOnServer.SingleOrDefault(s => s.UserId == user.UserId);
                    //    if (userOnServer == null)
                    //    {
                    //        // ToDo
                    //        continue;
                    //    }
                    //    var exUser = user;
                    //    var membersUsersRoles = db.MemberUsersRoles.Where(s => s.MemberId == memberId && s.EsUserId == exUser.UserId);
                    //    var membersUsersRolesOnServer = dbServer.MemberUsersRoles.Where(s => s.MemberId == memberId && s.EsUserId == exUser.UserId);
                    //    if (user.LastActivityDate > userOnServer.LastActivityDate)
                    //    {
                    //        foreach (var itemForRemove in membersUsersRolesOnServer)
                    //        {
                    //            dbServer.MemberUsersRoles.Remove(itemForRemove);
                    //        }
                    //        foreach (var itemForInsert in membersUsersRoles)
                    //        {
                    //            dbServer.MemberUsersRoles.Add(new MemberUsersRoles()
                    //            {
                    //                Id = itemForInsert.Id,
                    //                EsUserId = itemForInsert.EsUserId,
                    //                MemberRoleId = itemForInsert.MemberRoleId,
                    //                MemberId = itemForInsert.MemberId
                    //            });
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (var itemForRemove in membersUsersRoles)
                    //        {
                    //            db.MemberUsersRoles.Remove(itemForRemove);
                    //        }
                    //        foreach (var itemForInsert in membersUsersRolesOnServer)
                    //        {
                    //            db.MemberUsersRoles.Add(new MemberUsersRoles()
                    //            {
                    //                Id = itemForInsert.Id,
                    //                EsUserId = itemForInsert.EsUserId,
                    //                MemberRoleId = itemForInsert.MemberRoleId,
                    //                MemberId = itemForInsert.MemberId
                    //            });
                    //        }
                    //    }
                    //}
                    //db.SaveChanges();
                    #endregion

                    //Disabled
                    #region Syncronize CashDesks
                    //var cashDesks = dbServer.CashDesk.Where(s => s.MemberId == memberId);
                    //foreach (var item in cashDesks)
                    //{
                    //    var exCashDesk = db.CashDesk.SingleOrDefault(s => s.Id == item.Id);
                    //    if (exCashDesk == null)
                    //    {
                    //        db.CashDesk.Add(new CashDesk
                    //        {
                    //            Id = item.Id,
                    //            MemberId = item.MemberId,
                    //            Total = item.Total,
                    //            Name = item.Name,
                    //            Description = item.Description,
                    //            Notes = item.Notes,
                    //            IsCash = item.IsCash,
                    //            IsActive = item.IsActive,
                    //        });
                    //    }
                    //    else
                    //    {
                    //        // ToDo
                    //        exCashDesk.MemberId = item.MemberId;
                    //        //exCashDesk.Total = item.Total;
                    //        exCashDesk.Name = item.Name;
                    //        exCashDesk.Description = item.Description;
                    //        exCashDesk.Notes = item.Notes;
                    //        exCashDesk.IsCash = item.IsCash;
                    //        exCashDesk.IsActive = item.IsActive;
                    //    }

                    //}
                    //db.SaveChanges();
                    #endregion

                    //Disabled
                    #region Syncronize stocks
                    //var stocks = dbServer.EsStock.Where(s => s.EsMemberId == memberId);
                    //foreach (var item in stocks)
                    //{
                    //    var exStock = db.EsStock.SingleOrDefault(s => s.Id == item.Id);
                    //    if (exStock == null)
                    //    {
                    //        //todo exception on adding new stock
                    //        db.EsStock.Add(new EsStock()
                    //        {
                    //            Id = item.Id,
                    //            ParentStockId = item.ParentStockId,
                    //            StorekeeperId = item.StorekeeperId,
                    //            Name = item.Name,
                    //            Description = item.Description,
                    //            Address = item.Address,
                    //            SpecialCode = item.SpecialCode,
                    //            IsEnable = item.IsEnable,
                    //            EsMemberId = item.EsMemberId
                    //        });
                    //    }
                    //    else
                    //    {
                    //        // ToDo
                    //        exStock.ParentStockId = item.ParentStockId;
                    //        exStock.StorekeeperId = item.StorekeeperId;
                    //        exStock.Name = item.Name;
                    //        exStock.Description = item.Description;
                    //        exStock.Address = item.Address;
                    //        exStock.SpecialCode = item.SpecialCode;
                    //        exStock.IsEnable = item.IsEnable;
                    //        exStock.EsMemberId = item.EsMemberId;
                    //    }
                    //}
                    //db.SaveChanges();
                    #endregion
                    
                    //Disabled
                    #region Syncronize Default data
                    //var defaults = dbServer.EsDefaults.Where(s => s.MemberId == memberId);
                    //foreach (var item in defaults)
                    //{
                    //    var exDefault = db.EsDefaults.SingleOrDefault(s => s.Id == item.Id);
                    //    if (exDefault == null)
                    //    {
                    //        db.EsDefaults.Add(new EsDefaults()
                    //        {
                    //            Id = item.Id,
                    //            MemberId = item.MemberId,
                    //            Control = item.Control,
                    //            ValueInGuid = item.ValueInGuid,
                    //            ValueInLong = item.ValueInLong
                    //        });
                    //    }
                    //    else
                    //    {
                    //        exDefault.MemberId = item.MemberId;
                    //        exDefault.Control = item.Control;
                    //        exDefault.ValueInGuid = item.ValueInGuid;
                    //        exDefault.ValueInLong = item.ValueInLong;

                    //    }
                    //}
                    //db.SaveChanges();
                    #endregion
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage(ex.InnerException != null && ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.Message, MessageTypeEnum.Warning);
                    return false;
                }
                dbServer.Dispose();
                MessageManager.OnMessage("Սերվերի համաժամանակեցումն իրականացել է հաջողությամբ:");
                return true;
            }
        }
        private static bool TrySyncronizeProducts()
        {
            using (var db = GetDataContext())
            {
                var dbServer = GetServerDataContext();
                try
                {
                    var products = db.Products;
                    var members = db.EsMembers.ToList();
                    foreach (var product in products)
                    {
                        if (string.IsNullOrEmpty(product.Barcode) || dbServer.ESSharedProducts.Any(s => s.OriginalId == product.Id))
                        {
                            continue;
                        }
                        dbServer.ESSharedProducts.Add(Convert(product));
                    }
                    dbServer.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }
                dbServer.Dispose();
                return true;
            }
        }
        #endregion

        #region External methods
        public static bool IsCanCreateLocalDb()
        {
            using (var db = GetDataContext())
            {
                using (var serverDb = GetServerDataContext())
                {
                    try
                    {
                        return !db.EsMembers.Any() && serverDb.EsMembers.Any();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }
        public static string GetConnectionString()
        {
            using (var db = GetDataContext())
            {
                return db.Database.Connection.ConnectionString;
            }
        }
        public static bool CheckConnection(string connectionString)
        {
            using (var db = GetDataContext(connectionString))
            {
                try
                {
                    var members = db.EsMembers.ToList();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage(ex.InnerException != null && ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Download Member Data
        /// </summary>
        /// <param name="syncronizeMode"></param>
        /// <returns></returns>
        public static bool SyncronizeServers(SyncronizeServersMode syncronizeMode)
        {
            var memberId = ApplicationManager.Member.Id;
            switch (syncronizeMode)
            {
                case SyncronizeServersMode.DownloadMemberData:
                    return TryDownloadMemberData(memberId);
                case SyncronizeServersMode.DownloadUserData:
                    return TryDownloadUserData(memberId);
                case SyncronizeServersMode.DownloadBaseData:
                    return TryDownloadMemberBaseData(memberId);
                case SyncronizeServersMode.SyncronizeBaseData:
                    return TrySyncronizeMemberBaseData(memberId);
                case SyncronizeServersMode.None:
                    return false;
                default:
                    return false;
            }
        }
        
        public static void SyncronizeProducts()
        {
            MessageManager.OnMessage("Տվյալների համաժամանակեցումն սկսված է։", MessageTypeEnum.Success);
            if (TrySyncronizeProducts())
            {
                MessageManager.OnMessage("Տվյալների համաժամանակեցումն ավարտվել է հաջողությամբ է։", MessageTypeEnum.Success);
            }
            else
            {
                MessageManager.OnMessage("Տվյալների համաժամանակեցումը ձախողվել է։", MessageTypeEnum.Warning);
            }

        }

        #region Backup Restore
        public static string BackUpCommand(string databaseName, string fileAddress)
        {
            string command = string.Format("BACKUP DATABASE {0} TO DISK = N'{1}' WITH NOFORMAT, NOINIT,  NAME = N'MyAir-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10", databaseName, fileAddress);
            return command;
        }
        public static string RestoreCommand(string databaseName, string fileAddress)
        {
            string command = @"use [master]
                        ALTER DATABASE  " + databaseName + @"
                        SET SINGLE_USER
                        WITH ROLLBACK IMMEDIATE
                        RESTORE DATABASE " + databaseName + @"
                        FROM  DISK = N'" + fileAddress + "'";

            return command;
        }

        public static void BackupDatabase(string connectionString, string databaseName)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog
            {
                Title = "Backup File",
                RestoreDirectory = true,
                Filter = "ES-Market Bakup Files (*.esm)|*.esm | SQL Bakup Files (*.bak)|*.bak",
                AddExtension = true
            };
            string backupFilePath = string.Empty;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                backupFilePath = dialog.FileName;
            }
            else
            {
                return;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var query = String.Format("BACKUP DATABASE [{0}] TO DISK='{1}'", databaseName, backupFilePath);

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public static bool CreateDatabaseBackup(string backupFilePath, string dbName, ref string errorMessage)
        {
            if (File.Exists(backupFilePath))
            {
                try
                {
                    File.Delete(backupFilePath);
                }
                catch { }
            }
            else
            {
                File.Create(backupFilePath);
            }



            using (var db = GetDataContext())
            {
                using (var ts = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    try
                    {
                        db.Database.UseTransaction(null);
                        db.Database.ExecuteSqlCommand(System.Data.Entity.TransactionalBehavior.DoNotEnsureTransaction, BackUpCommand(dbName, backupFilePath));
                    }
                    catch (Exception ex)
                    {
                        MessageManager.OnMessage(ex.InnerException != null && ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.Message);

                        return false;
                    }
                    ts.Complete();
                    return true;
                }
            }
        }

        #endregion Backup Restore
        #endregion
    }
}
