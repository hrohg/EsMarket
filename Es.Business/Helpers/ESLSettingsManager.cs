using Shared.Helpers;

namespace ES.Business.Helpers
{
	public class ESLSettingsManager : DataManagerBase
	{
	    
	    public enum MemberRoles
	    {
	        Administrator = 1,
            Director = 2,
            Manager = 3,
            Seller =4,
            Storekeeper=5,
            SaleManager = 6,
            SeniorSeller =7
	    }
        //public static ESLSetting GetSettings(bool isOfflineMode = false)
        //{
        //    string connectionString = GetConnectionString(isOfflineMode);
        //    return new DataClassesDataContext(connectionString).RealtorSettings.FirstOrDefault();
        //}

        //public static bool SaveSettings(ESLSetting settingsData)
        //{
        //    EsStockDBEntities db = new EsStockDBEntities();
        //    try
        //    {
        //        var settings = db.RealtorSettings.FirstOrDefault();
        //        if (settings == null)
        //        {
        //            settings = new ESLSetting();
        //            db.RealtorSettings.InsertOnSubmit(settings);
        //        }
        //        settings.AllowBrokersToAddData = settingsData.AllowBrokersToAddData;
        //        settings.DaysBeforeToRentClose = settingsData.DaysBeforeToRentClose;
        //        settings.RatingFrom = settingsData.RatingFrom;
        //        settings.RatingTo = settingsData.RatingTo;
        //        settings.ShowOpenAddressToBrokers = settingsData.ShowOpenAddressToBrokers;
        //        settings.AllowBrokersToPrintEstates = settingsData.AllowBrokersToPrintEstates;
        //        db.SubmitChanges();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //public static List<UserDisplayColumn> GetDisplayColumns(EsUser user, bool isOfflineMode)
        //{
        //    if (user.ID == 0) return new List<UserDisplayColumn>();
        //    string connectionString = GetConnectionString(isOfflineMode);
        //    using (var db = new DataClassesDataContext(connectionString))
        //    {
        //        var columns = db.UserDisplayColumns.Where(s => s.UserID == user.ID).OrderBy(s => s.OrderIndex).ToList();
        //        if (columns.Count != 25)
        //        {
        //            columns = UserDisplayColumn.GetEmptyDisplayColumns();
        //            var userInDB = db.Users.Single(s => s.ID == user.ID);
        //            foreach (UserDisplayColumn col in columns)
        //            {
        //                col.User = userInDB;
        //            }
        //            db.UserDisplayColumns.InsertAllOnSubmit(columns);
        //            db.SubmitChanges();
        //        }
        //        columns.ForEach(s => s.IdName = s.ColumnName);
        //        return columns;
        //    }
        //}

        //public static void SaveDisplayColumns(ObservableCollection<UserDisplayColumn> displayColumns, bool isOfflineMode = false)
        //{
        //    string connectionString = GetConnectionString(isOfflineMode);
        //    using (DataClassesDataContext db = new DataClassesDataContext(connectionString))
        //    {
        //        foreach (UserDisplayColumn column in displayColumns)
        //        {
        //            var columnInDB = db.UserDisplayColumns.Single(s => s.ID == column.ID);
        //            columnInDB.Show = column.Show;
        //            columnInDB.OrderIndex = column.OrderIndex;
        //            db.SubmitChanges();
        //        }
        //    }
        //}
	}
    public class EsUserRoles
    {
        public string RoleName;
        public string Description;
        public int Id;

        public EsUserRoles(string roleName)
        {
            RoleName = roleName;
        }
    }
}
