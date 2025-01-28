using System;
using System.Data;
using System.Data.Common;
using ES.Business.DataUtilities;
using ES.DataAccess.DataUtilities.Exceptions;
using ES.Business.Managers;
using ES.DataAccess.Models;
using SqlConnectionFactory = ES.Business.DataUtilities.SqlConnectionFactory;

namespace ES.Business.Helpers
{
    public class BaseManager : Disposable
    {
        #region Properties and variables
        protected ConnectionContext ContextServer { get; private set; }
        protected ConnectionContext Context { get; private set; }
        private readonly bool _isExternalConnection;
        protected DbProviderFactory ProviderFactoryServer { get; set; }
        protected DbProviderFactory ProviderFactory { get; set; }
        #endregion

        #region Construction
        public BaseManager()
        {
            ContextServer = new ConnectionContext(new SqlConnectionFactory().GetServerConnection());
            ProviderFactoryServer = new SqlConnectionFactory().GetServerFactory();
            Context = new ConnectionContext(new SqlConnectionFactory().GetConnection());
            ProviderFactory = new SqlConnectionFactory().GetFactory();
        }

        public BaseManager(ConnectionContext context)
        {
            Context = context;
            _isExternalConnection = true;
            ProviderFactory = new SqlConnectionFactory().GetFactory();
        }
        #endregion

        #region Methods
        protected static EsStockDbServerEntities GetServerDataContext()
        {
            return new EsStockDbServerEntities(ApplicationManager.Instance.GetServerConnectionString());
        }
        protected static EsStockDBEntities GetDataContext()
        {
            return new EsStockDBEntities(ApplicationManager.Instance.GetConnectionString() ?? ApplicationManager.Instance.GetDefaultConnectionString());
        }
        protected static EsStockDBEntities GetDataContext(string connectionString)
        {
            return new EsStockDBEntities(connectionString ?? ApplicationManager.Instance.GetDefaultConnectionString());
        }

        protected void Execute(Action exec, bool transaction)
        {
            ValidateConnection();
            bool closeConnection = false;
            bool canManageTransaction = false;
            try
            {
                if (Context.Connection.State != ConnectionState.Open)
                {
                    Context.Connection.Open();
                    closeConnection = true;
                }
                canManageTransaction = !Context.IsTransactionStarted;
                if (canManageTransaction && transaction)
                    Context.BeginTransaction();
                exec();
                if (canManageTransaction)
                    Context.CommitTransaction();
            }
            catch (Exception)
            {
                if (Context.IsTransactionStarted && canManageTransaction)
                    Context.RollBackTransaction();
                throw;
            }
            finally
            {
                if (closeConnection)
                    Context.Connection.Close();
            }
        }
        protected T Execute<T>(Func<T> exec, bool transaction)
        {
            ValidateConnection();
            bool closeConnection = false;
            bool canManageTransaction = false;
            T retval = default(T);
            try
            {
                if (Context.Connection.State != ConnectionState.Open)
                {
                    Context.Connection.Open();
                    closeConnection = true;
                }
                canManageTransaction = !Context.IsTransactionStarted;
                if (canManageTransaction && transaction)
                    Context.BeginTransaction();
                retval = exec();
                if (canManageTransaction)
                    Context.CommitTransaction();
            }
            catch (Exception)
            {
                if (Context.IsTransactionStarted && canManageTransaction)
                    Context.RollBackTransaction();
                throw;
            }
            finally
            {
                if (closeConnection)
                    Context.Connection.Close();
            }
            return retval;
        }

        private void ValidateConnection()
        {
            if (Context == null || Context.Connection == null)
                throw new InvalidConnectionException("Connection does not exists");
        }
        #endregion

        #region Disposable implementation
        protected override void DisposeCore()
        {
            if (!_isExternalConnection && Context != null)
                Context.Dispose();
        }
        #endregion
    }
}

