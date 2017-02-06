using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ES.Common
{
    [Serializable]
    public class SettingsContainer
    {
		static SettingsContainer()
		{
			Settings = GetSettings();
		}

		#region Properties

		public string LastSelectedLanguage { get; set; }

		#endregion

		private static SettingsContainer Settings { get; set; }

		#region Methods
		
		public static bool SaveSettings(SettingsContainer settings)
		{
			var setingsFilePath = string.Format("{0}{1}", Constants.ApplicationExecutablePath, Constants.SettingsContainerFilePath);
			FileStream fs = new FileStream(setingsFilePath, FileMode.Create);

			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
				formatter.Serialize(fs, settings);
				Settings = settings;
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				fs.Close();
			}
		}

		public static SettingsContainer GetSettings()
		{
			if (Settings != null) return Settings;

			string settingsFilePath = string.Format(@"{0}\{1}", Constants.ApplicationExecutablePath, Constants.SettingsContainerFilePath);
			if (File.Exists(settingsFilePath))
			{
				FileStream fileStream = new FileStream(settingsFilePath, FileMode.Open);
				try
				{
					BinaryFormatter serializer = new BinaryFormatter();
					return (SettingsContainer)serializer.Deserialize(fileStream);
				}
				catch (Exception)
				{
					return new SettingsContainer();
				}
				finally
				{
					fileStream.Close();
				}
			}
			return new SettingsContainer();
		} 

		#endregion

    	public static void SaveSelectedLanguage(string language)
    	{
    		var settings = GetSettings();
			settings.LastSelectedLanguage = language;
    		SaveSettings(settings);
    	}
    }
}
