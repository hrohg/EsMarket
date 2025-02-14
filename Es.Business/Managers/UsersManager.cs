﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ES.Business.Helpers;
using ES.Common.Managers;
using ES.Data.Models;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class UsersManager : BaseManager
    {

        #region EsUsers public methods
        public static EsUserModel GetEsUser(int userId)
        {
            return ConvertEsUser(TryGetUser(userId));
        }
        public static List<EsUserModel> GetEsUsers(int memberId)
        {
            return TryGetUsers(memberId).Select(ConvertEsUser).ToList();
        }
        public static EsUserModel VerifyLogin(string userName, SecureString password)
        {
            IntPtr passwordBSTR = default(IntPtr);
            string insecurePassword = "";
            try
            {
                passwordBSTR = Marshal.SecureStringToBSTR(password);
                insecurePassword = Marshal.PtrToStringBSTR(passwordBSTR);
            }
            catch
            {
                insecurePassword = "";
            }

            // immediately use insecurePassword (in local variable) value after decrypting it:
            return ConvertEsUser(TryVerifyLocalLogin(userName, insecurePassword));
        }
        /// <summary>
        /// Convert EsUser to EsUserModel
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static EsUserModel ConvertEsUser(EsUsers item)
        {
            if (item == null) return null;
            return new EsUserModel(item.UserId)
            {
                UserName = item.UserName,
                Password = item.Password,
                Email = item.Email,
                Mobile = item.Mobile,
                EssClubId = item.EssClubId,
                LastActivityDate = item.LastActivityDate,
                IsActive = item.IsActive
            };
        }
        public static EsUsers ConvertEsUser(EsUserModel item)
        {
            if (item == null) return null;
            return new EsUsers
            {
                UserId = item.UserId,
                UserName = item.UserName,
                Password = item.Password,
                Email = item.Email,
                Mobile = item.Mobile,
                EssClubId = item.EssClubId,
                LastActivityDate = item.LastActivityDate,
                IsActive = item.IsActive
            };
        }

        public static bool ChangePassword(EsUserModel esUserModel)
        {
            if (esUserModel == null || string.IsNullOrEmpty(esUserModel.NewPassword) ||
                esUserModel.NewPassword != esUserModel.ConfirmPassword) return false;
            return TryChangePassword(esUserModel.UserId, esUserModel.Password, esUserModel.NewPassword);
        }

        public static List<MembersRoles> GetUserRoles(int userId, int memberId)
        {
            return TryGetMembersRoles(userId, memberId);
        }
        public static List<MemberRolesModel> GetMemberRoles()
        {
            return TryGetMembersRoles().Select(s => new MemberRolesModel { Id = s.Id, RoleName = s.RoleName, Description = s.Description }).ToList();
        }
        public static List<UsersRolesModel> GetUsersRoles()
        {
            return TryGetMembersUsersRoles().Select(Convert).ToList();
        }
        public static EsUserModel LoadEsUserByEmail(string email)
        {
            return ConvertEsUser(TryLoadUserByEmail(email));
        }
        public static bool RemoveEsUser(int userId)
        {
            return TryRemoveUser(userId);
        }
        public static bool EditUser(EsUserModel user, List<MemberUsersRoles> roles, int memberId)
        {
            return TryEditUser(ConvertEsUser(user), roles, memberId);
        }
        public static bool RemoveUserRole(UsersRolesModel userRole)
        {
            return false;// TryRemoveUserRole(userRole.RoleId, userRole.EsUserId, userRole.MemberId);
        }
        #endregion

        /// <summary>
        /// EsUsers private methods
        /// </summary>
        #region EsUsers private methods
        private static EsUsers TryGetUser(int userId)
        {
            using (var db = GetDataContext())
            {
                return db.EsUsers.SingleOrDefault(s => s.UserId == userId);
            }
        }
        private static List<EsUsers> TryGetUsers()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsUsers.ToList();
                }
                catch (Exception)
                {
                    return new List<EsUsers>();
                }
            }
        }
        private static List<EsUsers> TryGetUsers(int memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.MemberUsersRoles.Where(s => s.MemberId == memberId).Select(s => s.EsUsers).Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<EsUsers>();
                }
            }
        }

        private static EsUsers TryVerifyLocalLogin(string userName, string password)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var encodePassword = EncodePassword(password);
                    var user = db.EsUsers.FirstOrDefault(s =>
                        (s.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase) ||
                         s.Email.Equals(userName, StringComparison.CurrentCultureIgnoreCase)) &&
                        s.IsActive &&
                        s.Password == encodePassword);
                    return user;
                }
                catch (EntityException ex)
                {
                    MessageBox.Show("Սերվերն անհասանելի է:");
                    return new EsUsers { UserId = -1 };
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        private static EsUsers TryVerifyLogin(string userName, string password)
        {
            EsMemberModel member;
            var db = GetDataContext();
            var dbServer = GetServerDataContext();
            {
                try
                {
                    var encodePassword = EncodePassword(password);
                    var user = db.EsUsers.SingleOrDefault(s =>
                        String.Equals(s.UserName, userName, StringComparison.CurrentCultureIgnoreCase) || String.Equals(s.Email, userName, StringComparison.CurrentCultureIgnoreCase));
                    var userOnServer = dbServer.EsUsers.SingleOrDefault(s =>
                        String.Equals(s.UserName, userName, StringComparison.CurrentCultureIgnoreCase) || String.Equals(s.Email, userName, StringComparison.CurrentCultureIgnoreCase));
                    if (userOnServer == null && user == null) { return null; }
                    if (userOnServer == null) { return user.IsActive && user.Password == encodePassword ? user : null; }
                    if (user == null) { return userOnServer.IsActive && userOnServer.Password == encodePassword ? userOnServer : null; }
                    user.IsActive = userOnServer.IsActive;
                    if (user.LastActivityDate < userOnServer.LastActivityDate)
                    {
                        user.LastActivityDate = userOnServer.LastActivityDate;
                        user.Password = userOnServer.Password;
                        db.SaveChanges();
                        return (userOnServer.IsActive && userOnServer.Password == encodePassword) ? user : null;
                    }
                    else
                    {
                        userOnServer.LastActivityDate = user.LastActivityDate;
                        userOnServer.Password = user.Password;
                        dbServer.SaveChanges();
                        return (user.IsActive && user.Password == encodePassword) ? user : null;
                    }

                    //var exUser = user??userOnServer;

                    //        var members = dbServer.MemberUsersRoles.Where(s => s.EsUserId == userOnServer.UserId)
                    //            .GroupBy(s => s.MemberId).Select(s => s.FirstOrDefault().EsMembers).ToList();
                    //       member = SelectItemsManager.SelectEsMembers(members.Select(MembersManager.Convert).ToList(),
                    //                false, "Ընտրել համակարգ տվյալները բեռնելու համար։").FirstOrDefault();
                    //        if (member == null)
                    //        {
                    //            return null;
                    //        }
                    //        else
                    //        {
                    //            DatabaseManager.LoadMember(userOnServer.UserId, member.Id);
                    //            return userOnServer;
                    //        }
                    //{
                    //    user.LastActivityDate = DateTime.Now;
                    //    if (user.Password == password)
                    //    {
                    //        user.Password = EncodePassword(user.Password);
                    //    }
                    //    db.SaveChanges();
                    //    return user;
                    //}

                }
                catch (Exception ex)
                {
                    MessageManager.ShowMessage(ex.Message);
                    return null;
                }

            }

        }
        private static bool TryChangePassword(int userId, string password, string newPassword)
        {
            using (var db = GetDataContext())
            {
                var exUser = db.EsUsers.SingleOrDefault(s => s.UserId == userId);
                if (exUser == null || (exUser.Password != EncodePassword(password) && exUser.Password != password)) return false;
                exUser.Password = EncodePassword(newPassword);
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
        private static string EncodePassword(string password)
        {
            //Declarations
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;

            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
            md5 = new MD5CryptoServiceProvider();
            originalBytes = Encoding.Default.GetBytes(password);
            encodedBytes = md5.ComputeHash(originalBytes);

            //Convert encoded bytes back to a 'readable' string
            return BitConverter.ToString(encodedBytes);
        }

        private static List<MembersRoles> TryGetMembersRoles(int userId, int memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return
                        db.MemberUsersRoles.Where(s => s.EsUserId == userId && s.MemberId == memberId)
                            .GroupBy(s => s.MemberRoleId)
                            .Select(s => s.Select(t => t.MembersRoles).FirstOrDefault())
                            .ToList();
                }
                catch
                {
                    return new List<MembersRoles>();
                }
            }
        }
        private static List<MembersRoles> TryGetMembersRoles()
        {
            using (var db = GetDataContext())
            {
                return db.MembersRoles.ToList();
            }
        }
        private static List<MembersRoles> TryGetUsersRoles(int userId, int memberId)
        {
            using (var db = GetDataContext())
            {
                return db.MemberUsersRoles.Where(s => s.EsUserId == userId && s.MemberId == memberId).GroupBy(s => s.MemberRoleId).Select(s => s.Select(t => t.MembersRoles).FirstOrDefault()).ToList();
            }
        }
        private static List<MemberUsersRoles> TryGetMembersUsersRoles()
        {
            var memberId = ApplicationManager.Instance.GetMember.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.MemberUsersRoles.Where(s => s.MemberId == memberId)
                                        .Include(s => s.EsUsers)
                                        .Include(s => s.MembersRoles)
                                        .ToList();
                }
                catch (Exception)
                {
                    return new List<MemberUsersRoles>();
                }

            }
        }
        private static EsUsers TryLoadUserByEmail(string email)
        {
            EsUsers serverUser, user;
            using (var serverDb = GetServerDataContext())
            {
                serverUser = serverDb.EsUsers.FirstOrDefault(s => s.Email.ToLower() == email.ToLower());
                if (serverUser == null) return null;
            }
            using (var db = GetDataContext())
            {
                user = db.EsUsers.SingleOrDefault(s => s.UserId == serverUser.UserId);
                if (user != null)
                {
                    user.UserId = serverUser.UserId;
                    user.UserName = serverUser.UserName;
                    user.Password = serverUser.Password;
                    user.Email = serverUser.Email;
                    user.Mobile = serverUser.Mobile;
                    user.LastActivityDate = serverUser.LastActivityDate;
                    user.IsActive = serverUser.IsActive;
                    user.EssClubId = serverUser.EssClubId;
                }
                else
                {
                    user = new EsUsers
                    {
                        UserId = serverUser.UserId,
                        UserName = serverUser.UserName,
                        Password = serverUser.Password,
                        Email = serverUser.Email,
                        Mobile = serverUser.Mobile,
                        LastActivityDate = serverUser.LastActivityDate,
                        IsActive = serverUser.IsActive,
                        EssClubId = serverUser.EssClubId
                    };
                    db.EsUsers.Add(user);
                }
                db.SaveChanges();
            }
            return user;
        }
        private static bool TryRemoveUser(int userId)
        {
            using (var db = GetDataContext())
            {
                var exUser = db.EsUsers.SingleOrDefault(s => s.UserId == userId);
                if (exUser == null)
                {
                    return false;
                }
                else
                {
                    //exUser.IsActive = false;
                    var roles = db.MemberUsersRoles.Where(s => s.EsUserId == exUser.UserId && s.MemberId == ApplicationManager.Member.Id);
                    foreach (var memberUsersRolese in roles)
                    {
                        db.MemberUsersRoles.Remove(memberUsersRolese);
                    }
                }
                db.SaveChanges();
            }
            return true;
        }
        private static bool TryRemoveUserRole(int roleId, int userId, int memberId)
        {
            using (var db = GetDataContext())
            {
                var roles = db.MemberUsersRoles.Where(s => s.MemberId == memberId && s.EsUserId == userId && s.MemberRoleId == roleId);
                foreach (var role in roles)
                {
                    db.MemberUsersRoles.Remove(role);
                }
                db.SaveChanges();
            }
            return true;
        }
        private static bool TryEditUser(EsUsers user, List<MemberUsersRoles> roles, int memberId)
        {
            using (var db = GetDataContext())
            {
                var exUser = db.EsUsers.SingleOrDefault(s => s.UserId == user.UserId);
                if (exUser != null)
                {
                    exUser.Mobile = user.Mobile;
                }
                else
                {
                    db.EsUsers.Add(user);
                    db.SaveChanges();
                }

                var exRolesInDb = db.MemberUsersRoles.Where(s => s.EsUserId == user.UserId && s.MemberId == memberId);
                foreach (var role in exRolesInDb)
                {
                    var roleInDb = role;
                    var exRoles = roles.Where(s => s.MemberRoleId == roleInDb.MemberRoleId && s.MemberId == roleInDb.MemberId && s.EsUserId == roleInDb.EsUserId).ToList();
                    if (exRoles.Any())
                    {
                        foreach (var item in exRoles)
                        {
                            roles.Remove(item);
                        }
                    }
                    else
                    {
                        db.MemberUsersRoles.Remove(role);
                    }
                }
                foreach (var role in roles)
                {
                    db.MemberUsersRoles.Add(role);
                }
                db.SaveChanges();
            }
            return true;
        }
        #endregion
        #region Convertors

        private static UsersRolesModel Convert(MemberUsersRoles item)
        {
            if (item == null) return null;
            return new UsersRolesModel
            {
                Id = item.Id,
                EsUser = ConvertEsUser(item.EsUsers),
                Role = new MemberRolesModel
                {
                    Id = item.MembersRoles.Id,
                    RoleName = item.MembersRoles.RoleName,
                    Description = item.MembersRoles.Description
                }
            };
        }
        #endregion //Convertors
        public static List<ESUserRoles> GetEsUserRoles()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ESUserRoles.ToList();
                }
                catch (Exception)
                {
                    return null;
                }

            }
        }
        public static List<EsUsers> GetEsUsers()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsUsers.ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static EsUsers LoginUser(string login, string pass)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsUsers.FirstOrDefault(s => (s.UserName.ToLower() == login.ToLower() || s.Email.ToLower() == login.ToLower()) && s.Password == pass);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public EsUsers EditUser(EsUsers item)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var exItem = db.EsUsers.FirstOrDefault(s => s.UserId == item.UserId);
                    if (exItem != null)
                    {
                        exItem.UserName = item.UserName;
                        exItem.Password = item.Password;
                        exItem.Email = item.Email;
                        exItem.Mobile = item.Mobile;
                        exItem.LastActivityDate = DateTime.Now;
                    }
                    else
                    {
                        db.EsUsers.Add(item);
                    }

                    db.SaveChanges();
                    return item;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private static string GenerateSaltValue()
        {
            UnicodeEncoding utf16 = new UnicodeEncoding();

            if (utf16 != null)
            {
                // Create a random number object seeded from the value
                // of the last random seed value. This is done
                // interlocked because it is a static value and we want
                // it to roll forward safely.

                Random random = new Random(unchecked((int)DateTime.Now.Ticks));

                if (random != null)
                {
                    // Create an array of random values.

                    byte[] saltValue = new byte[SaltValueSize];

                    random.NextBytes(saltValue);

                    // Convert the salt value to a string. Note that the resulting string
                    // will still be an array of binary values and not a printable string. 
                    // Also it does not convert each byte to a double byte.

                    string saltValueString = utf16.GetString(saltValue);

                    // Return the salt value as a string.

                    return saltValueString;
                }
            }

            return null;
        }

        private const int SaltValueSize = 256;

        private static string HashPassword(string clearData, string saltValue, HashAlgorithm hash)
        {
            UnicodeEncoding encoding = new UnicodeEncoding();

            if (clearData != null && hash != null && encoding != null)
            {
                // If the salt string is null or the length is invalid then
                // create a new valid salt value.

                if (saltValue == null)
                {
                    // Generate a salt string.
                    saltValue = GenerateSaltValue();
                }

                // Convert the salt string and the password string to a single
                // array of bytes. Note that the password string is Unicode and
                // therefore may or may not have a zero in every other byte.

                byte[] binarySaltValue = new byte[SaltValueSize];

                binarySaltValue[0] = byte.Parse(saltValue.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat);
                binarySaltValue[1] = byte.Parse(saltValue.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat);
                binarySaltValue[2] = byte.Parse(saltValue.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat);
                binarySaltValue[3] = byte.Parse(saltValue.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat);

                byte[] valueToHash = new byte[SaltValueSize + encoding.GetByteCount(clearData)];
                byte[] binaryPassword = encoding.GetBytes(clearData);

                // Copy the salt value and the password to the hash buffer.

                binarySaltValue.CopyTo(valueToHash, 0);
                binaryPassword.CopyTo(valueToHash, SaltValueSize);

                byte[] hashValue = hash.ComputeHash(valueToHash);

                // The hashed password is the salt plus the hash value (as a string).

                string hashedPassword = saltValue;

                foreach (byte hexdigit in hashValue)
                {
                    hashedPassword += hexdigit.ToString("X2", CultureInfo.InvariantCulture.NumberFormat);
                }

                // Return the hashed password as a string.

                return hashedPassword;
            }
            return null;
        }


    }
}
