using System;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace ES.DataAccess.Helpers
{
	public static class DatabaseTools
	{
		public static bool BackupDatabase(String destinationPath, ref string errorMessage)
		{
			try
			{
                const string databaseName = "EsStockDb";
                //var con = new SqlConnection(ConfigurationManager.ConnectionStrings["ESL.DataAccess.Properties.Settings.ESStockDBEntities"].ConnectionString);
                //var con = new EntityConnection().ConnectionString;
                //var con = new SqlConnection(ConfigurationManager.ConnectionStrings["ESL.DataAccess.Properties.Settings.ESStockDBEntities"].ConnectionString);
                var con = @"localhost\mssqlexpress;Initial Catalog=EsStockDB;Integrated Security=True; MultipleActiveResultSets=True;App=EntityFramework&quot;";
                var connection = new ServerConnection(con);
                var sqlServer = new Server(connection);

                var bkpDatabase = new Backup();
                bkpDatabase.Action = BackupActionType.Database;
                bkpDatabase.Database = databaseName;
                var bkpDevice = new BackupDeviceItem(destinationPath, DeviceType.File);

                bkpDatabase.Devices.Add(bkpDevice);
                bkpDatabase.SqlBackup(sqlServer);
                connection.Disconnect();
				return true;
			}
			catch(Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}
		}
        public static bool BackupDatabase(String destinationPath,string con, ref string errorMessage)
        {
            try
            {
                const string databaseName = "EsStockDb";
                var connection = new ServerConnection(con);
                var sqlServer = new Server(connection);

                var bkpDatabase = new Backup();
                bkpDatabase.Action = BackupActionType.Database;
                bkpDatabase.Database = databaseName;
                var bkpDevice = new BackupDeviceItem(destinationPath, DeviceType.File);

                bkpDatabase.Devices.Add(bkpDevice);
                bkpDatabase.SqlBackup(sqlServer);
                connection.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
        public static bool RestoreDatabase(string bakupFilePath, string dataFolderPath, ref string errorMessage)
		{
			SqlConnection curSQLConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["MasterDBConnectionString"].ConnectionString);
			try
			{
                curSQLConnection.Open();
                ServerConnection sc = new ServerConnection(curSQLConnection);
                Server s = new Server(sc);
                Restore r = new Restore();
                r.Action = RestoreActionType.Database;
                r.Database = "REDB";
                r.ReplaceDatabase = true;
                r.Devices.AddDevice(bakupFilePath, DeviceType.File);
                r.RelocateFiles.Add(new RelocateFile("REDB", dataFolderPath + "\\REDB.mdf"));
                r.RelocateFiles.Add(new RelocateFile("REDB_log", dataFolderPath + "\\REDB.ldf"));
                r.SqlRestore(s);
				return true;
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}
			finally
			{
				curSQLConnection.Close();
			}

		}
	}
}
