
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;

namespace LocalizationResources
{
	public class SecurityObject
	{
		public const string RegistryFolder = "RRFA";

		public DateTime LastAccessDate { get; set; }
		public const string LastAccessDateKey = "LAD";

		public DateTime ExpirationDate { get; set; }
		public const string ExpirationDateKey = "EAD";

		public const string ApplicationRegistryEntryKey = "RAK";
		public const string ApplicationRegistryEntryValue = "z3Qm5xn7Rq8pe9Uk2s";

		internal const string DateEncryptPassword = "lCk1Vs3EO0E7p8q6r1M2C2R6u9i2o6w";
		public const string AppEncryptPassword = "q8A8mn8Yq8pOo4eG3ry2Su6Ez9Gn9v5bM3z5mQ6v";
		internal const string EncriptAppKey = "ds1jf3Jsd9k2fl9J9LK70kl";
		internal const string EncriptKeyPartDate = "Pkvaz8Z1M9k";
		//internal const string EncriptKeyPart1Month = "lkjK94az8D9lkq1OM9kU";
		//internal const string EncriptKeyPart2Month = "iYJ9k3cn8Bl1ZP7Wmqz7";
		public string GetAppEncriptedKey()
		{
			return StringEncription.EncryptString(GetHardUniqueID(), AppEncryptPassword);
		}

		public string GetNetworkAdapterAddress()
		{
			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (nic.OperationalStatus == OperationalStatus.Up)
				{
					return nic.GetPhysicalAddress().ToString();
				}
			}
			return ApplicationRegistryEntryValue;
		}

		public string GetHardUniqueID()
		{
			string drive = string.Empty;
			if (drive == string.Empty)
			{
				//Find first drive
				foreach (DriveInfo compDrive in DriveInfo.GetDrives())
				{
					if (compDrive.IsReady)
					{
						drive = compDrive.RootDirectory.ToString();
						break;
					}
				}
			}

			if (drive.EndsWith(":\\"))
			{
				//C:\ -> C
				drive = drive.Substring(0, drive.Length - 2);
			}

			string volumeSerial = getVolumeSerial(drive);
			string cpuID = getCPUID();

			//Mix them up and remove some useless 0's
			return cpuID.Substring(13) + cpuID.Substring(1, 4) + volumeSerial + cpuID.Substring(4, 4);
		}

		private string getVolumeSerial(string drive)
		{
			ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
			disk.Get();

			string volumeSerial = disk["VolumeSerialNumber"].ToString();
			disk.Dispose();

			return volumeSerial;
		}

		private string getCPUID()
		{
			string cpuInfo = "";
			ManagementClass managClass = new ManagementClass("win32_processor");
			ManagementObjectCollection managCollec = managClass.GetInstances();

			foreach (ManagementObject managObj in managCollec)
			{
				if (cpuInfo == "")
				{
					//Get only the first CPU's ID
					cpuInfo = managObj.Properties["processorID"].Value.ToString();
					break;
				}
			}

			return cpuInfo;
		}
	}
}