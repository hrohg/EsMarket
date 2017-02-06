using System;
using System.Configuration;
using System.Data.Common;
using ES.DataAccess.Helpers;
using ESL.DataAccess.Helpers;

namespace ES.Business.DataUtilities
{
    public class SqlConnectionFactory : IConnectionFactory
    {

        /// <summary>
        /// Server
        /// </summary>
        private static DbProviderFactory _eslServerProviderFactory;
        private const string EslServerConnectionProvider = "ESLServerConnectionProvider";
        private const string EslServerConnectionString = "ESLServerConnectionString";
        /// <summary>
        /// Local
        /// </summary>
        private static DbProviderFactory _eslProviderFactory;
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
            try
            {
                DbConnection retval = GetFactory().CreateConnection();
                retval.ConnectionString = ConfigurationManager.ConnectionStrings[EslServerConnectionProvider].ConnectionString;
                return retval;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DbProviderFactory GetServerFactory()
        {
            if (_eslServerProviderFactory == null)
                _eslServerProviderFactory = DbProviderFactories.GetFactory(AppSettings.EslServerConnectionProvider);
            return _eslServerProviderFactory;
        }
        /// <summary>
        /// Local
        /// </summary>
        /// <returns></returns>
        public DbConnection GetConnection()
        {
            try
            {
                DbConnection retval = GetFactory().CreateConnection();
                retval.ConnectionString = ConfigurationManager.ConnectionStrings[EslConnectionString].ConnectionString;
                return retval;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public DbProviderFactory GetFactory()
        {
            if (_eslProviderFactory == null)
                _eslProviderFactory = DbProviderFactories.GetFactory(AppSettings.EslConnectionProvider);
            return _eslProviderFactory;
        }
    }
}
