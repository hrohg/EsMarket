using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ES.DataAccess.DataUtilities;
using DbIncompatibleTypeException = ES.Business.DataUtilities.Exceptions.DbIncompatibleTypeException;
using InvalidConnectionException = ES.Business.DataUtilities.Exceptions.InvalidConnectionException;

namespace ES.Business.DataUtilities
{
    public class ConnectionContext : IDisposable
    {
        #region Properties and variables
        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }
        public bool IsTransactionStarted
        {
            get { return Transaction != null; }
        }
        private static Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>();
        private DbProviderFactory ProviderFactory { get; set; }
        #endregion

        #region Construction
        public ConnectionContext(DbConnection connection)
        {
            Connection = connection;
            ProviderFactory = new SqlConnectionFactory().GetFactory();
        }

        static ConnectionContext()
        {
            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime2;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(byte[])] = DbType.Binary;
            typeMap[typeof(byte?)] = DbType.Byte;
            typeMap[typeof(sbyte?)] = DbType.SByte;
            typeMap[typeof(short?)] = DbType.Int16;
            typeMap[typeof(ushort?)] = DbType.UInt16;
            typeMap[typeof(int?)] = DbType.Int32;
            typeMap[typeof(uint?)] = DbType.UInt32;
            typeMap[typeof(long?)] = DbType.Int64;
            typeMap[typeof(ulong?)] = DbType.UInt64;
            typeMap[typeof(float?)] = DbType.Single;
            typeMap[typeof(double?)] = DbType.Double;
            typeMap[typeof(decimal?)] = DbType.Decimal;
            typeMap[typeof(bool?)] = DbType.Boolean;
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            typeMap[typeof(Guid?)] = DbType.Guid;
            typeMap[typeof(DateTime?)] = DbType.DateTime2;
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            typeMap[typeof(System.Data.Linq.Binary)] = DbType.Binary;
        }
        #endregion

        #region Methods
        public void BeginTransaction()
        {
            if (Connection == null)
                throw new InvalidConnectionException("Connection does not exists");
            if (Transaction != null)
                return;
            Transaction = Connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (Transaction == null)
                return;
            try
            {
                Transaction.Commit();
            }
            finally
            {
                Transaction = null;
            }
        }

        public void RollBackTransaction()
        {
            if (Transaction == null)
                return;
            try
            {
                Transaction.Rollback();
            }
            finally
            {
                Transaction = null;
            }
        }

        public List<T> GetList<T>(string commandText, System.Data.CommandType cmdType, params DbParameter[] parameters)
        {
            List<T> retval = new List<T>();
            DbProviderFactory factory = new SqlConnectionFactory().GetFactory();
            DbCommand cmd = factory.CreateCommand();
            cmd.CommandText = commandText;
            cmd.Connection = Connection;
            cmd.CommandType = cmdType;
            if (IsTransactionStarted)
                cmd.Transaction = Transaction;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.Clear();
                foreach (var item in parameters)
                {
                    DbParameter par = factory.CreateParameter();
                    par.ParameterName = item.ParameterName;
                    par.Value = item.Value == null ? DBNull.Value : item.Value;
                    par.DbType = item.DbType;
                    cmd.Parameters.Add(par);
                }
            }
            bool isopen = (Connection.State == ConnectionState.Open);
            if (!isopen)
                Connection.Open();
            try
            {
                using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    retval = DataTranslator.Convert<T>(reader);
                }
            }
            finally
            {
                if (!isopen)
                    Connection.Close();
            }
            return retval;
        }

        public int Execute(string commandText, System.Data.CommandType cmdType, params DbParameter[] parameters)
        {
            int retval;
            DbProviderFactory factory = new SqlConnectionFactory().GetFactory();
            DbCommand cmd = factory.CreateCommand();
            cmd.CommandText = commandText;
            cmd.Connection = Connection;
            cmd.CommandType = cmdType;
            if (IsTransactionStarted)
                cmd.Transaction = Transaction;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.Clear();
                foreach (var item in parameters)
                {
                    DbParameter par = factory.CreateParameter();
                    par.ParameterName = item.ParameterName;
                    par.Value = item.Value == null ? DBNull.Value : item.Value;
                    par.DbType = item.DbType;
                    cmd.Parameters.Add(par);
                }
            }
            bool isopen = (Connection.State == ConnectionState.Open);
            if (!isopen)
                Connection.Open();
            try
            {
                retval = cmd.ExecuteNonQuery();
            }
            finally
            {
                if (!isopen)
                    Connection.Close();
            }
            return retval;
        }

        public DataSet GetDataset(string commandText, System.Data.CommandType cmdType, params DbParameter[] parameters)
        {
            DbProviderFactory factory = new SqlConnectionFactory().GetFactory();
            DbCommand cmd = factory.CreateCommand();
            cmd.CommandText = commandText;
            cmd.Connection = Connection;
            cmd.CommandType = cmdType;
            if (IsTransactionStarted)
                cmd.Transaction = Transaction;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.Clear();
                foreach (var item in parameters)
                {
                    DbParameter par = factory.CreateParameter();
                    par.ParameterName = item.ParameterName;
                    par.Value = item.Value == null ? DBNull.Value : item.Value;
                    par.DbType = item.DbType;
                    cmd.Parameters.Add(par);
                }
            }
            bool isopen = (Connection.State == ConnectionState.Open);
            if (!isopen)
                Connection.Open();
            DataSet retval = new DataSet();
            try
            {
                DbDataAdapter da = factory.CreateDataAdapter();
                da.SelectCommand = cmd;
                DbCommandBuilder cmb = factory.CreateCommandBuilder();
                cmb.DataAdapter = da;
                da.Fill(retval);
            }
            finally
            {
                if (!isopen)
                    Connection.Close();
            }
            return retval;
        }

        public void AddParameter<T>(string name, object val, List<DbParameter> parameters)
        {
            parameters.Add(CreateParameter<T>(name, val));
        }

        public DbParameter CreateParameter<T>(string name, object val)
        {
            Type tp = typeof(T);
            if (!typeMap.ContainsKey(tp))
                throw new DbIncompatibleTypeException();
            DbParameter param;
            param = ProviderFactory.CreateParameter();
            param.ParameterName = name;
            param.Value = val == null ? DBNull.Value : val;
            param.DbType = typeMap[tp];
            return param;
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            if (Connection != null)
                Connection.Dispose();
        }
        #endregion
    }
}
