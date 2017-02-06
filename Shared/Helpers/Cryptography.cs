using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers
{
	//public static class Cryptography
	//{
	//    public static bool EncryptFile(string inputFilename, string outputFilename, string key)
	//    {
	//        try
	//        {
	//            FileStream fsInput = new FileStream(inputFilename, FileMode.Open, FileAccess.Read);
	//            FileStream fsEncrypted = new FileStream(outputFilename, FileMode.Create, FileAccess.Write);
	//            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
	//            DES.Key = DES.IV = Encoding.ASCII.GetBytes(key);
	//            ICryptoTransform desencrypt = DES.CreateEncryptor();
	//            CryptoStream cryptoStream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);
	//            byte[] bytearrayinput = new byte[fsInput.Length - 1];
	//            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
	//            cryptoStream.Write(bytearrayinput, 0, bytearrayinput.Length);
	//            return true;
	//        }
	//        catch (Exception)
	//        {
	//            return false;
	//        }
	//    }

	//    public static bool DecryptFile(string inputFilename, string outputFilename, string key)
	//    {
	//        try
	//        {
	//            DESCryptoServiceProvider DES = new DESCryptoServiceProvider
	//                                            {
	//                                                Key = Encoding.ASCII.GetBytes(key),
	//                                                IV = Encoding.ASCII.GetBytes(key)
	//                                            };
	//            FileStream fsread = new FileStream(inputFilename, FileMode.Open, FileAccess.Read);
	//            ICryptoTransform desdecrypt = DES.CreateDecryptor();
	//            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);
	//            StreamWriter fsDecrypted = new StreamWriter(outputFilename);
	//            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
	//            fsDecrypted.Flush();
	//            fsDecrypted.Close();
	//            return true;
	//        }
	//        catch (Exception)
	//        {
	//            return false;
	//        }
	//    }
	//}


	public class Cryptography
	{
		private string _Phrase = string.Empty;
		private string _inputFile = "";
		private string _outputFile = "";
		enum TransformType { ENCRYPT = 0, DECRYPT = 1 }

		/// <value>Set the phrase used to generate the secret key.</value>
		public string Phrase
		{
			set
			{
				this._Phrase = value;
				this.GenerateKey(this._Phrase);
			}
		}

		private byte[] _IV;

		private byte[] _Key;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="SecretPhrase">Secret phrase to generate key</param>
		public Cryptography(string SecretPhrase)
		{
			this.Phrase = SecretPhrase;
		}

		/// <summary>
		/// Encrypt the given value with the Rijndael algorithm.
		/// </summary>
		/// <param name="EncryptValue">Value to encrypt</param>
		/// <returns>Encrypted value. </returns>
		public string Encrypt(string EncryptValue)
		{
			try
			{
				if (EncryptValue.Length > 0)
				{
					// Write the encrypted value into memory
					byte[] input = Encoding.UTF8.GetBytes(EncryptValue);

					// Retrieve the encrypted value and return it
					return (Convert.ToBase64String(Transform(input, TransformType.ENCRYPT)));

				}
				else
				{
					return "";
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Decrypt the given value with the Rijndael algorithm.
		/// </summary>
		/// <param name="DecryptValue">Value to decrypt</param>
		/// <returns>Decrypted value. </returns>
		public string Decrypt(string DecryptValue)
		{

			try
			{
				if (DecryptValue.Length > 0)
				{
					// Write the encrypted value into memory                    
					byte[] input = Convert.FromBase64String(DecryptValue);

					// Retrieve the decrypted value and return it
					return (Encoding.UTF8.GetString(Transform(input, TransformType.DECRYPT)));

				}
				else
				{
					return "";
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool Encrypt(string InputFile, string OutputFile)
		{

			try
			{
				if ((InputFile != null) && (InputFile.Length > 0))
				{
					_inputFile = InputFile;
				}
				if ((OutputFile != null) && (OutputFile.Length > 0))
				{
					_outputFile = OutputFile;
				}
				Transform(null, TransformType.ENCRYPT);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Decrypt the given value with the Rijndael algorithm.
		/// </summary>
		/// <returns>Decrypted file. </returns>
		public bool Decrypt(string InputFile, string OutputFile)
		{

			try
			{
				if ((InputFile != null) && (InputFile.Length > 0))
				{
					_inputFile = InputFile;
				}
				if ((OutputFile != null) && (OutputFile.Length > 0))
				{
					_outputFile = OutputFile;
				}
				Transform(null, TransformType.DECRYPT);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/*****************************************************************
		 * Generate an encryption key based on the given phrase.  The 
		 * phrase is hashed to create a unique 32 character (256-bit) 
		 * value, of which 24 characters (192 bit) are used for the
		 * key and the remaining 8 are used for the initialization 
		 * vector (IV).
		 * 
		 * Parameters:  SecretPhrase - phrase to generate the key and 
		 * IV from.
		 * 
		 * Return Val:  None  
		 ***************************************************************/
		private void GenerateKey(string SecretPhrase)
		{
			// Initialize internal values
			this._Key = new byte[24];
			this._IV = new byte[16];

			// Perform a hash operation using the phrase.  This will 
			// generate a unique 32 character value to be used as the key.
			byte[] bytePhrase = Encoding.ASCII.GetBytes(SecretPhrase);
			SHA384Managed sha384 = new SHA384Managed();
			sha384.ComputeHash(bytePhrase);
			byte[] result = sha384.Hash;

			// Transfer the first 24 characters of the hashed value to the key
			// and the remaining 8 characters to the intialization vector.
			for (int loop = 0; loop < 24; loop++) this._Key[loop] = result[loop];
			for (int loop = 24; loop < 40; loop++) this._IV[loop - 24] = result[loop];
		}

		/*****************************************************************
		 * Transform one form to anoter based on CryptoTransform
		 * It is used to encrypt to decrypt as well as decrypt to encrypt
		 * Parameters:  input [byte array] - which needs to be transform 
		 *              transformType - encrypt/decrypt transform
		 * 
		 * Return Val:  byte array - transformed value.
		 ***************************************************************/
		private byte[] Transform(byte[] input, TransformType transformType)
		{
			CryptoStream cryptoStream = null;      // Stream used to encrypt
			RijndaelManaged rijndael = null;        // Rijndael provider
			ICryptoTransform rijndaelTransform = null;// Encrypting object            
			FileStream fsIn = null;                 //input file
			FileStream fsOut = null;                //output file
			MemoryStream memStream = null;          // Stream to contain data
			try
			{
				// Create the crypto objects
				rijndael = new RijndaelManaged();
				rijndael.Key = this._Key;
				rijndael.IV = this._IV;
				if (transformType == TransformType.ENCRYPT)
				{
					rijndaelTransform = rijndael.CreateEncryptor();
				}
				else
				{
					rijndaelTransform = rijndael.CreateDecryptor();
				}

				if ((input != null) && (input.Length > 0))
				{
					memStream = new MemoryStream();
					cryptoStream = new CryptoStream(memStream, rijndaelTransform, CryptoStreamMode.Write);
					cryptoStream.Write(input, 0, input.Length);
					cryptoStream.FlushFinalBlock();
					return memStream.ToArray();
				}
				else if ((_inputFile.Length > 0) && (_outputFile.Length > 0))
				{
					fsIn = new FileStream(_inputFile, FileMode.Open, FileAccess.Read);
					fsOut = new FileStream(_outputFile, FileMode.OpenOrCreate, FileAccess.Write);
					cryptoStream = new CryptoStream(fsOut, rijndaelTransform, CryptoStreamMode.Write);
					int bufferLen = 4096;
					byte[] buffer = new byte[bufferLen];
					int bytesRead;
					do
					{
						bytesRead = fsIn.Read(buffer, 0, bufferLen);
						cryptoStream.Write(buffer, 0, bytesRead);
					} while (bytesRead != 0);
					cryptoStream.FlushFinalBlock();
				}
				return null;
			}
			catch (CryptographicException)
			{
				throw new CryptographicException();
			}
			finally
			{
				if (rijndael != null) rijndael.Clear();
				if (rijndaelTransform != null) rijndaelTransform.Dispose();
				if (cryptoStream != null) cryptoStream.Close();
				if (memStream != null) memStream.Close();
				if (fsOut != null) fsOut.Close();
				if (fsIn != null) fsIn.Close();
			}
		}

	}
}

