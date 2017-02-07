using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Shared.Helpers;

namespace LocalizationResources
{
	public class StringEncription
	{
		public const string DateFormat = "dd.MM.yyyy";

		public static string EncryptString(string Message, string Passphrase)
		{
			byte[] Results;
			System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
			MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
			byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));
			TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
			TDESAlgorithm.Key = TDESKey;
			TDESAlgorithm.Mode = CipherMode.ECB;
			TDESAlgorithm.Padding = PaddingMode.PKCS7;
			byte[] DataToEncrypt = UTF8.GetBytes(Message);
			try
			{
				ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
				Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
			}
			finally
			{
				TDESAlgorithm.Clear();
				HashProvider.Clear();
			}
			return Convert.ToBase64String(Results);
		}

		public static string DecryptString(string Message, string Passphrase)
		{
			byte[] Results;
			System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
			MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
			byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));
			TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
			TDESAlgorithm.Key = TDESKey;
			TDESAlgorithm.Mode = CipherMode.ECB;
			TDESAlgorithm.Padding = PaddingMode.PKCS7;
			byte[] DataToDecrypt = Convert.FromBase64String(Message);
			try
			{
				ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
				Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
			}
			catch (Exception ex)
			{
				Results = null;
				var v = ex.ToString();
			}
			finally
			{
				// Clear the TripleDes and Hashprovider services of any sensitive information
				TDESAlgorithm.Clear();
				HashProvider.Clear();
			}


			return UTF8.GetString(Results);
		}

		public static string GetEncryptedDate(DateTime currentDate)
		{
			string stringToEncript = string.Format("{0}+{1}", currentDate.ToString(DateFormat), SecurityObject.EncriptKeyPartDate);
			return EncryptString(stringToEncript, SecurityObject.DateEncryptPassword);
		}

		public static DateTime GetDecryptedDate(string stringToDecrypt)
		{
			string decriptedString = DecryptString(stringToDecrypt, SecurityObject.DateEncryptPassword);
			var date = decriptedString.Substring(0, decriptedString.IndexOf('+'));
			return DateTime.ParseExact(date, DateFormat, null);
		}

		public static DateTime? GetDecryptedExpirationDate(string expirationDateCode)
		{
			string decriptedString = DecryptString(expirationDateCode, SecurityObject.DateEncryptPassword);
			var date = decriptedString.Substring(0, 10);
			var code = decriptedString.Substring(10);
			if (code != SecurityObject.EncriptKeyPartDate)
			{
				return null;
			}
			DateTime dt;
			if (DateTime.TryParseExact(date, DateFormat, null, System.Globalization.DateTimeStyles.None, out dt))
			{
				return dt;
			}
			return null;
		}

		public static string GetEncryptedExpirationDate(DateTime expirationDate)
		{
			string stringToEncript = string.Format("{0}{1}", expirationDate.ToString(DateFormat), SecurityObject.EncriptKeyPartDate);
			return EncryptString(stringToEncript, SecurityObject.DateEncryptPassword);
		}

		// Call this function to remove the key from memory after use for security
		[System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
		public static extern bool ZeroMemory(IntPtr Destination, int Length);

		// Function to Generate a 64 bits Key.
		public static string GenerateKey()
		{
			// Create an instance of Symetric Algorithm. Key and IV is generated automatically.
			TripleDESCryptoServiceProvider desCrypto = (TripleDESCryptoServiceProvider)TripleDESCryptoServiceProvider.Create();

			// Use the Automatically generated key for Encryption. 
			return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
		}

		public static bool EncryptFile(string sInputFilename, string sOutputFilename, string sKey)
		{
			//FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
			//FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
			//TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
			//var keyBytes = ASCIIEncoding.ASCII.GetBytes(sKey);
			//DES.Key = keyBytes;
			//DES.IV = keyBytes;
			//ICryptoTransform desencrypt = DES.CreateEncryptor();
			//CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

			//byte[] bytearrayinput = new byte[fsInput.Length];
			//fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
			//cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
			//cryptostream.Close();
			//fsInput.Close();
			//fsEncrypted.Close();
			Cryptography cryptography = new Cryptography(sKey);
			return cryptography.Encrypt(sInputFilename, sOutputFilename);
		}

		public static void DecryptFile(string sInputFilename, string sOutputFilename, string sKey)
		{
			DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
			//A 64 bit key and IV is required for this provider.
			//Set secret key For DES algorithm.
			DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
			//Set initialization vector.
			DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

			//Create a file stream to read the encrypted file back.
			FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
			//Create a DES decryptor from the DES instance.
			ICryptoTransform desdecrypt = DES.CreateDecryptor();
			//Create crypto stream set to read and do a 
			//DES decryption transform on incoming bytes.
			CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);
			//Print the contents of the decrypted file.
			StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);
			fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
			fsDecrypted.Flush();
			fsDecrypted.Close();
		}
	}
}
