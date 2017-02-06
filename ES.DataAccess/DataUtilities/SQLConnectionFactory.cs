using System.Configuration;
using System.Data.Common;
using ES.DataAccess.Helpers;
using ESL.DataAccess.Helpers;

namespace ES.DataAccess.DataUtilities
{
    public class SqlConnectionFactory : IConnectionFactory
    {
        private static DbProviderFactory _eslServerProviderFactory;
        private static DbProviderFactory _eslProviderFactory;
        /// <summary>
        /// Connection string for Server
        /// </summary>
        private const string EslServerConnectionProvider = "ESLServerConnectionProvider";
        private const string EslServerConnectionString = "ESLServerConnectionString";
        /// <summary>
        /// Connection string for Local
        /// </summary>
        private const string EslConnectionProvider = "ESLConnectionProvider";
        private const string EslConnectionString = "ESLConnectionString";

        static SqlConnectionFactory()
        {
            _eslServerProviderFactory = DbProviderFactories.GetFactory(AppSettings.EslServerConnectionProvider);
            _eslProviderFactory = DbProviderFactories.GetFactory(AppSettings.EslConnectionProvider);
        }
        /// <summary>
        /// Server
        /// </summary>
        /// <returns></returns>
        public DbConnection GetServerConnection()
        {
            DbConnection retval = GetFactory().CreateConnection();
            retval.ConnectionString = ConfigurationManager.ConnectionStrings[EslConnectionString].ConnectionString;
            return retval;
        }
        /// <summary>
        /// Local
        /// </summary>
        /// <returns></returns>
        public DbConnection GetConnection()
        {
            DbConnection retval = GetFactory().CreateConnection();
            retval.ConnectionString = ConfigurationManager.ConnectionStrings[EslConnectionString].ConnectionString;
            return retval;
        }
        /// <summary>
        /// Server
        /// </summary>
        /// <returns></returns>
        public DbProviderFactory GetServerFactory()
        {
            return _eslServerProviderFactory ?? (_eslServerProviderFactory = DbProviderFactories.GetFactory(AppSettings.EslServerConnectionProvider));
        }

        /// <summary>
        /// Local
        /// </summary>
        /// <returns></returns>
        public DbProviderFactory GetFactory()
        {
            if (_eslProviderFactory == null)
                _eslProviderFactory = DbProviderFactories.GetFactory(AppSettings.EslConnectionProvider);
            return _eslProviderFactory;
        }
    }
}
