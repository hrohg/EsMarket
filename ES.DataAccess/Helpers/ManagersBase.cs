using System;
using System.Data;
using System.Data.Common;
using ES.DataAccess.DataUtilities;
using ES.DataAccess.DataUtilities.Exceptions;
using ES.DataAccess.Models;

namespace ES.DataAccess.Helpers
{
    public class ManagerBase: Disposable
    {
        #region Properties and variables
        protected ConnectionContext Context { get; private set; }

        private bool isExternalConnection;

        protected DbProviderFactory ProviderFactory { get; set; }
        #endregion

        #region Construction
        public ManagerBase()
        {
            Context = new ConnectionContext(new SqlConnectionFactory().GetConnection());
            ProviderFactory = new SqlConnectionFactory().GetFactory();
        }

        public ManagerBase(ConnectionContext context)
        {
            Context = context;
            isExternalConnection = true;
            ProviderFactory = new SqlConnectionFactory().GetFactory();
        }
        #endregion

        #region Methods
        protected static EsStockDBEntities CreateDataContext()
        {
            return new EsStockDBEntities();
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
            if (!isExternalConnection && Context != null)
                Context.Dispose();
        }
        #endregion
    }
    }

