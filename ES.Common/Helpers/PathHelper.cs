using System;
using System.IO;
using System.Reflection;

namespace ES.Common.Helpers
{
    public class PathHelper
    {
        public static string GetLocalAppDataPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),Constants.AppName);
            if (Directory.Exists(path)) return path;
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return path;
        }
        public static string GetTempAppPath()
        {
            var path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Constants.AppName);
            if (Directory.Exists(path)) return path;
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return path;
        }
        public static string GetMemberSettingsFilePath(long memberId)
        {
            var appDataPath = GetLocalAppDataPath();
            return string.Format(@"{0}\{1}_{2}.{3}", appDataPath, Constants.MemberSettingsDataPath, memberId, Constants.ConfigFileExtantion);

        }
        public static string GetMemberTempInvoicePath(long memberId)
        {
            var appTempPath = GetTempAppPath();
            if (string.IsNullOrEmpty(appTempPath) || !Directory.Exists(appTempPath)) return null;
            var path = string.Format(@"{0}\AutoSave\{1}", appTempPath, memberId);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;

        }
        public static string GetMemberTempInvoiceFilePath(Guid invoiceId, long memberId)
        {
            var appDataPath = GetLocalAppDataPath();
            return string.Format(@"{0}\_${1}.{2}", GetMemberTempInvoicePath(memberId), invoiceId, Constants.DataFileExtantion);

        }
        public static string GetGeneralSettingsFilePath()
        {
            var appDataPath = GetLocalAppDataPath();
            return string.Format(@"{0}\{1}.{2}", appDataPath, Constants.GeneralSettingsDataPath, Constants.ConfigFileExtantion);

        }
        public static string GetDataServerSettingsFilePath()
        {
            var appDataPath = GetLocalAppDataPath();
            return string.Format(@"{0}\{1}.{2}", appDataPath, Constants.DataServerSettingsDataPath, Constants.ConfigFileExtantion);

        }
        public static string ApplicationExecutablePath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
            }
        }
    }
}
