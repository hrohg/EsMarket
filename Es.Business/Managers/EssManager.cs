using System;
using System.Collections.Generic;

namespace ES.Business.Managers
{
    public enum LicensePlansEnum
    {
        None, Starter, Advanced
    }
    public class LicensePlanModel
    {
        public bool CanRedeemVaucher { get; set; }
    }
    public class EsMarketManager
    {
        private Dictionary<LicensePlansEnum, LicensePlanModel> _licensePlans;
        private static EsMarketManager _instance;
        public static EsMarketManager Instance { get { return _instance ?? (_instance = new EsMarketManager()); } }
        EsMarketManager()
        {
            _licensePlans = new Dictionary<LicensePlansEnum, LicensePlanModel>();
            Initialize();
        }
        private void Initialize()
        {
            _licensePlans.Add(LicensePlansEnum.Starter, new LicensePlanModel { });
            _licensePlans.Add(LicensePlansEnum.Advanced, new LicensePlanModel { CanRedeemVaucher = true }); ;
        }
        public LicensePlanModel GetLicensePlan(LicensePlansEnum license)
        {
            return _licensePlans.ContainsKey(license) ? _licensePlans[license] : new LicensePlanModel();
        }       

        public static LicensePlansEnum GetLicense(int memberId)
        {
            return LicensePlansEnum.Advanced;
        }

        public object GetLicensePlan(object getLicense)
        {
            throw new NotImplementedException();
        }
    }
}
