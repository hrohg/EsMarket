using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ES.Common.Helpers
{
    public static class StringHelper
    {
        public static SecureString ToSecureString(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;
            SecureString result = new SecureString();
            foreach (char c in source)
                result.AppendChar(c);
            return result;
        }

        public static string ToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null) return string.Empty;
                
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
