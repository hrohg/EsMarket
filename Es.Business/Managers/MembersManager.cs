using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ES.Business.Helpers;
using ES.Data.Model;
using ES.DataAccess.Models;


namespace ES.Business.Managers
{
    public class MembersManager : BaseManager
    {
        #region Members enums
        #endregion
        #region Es Members
        #region Converters
        public static EsMemberModel Convert(EsMembers item)
        {
            if (item == null) return null;
            var newItem = new EsMemberModel
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.FullName,
                ClubSixteenId = item.ClubSixteenId
            };
            return newItem;
        }
        public static EsMembers Convert(EsMemberModel item)
        {
            if (item == null) return null;
            var newItem = new EsMembers
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.FullName,
                ClubSixteenId = item.ClubSixteenId
            };
            return newItem;
        }
        public static EsMembersAccountsModel Convert(EsMembersAccounts item)
        {
            return item == null ? null : new EsMembersAccountsModel(item.MemberId, item.TotalScores);
        }
        public static EsMembersAccountsModel Convert(EsMembersAccounts item, long memberId)
        {
            return item == null ? new EsMembersAccountsModel(memberId) : new EsMembersAccountsModel(item.MemberId, item.TotalScores);
        }

        #endregion
        #region public methods
        public static bool GetMemberFromServerWithData(long memberId)
        {
            return UpdateManager.GetMemberFromServerWithData(memberId);
        }
        public static List<EsMembers> GetEsMember()
        {
            return TryGetEsMembers();
        }

        public static EsMemberModel GetEsMember(long id)
        {
            return Convert(TryGetEsMember(id));
        }
        public static List<EsMemberModel> GetEsMembers()
        {
            return TryGetEsMembers().Select(Convert).ToList();
        }
        public static List<EsMemberModel> GetEsMembersFromServer()
        {
            return TryGetEsMembersFromServer().Select(Convert).ToList();
        }
        public static List<EsMemberModel> GetMembersByUser(long userId)
        {
            return TryGetMembersByUser(userId).Select(Convert).ToList();
        }
        public static List<MemberUsersRoles> GetMembersUsersRoles(long userId, long memberId)
        {
            return TryGetMembersUsersRoles(userId, memberId);
        }

        public static List<EsMembersAccountsModel> GetMembersAccounts()
        {
            return TryGetEsMembersAccounts().Select(s=>Convert(s)).ToList();
        }
        public static EsMembersAccountsModel GetMembersAccounts(long memberId)
        {
            return Convert(TryGetEsMembersAccounts(memberId), memberId);
        }
        #endregion
        #region private methods
        private static EsMembers TryGetEsMember(long id)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsMembers.SingleOrDefault(s => s.Id == id);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static List<EsMembers> TryGetEsMembers()
        {
            using (var db = GetDataContext())
            {
                return db.EsMembers.ToList();
            }
        }
        private static List<EsMembers> TryGetEsMembersFromServer()
        {
            using (var db = GetServerDataContext())
            {
                return db.EsMembers.ToList();
            }
        }
        private static List<EsMembers> TryGetMembersByUser(long userId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.MemberUsersRoles.Include(s => s.EsMembers).Where(s => s.EsUserId == userId).GroupBy(s => s.MemberId).Select(s => s.FirstOrDefault().EsMembers).ToList(); ;
                }
                catch (Exception ex)
                {
                    return new List<EsMembers>();
                }
            }
        }
        private static List<MemberUsersRoles> TryGetMembersUsersRoles(long userId, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.MemberUsersRoles
                        .Include(s => s.EsMembers)
                        .Include(s => s.EsUsers)
                        .Include(s => s.MembersRoles)
                        .Where(s => s.EsUserId == userId && s.MemberId==memberId).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static List<EsMembersAccounts> TryGetEsMembersAccounts()
        {
            using (var db = GetServerDataContext())
            {
                return db.EsMembersAccounts.ToList();
            }
        }
        private static EsMembersAccounts TryGetEsMembersAccounts(long memberId)
        {
            using (var db = GetServerDataContext())
            {
                return db.EsMembersAccounts.SingleOrDefault(s => s.MemberId == memberId);
            }
        }
        #endregion
        #endregion
    }
}
