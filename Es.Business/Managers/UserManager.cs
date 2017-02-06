using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ESL.DataAccess.Helpers;
using ESL.Common;
using ESL.DataAccess.Models;

namespace ESL.Business.Mangers
{
	public class UserManager : DataManagerBase
    {
        
        public static bool AddUser(EsUsers user)
		{
			try
			{
				var db = new EsStockDBEntities();
				user.LastModifiedDate = DateTime.Now;
				db.Users.InsertOnSubmit(user);
				db.SubmitChanges();
				if (!user.OriginalID.HasValue)
				{
					user.OriginalID = user.ID;
					db.SubmitChanges();
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static ObservableCollection<EsUser> GetUsers(bool isOfflineMode)
		{
            return new ObservableCollection<EsUser>(new DataClassesDataContext(GetConnectionString(isOfflineMode)).Users.ToList());

            //return new ObservableCollection<EsUser>(new DataClassesDataContext(GetConnectionString(isOfflineMode)).Users.Where(s => s.IsDeleted == null || s.IsDeleted == false).ToList());
        }

		public static bool UpdateUser(EsUser user)
		{
			try
			{
				DataClassesDataContext db = new DataClassesDataContext();
				var userInDb = db.Users.First(s => s.ID == user.ID);
				db.BrokersRegions.DeleteAllOnSubmit(db.BrokersRegions.Where(s => s.BrokerID == userInDb.ID));
				db.BrokerEstateTypes.DeleteAllOnSubmit(db.BrokerEstateTypes.Where(s => s.BrokerID == userInDb.ID));
				db.BrokerOrderTypes.DeleteAllOnSubmit(db.BrokerOrderTypes.Where(s => s.BrokerID == user.ID));
				db.BrokerStates.DeleteAllOnSubmit(db.BrokerStates.Where(s => s.BrokerID == user.ID));
				db.SubmitChanges();
				if (user.IsDeleted == null || user.IsDeleted == false)
				{
                    //foreach (BrokersRegion brokersRegion in user.BrokersRegions)
                    //{
                    //    userInDB.BrokersRegions.Add(new BrokersRegion { Region = db.Regions.First(s => s.RegionID == brokersRegion.RegionID) });
                    //}
                    //foreach (var estateType in user.BrokerEstateTypes)
                    //{
                    //    userInDB.BrokerEstateTypes.Add(new BrokerEstateType { EstateType = db.EstateTypes.First(s => s.EstateTypeID == estateType.EstateTypeID) });
                    //}
                    //foreach (var orderType in user.BrokerOrderTypes)
                    //{
                    //    userInDB.BrokerOrderTypes.Add(new BrokerOrderType { OrderType = db.OrderTypes.First(s => s.OrderTypeID == orderType.OrderTypeID) });
                    //}
                    //foreach (var state in user.BrokerStates)
                    //{
                    //    userInDB.BrokerStates.Add(new BrokerState { State = db.States.First(s => s.ID == state.StateID) });
                    //}
				}
				CopyProperties(userInDb, user);
				userInDb.LastModifiedDate = DateTime.Now;
				db.SubmitChanges();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static void CopyProperties(EsUser userInDB, EsUser user)
		{
			userInDB.Email = user.Email;
			userInDB.Name = user.Name;
			userInDB.RoleID = user.RoleID;
			userInDB.MobileAlias = user.MobileAlias;
			userInDB.Telephone2 = user.Telephone2;
			userInDB.UserName = user.UserName;
			userInDB.Password = user.Password;
			userInDB.IsDeleted = user.IsDeleted;
		}

		public static EsUser Login(string userName, string password, bool isOfflineMode)
		{
			try
			{
				DataClassesDataContext db = new DataClassesDataContext(GetConnectionString(isOfflineMode));
				return db.Users.FirstOrDefault(s => s.UserName == userName.ToLower() && s.Password == password && (s.IsDeleted == null || s.IsDeleted == false));
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static List<EsUsers> GetBrokers(bool isOfflineMode)
		{
			try
			{
				DataClassesDataContext db = new DataClassesDataContext(GetConnectionString(isOfflineMode));
				return db.Users.Where(s => s.RoleID == (int)EslRoles.Manager && (s.IsDeleted == null || s.IsDeleted == false)).ToList();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static bool DeleteUser(EsUsers user)
		{
			if (user == null) return false;
			user.IsDeleted = true;
			return UpdateUser(user);
		}

		public static EsUsers GetUserByID(int id, bool isOfflineMode)
		{
			try
			{
				db = new DataClassesDataContext(GetConnectionString(isOfflineMode));
				return db.Users.FirstOrDefault(s => s.ID == id);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static bool ValidateUserName(string userName)
		{
			DataClassesDataContext db = new DataClassesDataContext();
			try
			{
				var userNames = db.Users.Where(s => s.UserName == userName && (s.IsDeleted == null && s.IsDeleted == false)).Select(s => s.UserName);
				return userNames.Count() == 0;
			}
			catch (Exception)
			{
				return true;
			}
		}

		public static List<EsUser> GetUsersByRights(EsUser user, bool isOfflineMode)
		{
			DataClassesDataContext db = new DataClassesDataContext(GetConnectionString(isOfflineMode));
			var users = db.Users.Where(s => s.IsDeleted == null || s.IsDeleted == false);
			if (!user.IsDirector)
			{
				users = users.Where(s => s.RoleID != (int)EslRoles.Operator);
			}
			else
			{
				return users.ToList();
			}
			if (!user.IsAdmin)
			{
				users = users.Where(s => s.RoleID != (int)EslRoles.Admin);
			}
			else
			{
				return users.ToList();
			}
			return users.ToList();
		}

		public static DateTime GetLastChangeDate()
		{
			using (var db = new DataClassesDataContext(LocalConnectionString))
			{
				var item = db.Users.OrderByDescending(s => s.LastModifiedDate).FirstOrDefault();
				if (item == null) return DateTime.MinValue;
				return item.LastModifiedDate ?? DateTime.MinValue;
			}
		}

		public static List<EsUser> GetNewUsers(DateTime lastChangeDate)
		{
			if (lastChangeDate < Constants.SqlAcceptedMinDateValue)
			{
				lastChangeDate = Constants.SqlAcceptedMinDateValue;
			}
			using (var db = new DataClassesDataContext())
			{
				var users = db.Users.Where(s => s.LastModifiedDate > lastChangeDate).ToList();
				foreach (EsUser user in users)
				{
                    //user.EstateTypes = user.BrokerEstateTypes.ToList();
                    //user.OrderTypes = user.BrokerOrderTypes.ToList();
                    //user.Regions = user.BrokersRegions.ToList();
                    //user.States = user.BrokerStates.ToList();
				}
				return users;
			}
		}

		public static void SynchronizeUsers(List<EsUser> users)
		{
			if (users == null || users.Count == 0) return;
			using (var db = new DataClassesDataContext(LocalConnectionString))
			{
				foreach (EsUser item in users)
				{
					bool isNew = false;
					var itemInDB = db.Users.FirstOrDefault(s => s.OriginalID == item.ID);
					if (itemInDB == null)
					{
						isNew = true;
						itemInDB = new EsUser();
						itemInDB.OriginalID = item.ID;
					}
					else
					{
						db.BrokersRegions.DeleteAllOnSubmit(db.BrokersRegions.Where(s => s.BrokerID == itemInDB.ID));
						db.BrokerEstateTypes.DeleteAllOnSubmit(db.BrokerEstateTypes.Where(s => s.BrokerID == itemInDB.ID));
						db.BrokerOrderTypes.DeleteAllOnSubmit(db.BrokerOrderTypes.Where(s => s.BrokerID == itemInDB.ID));
						db.BrokerStates.DeleteAllOnSubmit(db.BrokerStates.Where(s => s.BrokerID == itemInDB.ID));
						db.SubmitChanges();
					}
					CopyProperties(itemInDB, item);
					itemInDB.LastModifiedDate = item.LastModifiedDate;
					if (item.IsDeleted == null || item.IsDeleted == false)
					{

                        //if (item.BrokerStates != null && item.BrokerStates.Count > 0)
                        //{

                        //    itemInDB.BrokerStates.AddRange(item.BrokerStates.Select(e => new BrokerState
                        //                                                                    {
                        //                                                                        StateID = db.States.First(s => s.OriginalID == e.StateID).ID
                        //                                                                    }));
                        //}
                        //if (item.BrokersRegions != null && item.BrokersRegions.Count > 0)
                        //{
                        //    itemInDB.BrokersRegions.AddRange(item.BrokersRegions.Select(s => new BrokersRegion
                        //                                                                        {
                        //                                                                            RegionID = db.Regions.First(e => e.OriginalID == s.RegionID).RegionID
                        //                                                                        }));
                        //}

                        //if (item.BrokerEstateTypes != null && item.BrokerEstateTypes.Count > 0)
                        //{
                        //    itemInDB.BrokerEstateTypes.AddRange(item.BrokerEstateTypes.Select(s => new BrokerEstateType
                        //                                                                            {
                        //                                                                                EstateTypeID = s.EstateTypeID
                        //                                                                            }));
                        //}
                        //if (item.BrokerOrderTypes != null && item.BrokerOrderTypes.Count > 0)
                        //{
                        //    itemInDB.BrokerOrderTypes.AddRange(item.BrokerOrderTypes.Select(s => new BrokerOrderType
                        //                                                                            {
                        //                                                                                OrderTypeID = s.OrderTypeID
                        //                                                                            }));
                        //}
					}
					if (isNew)
					{
						db.Users.InsertOnSubmit(itemInDB);
					}
					db.SubmitChanges();
				}
			}
		}
	}
}

