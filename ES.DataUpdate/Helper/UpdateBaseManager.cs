using System;
using System.Data;
using System.Data.Common;
using ES.DataAccess.DataUtilities;
using ES.DataAccess.DataUtilities.Exceptions;
using ES.DataAccess.Models;

namespace ES.DataUpdate.Helper
{
    public class UpdateBaseManager : Disposable
    {
        #region Properties and variables
        protected ConnectionContext Context { get; private set; }
        private readonly bool _isServerConnection;
        protected DbProviderFactory ProviderFactory { get; set; }
        #endregion

        #region Construction
        public UpdateBaseManager()
        {
            Context = new ConnectionContext(new SqlConnectionFactory().GetConnection());
            ProviderFactory = new SqlConnectionFactory().GetFactory();
        }

        public UpdateBaseManager(ConnectionContext context)
        {
            Context = context;
            _isServerConnection = true;
            ProviderFactory = new SqlConnectionFactory().GetFactory();
        }
        #endregion

        #region Methods
        protected static EsStockDBEntities GetDataContext()
        {
            return new EsStockDBEntities();
        }
        protected static EsStockDbServerEntities GetServerDataContext()
        {
            return new EsStockDbServerEntities();
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
            if (!_isServerConnection && Context != null)
                Context.Dispose();
        }
        #endregion
    }

}
