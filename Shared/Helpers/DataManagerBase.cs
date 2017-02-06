using System.Configuration;

namespace Shared.Helpers
{
	public class DataManagerBase
	{
		private static string _localConnectionString;
		protected static string LocalConnectionString
		{
			get
			{
				if(string.IsNullOrEmpty(_localConnectionString))
				{
					_localConnectionString = ConfigurationManager.ConnectionStrings["LocalConnectionString"].ConnectionString;
				}
				return _localConnectionString;
			}
		}

		private static string _eslConnectionString;
		protected static string EsStockConnectionString
		{
			get
			{
				if(string.IsNullOrEmpty(_eslConnectionString))
				{
					_eslConnectionString = ConfigurationManager.ConnectionStrings["ESL.DataAccess.Properties.Settings.ESStockConnectionString"].ConnectionString;
				}
				return _eslConnectionString;
			}
		}

		protected static string GetConnectionString(bool isOfflineMode)
		{
			return isOfflineMode ? LocalConnectionString : EsStockConnectionString;
		}
	}
}
