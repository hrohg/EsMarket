using System;
using System.Collections.Generic;
using System.Linq;
using ES.Business.Helpers;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class DefaultsManager:BaseManager
    {
        #region public properties
        public static List<EsDefaults> GetDefaults()
        {
            return TryGetEsDefaults();
        }
        public static long? GetDefaultValueLong(string control, long memberId)
        {
            var defaultControl = TryGetEsDefault(control, memberId);
            if (defaultControl == null) return null;
            return defaultControl.ValueInLong;
        }
        public static Guid?  GetDefaultValueGuid(string control, long memberId)
        {
            var defaultControl = TryGetEsDefault(control, memberId);
            if (defaultControl == null) return null;
            return defaultControl.ValueInGuid;
        }

        public static bool SetDefault(string control, long memberId, int? valueInInt, Guid valueInGuid)
        {
            return TrySetDefault(control, memberId, valueInInt, valueInGuid);
        }
        #endregion
        #region private properties
        private static List<EsDefaults> TryGetEsDefaults()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsDefaults.Where(s => s.MemberId == ApplicationManager.Member.Id).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static EsDefaults TryGetEsDefault(string control, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsDefaults.SingleOrDefault(s => s.MemberId == memberId && s.Control == control);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static bool TrySetDefault(string control, long memberId, long? valueInLong, Guid valueInGuid)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var exDefault = db.EsDefaults.SingleOrDefault(s =>  s.Control.ToLower() == control.ToLower() && s.MemberId == memberId);
                    if (exDefault != null)
                    {
                        exDefault.ValueInGuid = valueInGuid;
                        exDefault.ValueInLong = valueInLong;
                    }
                    else
                    {
                        exDefault = new EsDefaults
                        {
                            Id = Guid.NewGuid(),
                            Control = control,
                            ValueInGuid = valueInGuid,
                            ValueInLong = valueInLong,
                            MemberId = memberId
                        };
                        db.EsDefaults.Add(exDefault);
                    }
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
