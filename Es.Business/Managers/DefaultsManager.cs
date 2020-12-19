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
        public static long? GetDefaultValueLong(string control, int memberId)
        {
            var defaultControl = TryGetEsDefault(control);
            if (defaultControl == null) return null;
            return defaultControl.ValueInLong;
        }
        public static Guid?  GetDefaultValueGuid(string control)
        {
            var defaultControl = TryGetEsDefault(control);
            if (defaultControl == null) return null;
            return defaultControl.ValueInGuid;
        }

        public static bool SetDefault(string control, int? valueInInt, Guid? valueInGuid)
        {
            return TrySetDefault(control, valueInInt, valueInGuid);
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
        private static EsDefaults TryGetEsDefault(string control)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsDefaults.SingleOrDefault(s => s.MemberId == ApplicationManager.Member.Id && s.Control == control);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static bool TrySetDefault(string control, long? valueInLong, Guid? valueInGuid)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var exDefault = db.EsDefaults.SingleOrDefault(s =>  s.Control.ToLower() == control.ToLower() && s.MemberId == ApplicationManager.Member.Id);
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
                            MemberId = ApplicationManager.Member.Id
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
