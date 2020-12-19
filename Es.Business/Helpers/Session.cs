using System.ComponentModel;
using System.Threading;
using ES.Common;
using ES.Data.Models;

namespace ES.Business.Helpers
{
	public class Session : INotifyPropertyChanged
	{
	    public Session()
		{
			ApplicationSettings = ApplicationSettings.Default;
			User = new EsUserModel();
			IsClientShowingMode = false;

		}

		private static Session _inst;

		public static Session Inst
		{
			get { return _inst ?? (_inst = new Session()); }
		}

        private ApplicationSettings _applicationSettings;
		public ApplicationSettings ApplicationSettings
		{
			get
			{
				if (_applicationSettings == null)
				{
					var settings = ApplicationSettings.GetSettingsFromFile();
					_applicationSettings = settings ?? ApplicationSettings.Default;
				}
				return _applicationSettings;
			}
			set
			{
				_applicationSettings = value;
				OnPropertyChanged("ApplicationSettings");
			}


		}

		#region IsWithoutVerification - Описание свойства (summary)
		public const string IsWithoutVerificationProperty = "IsWithoutVerification";

		/// <summary>Описание свойства (summary)</summary>
		public bool IsWithoutVerification
		{
			get { return _fieldIsWithoutVerification; }
			set
			{
				if (_fieldIsWithoutVerification == value) return;
				_fieldIsWithoutVerification = value;
				OnPropertyChanged(IsWithoutVerificationProperty);
			}
		}
		private bool _fieldIsWithoutVerification = true;
		#endregion

        //#region MainSettings - Описание свойства (summary)
        //public const string MainSettingsProperty = "MainSettings";

        ///// <summary>Описание свойства (summary)</summary>
        //public ESLSetting MainSettings
        //{
        //    get
        //    {
        //        if (_fieldMainSettings == null)
        //        {
        //            _fieldMainSettings = ESLSettingsManager.GetSettings(OfflineMode) ?? new ESLSetting();
        //        }
        //        return _fieldMainSettings;
        //    }
        //    set
        //    {
        //        if (_fieldMainSettings == value) return;
        //        _fieldMainSettings = value;
        //        OnPropertyChanged(MainSettingsProperty);
        //    }
        //}
        //private ESLSetting _fieldMainSettings;
        //#endregion



		private EsUserModel _user;
		public EsUserModel User
		{
			get { return _user; }
			set
			{
				_user = value;
				OnPropertyChanged("User");
			}
		}

		private bool _isClientShowingMode;
		public bool IsClientShowingMode
		{
			get { return _isClientShowingMode; }
			set
			{
				if (_isClientShowingMode == value) return;
				_isClientShowingMode = value;
				OnPropertyChanged("IsClientShowingMode");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
        

		public bool IsRussian
		{
			get
			{
				return Thread.CurrentThread.CurrentCulture.Name == "ru-RU";
			}
		}

		private string _serverIp;
		public string ServerIP
		{
			get
			{
				if (string.IsNullOrEmpty(_serverIp))
				{
					_serverIp = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
				}
				return _serverIp;
			}
			set
			{
				_serverIp = value;
				OnPropertyChanged("ServerIP");
			}
		}

		#region IsWebEnabled - Описание свойства (summary)
		public const string IsWebEnabledProperty = "IsWebEnabled";

		public bool? IsWebEnabled
		{

			get
			{
				if (!_fieldIsWebEnabled.HasValue)
				{
					_fieldIsWebEnabled = HgConvert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsWebEnabled"]);
				}
				return _fieldIsWebEnabled;
			}
			set
			{
				if (_fieldIsWebEnabled == value) return;
				_fieldIsWebEnabled = value;
				OnPropertyChanged(IsWebEnabledProperty);
			}
		}
		private bool? _fieldIsWebEnabled;

		#endregion


		#region OfflineMode - Описание свойства (summary)
		public const string OfflineModeProperty = "OfflineMode";

		/// <summary>Описание свойства (summary)</summary>
		public bool OfflineMode
		{
			get { return _fieldOfflineMode; }
			set
			{
				if (_fieldOfflineMode == value) return;
				_fieldOfflineMode = value;
				OnPropertyChanged(OfflineModeProperty);
			}
		}
		private bool _fieldOfflineMode;
		#endregion

	}
}