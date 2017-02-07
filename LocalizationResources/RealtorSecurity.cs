
using System;
using System.ServiceModel;
using Microsoft.Win32;

namespace LocalizationResources
{
	public class RealtorSecurity
	{

		public bool VerifyAppValidation()
		{
			using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(SecurityObject.RegistryFolder, true))
			{
				if (registry == null) return false;

				var value = registry.GetValue(SecurityObject.ApplicationRegistryEntryKey);
				if (value == null) return false;

				string regValue = value.ToString();
				if (string.IsNullOrEmpty(regValue)) return false;

				var decryptedRegistryValue = StringEncription.DecryptString(regValue, SecurityObject.AppEncryptPassword);
				if (string.IsNullOrEmpty(decryptedRegistryValue)) return false;
				SecurityObject securityObject = new SecurityObject();
				if (decryptedRegistryValue != securityObject.GetHardUniqueID()) return false;

				return true;
			}
		}

		public bool VerifyLastAccessDate()
		{
			using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(SecurityObject.RegistryFolder, true))
			{
				if (registry == null) return false;

				var value = registry.GetValue(SecurityObject.LastAccessDateKey);
				if (value == null) return false;

				string regValue = value.ToString();
				if (string.IsNullOrEmpty(regValue)) return false;

				var lastAccessDateInRegistry = StringEncription.GetDecryptedDate(regValue);
				if (lastAccessDateInRegistry.Date > DateTime.Now.Date)
				{
					return false;
				}

				try
				{
					//WebService.DateTimeServiceSoapClient proxy = new WebService.DateTimeServiceSoapClient();
					//var encriptedDate = proxy.GetCurrentDate();
					//var dt = StringEncription.GetDecryptedDate(encriptedDate);
					//UpdateLastAccessDate(dt);
					//if (dt.Date != DateTime.Now.Date)
					//{
					//    return false;
					//}
				}
				catch (EndpointNotFoundException)
				{
					UpdateLastAccessDate(DateTime.Now);
				}
				catch
				{
					UpdateLastAccessDate(DateTime.Now);  //todo: temp
					//return false;
				}
				return true;
			}
		}

		public DateTime? GetExpirationDate()
		{
			using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(SecurityObject.RegistryFolder, true))
			{
				if (registry == null)
				{
					return null;
				}

				var value = registry.GetValue(SecurityObject.ExpirationDateKey);
				if (value == null)
				{
					return null;
				}

				string regValue = value.ToString();
				if (string.IsNullOrEmpty(regValue))
				{
					return null;
				}

				return StringEncription.GetDecryptedDate(regValue);
			}
		}

		private void UpdateLastAccessDate(DateTime currentDate)
		{
			string lastAccessDateEncripted = StringEncription.GetEncryptedDate(currentDate);
			using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(SecurityObject.RegistryFolder, true))
			{
				registry.SetValue(SecurityObject.LastAccessDateKey, lastAccessDateEncripted);
			}
		}

		private void UpdateExpirationDate(DateTime expirationDate)
		{
			string expirationDateEncripted = StringEncription.GetEncryptedDate(expirationDate);
			using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(SecurityObject.RegistryFolder, true))
			{
				if (registry == null)
				{
					throw new Exception("incorrect params");
				}
				registry.SetValue(SecurityObject.ExpirationDateKey, expirationDateEncripted);
			}
		}

		public bool? VerifyExpirationDate(out string errorMessage)
		{

			using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(SecurityObject.RegistryFolder, true))
			{
				if (registry == null)
				{
					errorMessage = "incorrect parameters";
					return false;
				}

				var value = registry.GetValue(SecurityObject.ExpirationDateKey);
				if (value == null)
				{
					errorMessage = "incorrect parameters";
					return null;
				}

				string regValue = value.ToString();
				if (string.IsNullOrEmpty(regValue))
				{
					errorMessage = "incorrect parameters";
					return null;
				}

				var expirationDate = StringEncription.GetDecryptedDate(regValue);
				if (expirationDate.Date >= DateTime.Now.Date)
				{
					var interval = expirationDate - DateTime.Now.Date;
					if (interval.Days <= 7)
					{
						errorMessage = string.Format("Հաջորդ կոդի մուտքագրումը պետք է կատարել {0}-ին", expirationDate.Date.ToString(StringEncription.DateFormat));
						return true;
					}
				}
				else
				{
					errorMessage = "Ներողություն, օգտագործման ժամկետը լրացել է, խնդրում ենք մուտքագրել նոր կոդ";
					return false;
				}
			}
			errorMessage = string.Empty;
			return true;
		}

		public bool ValidateExpirationDateCode(string expirationDateCode, out DateTime? date)
		{
			var expirationDate = date = StringEncription.GetDecryptedExpirationDate(expirationDateCode);

			if (expirationDate == null)
			{
				return false;
			}

			UpdateExpirationDate(expirationDate.Value);
			return true;
		}

		public string GenerateExpirationDateCode(DateTime expirationDate)
		{
			return StringEncription.GetEncryptedExpirationDate(expirationDate);
		}

		public string GenerateCode(string text)
		{
			return StringEncription.EncryptString(text, SecurityObject.AppEncryptPassword);
		}

		public void DeleteExpirationDate()
		{
			using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(SecurityObject.RegistryFolder, true))
			{
				if (registry == null)
				{
					throw new Exception("incorrect params");
				}
				registry.SetValue(SecurityObject.ExpirationDateKey, string.Empty);
			}
		}
	}
}

