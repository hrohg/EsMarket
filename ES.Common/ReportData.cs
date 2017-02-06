namespace ES.Common
{
	public class ReportData : BindableObject
	{
		#region EstatesCount - Описание свойства (summary)
		public const string EstatesCountProperty = "EstatesCount";

		/// <summary>Описание свойства (summary)</summary>
		public int EstatesCount
		{
			get { return _fieldEstatesCount; }
			set
			{
				if (_fieldEstatesCount == value) return;
				_fieldEstatesCount = value;
				OnPropertyChanged(EstatesCountProperty);
			}
		}
		private int _fieldEstatesCount;
		#endregion

		#region DemandsCount - Описание свойства (summary)
		public const string DemandsCountProperty = "DemandsCount";

		/// <summary>Описание свойства (summary)</summary>
		public int DemandsCount
		{
			get { return _fieldDemandsCount; }
			set
			{
				if (_fieldDemandsCount == value) return;
				_fieldDemandsCount = value;
				OnPropertyChanged(DemandsCountProperty);
			}
		}
		private int _fieldDemandsCount;
		#endregion

		#region AddedEstatesCount - Описание свойства (summary)
		public const string AddedEstatesCountProperty = "AddedEstatesCount";

		/// <summary>Описание свойства (summary)</summary>
		public int AddedEstatesCount
		{
			get { return _fieldAddedEstatesCount; }
			set
			{
				if (_fieldAddedEstatesCount == value) return;
				_fieldAddedEstatesCount = value;
				OnPropertyChanged(AddedEstatesCountProperty);
			}
		}
		private int _fieldAddedEstatesCount;
		#endregion

		#region AddedDemandsCount - Описание свойства (summary)
		public const string AddedDemandsCountProperty = "AddedDemandsCount";

		/// <summary>Описание свойства (summary)</summary>
		public int AddedDemandsCount
		{
			get { return _fieldAddedDemandsCount; }
			set
			{
				if (_fieldAddedDemandsCount == value) return;
				_fieldAddedDemandsCount = value;
				OnPropertyChanged(AddedDemandsCountProperty);
			}
		}
		private int _fieldAddedDemandsCount;
		#endregion

		#region ShowedEstatesCount - Описание свойства (summary)
		public const string ShowedEstatesCountProperty = "ShowedEstatesCount";

		/// <summary>Описание свойства (summary)</summary>
		public int ShowedEstatesCount
		{
			get { return _fieldShowedEstatesCount; }
			set
			{
				if (_fieldShowedEstatesCount == value) return;
				_fieldShowedEstatesCount = value;
				OnPropertyChanged(ShowedEstatesCountProperty);
			}
		}
		private int _fieldShowedEstatesCount;
		#endregion

		#region ShowedClientsCount - Описание свойства (summary)
		public const string ShowedClientsCountProperty = "ShowedClientsCount";

		/// <summary>Описание свойства (summary)</summary>
		public int ShowedClientsCount
		{
			get { return _fieldShowedClientsCount; }
			set
			{
				if (_fieldShowedClientsCount == value) return;
				_fieldShowedClientsCount = value;
				OnPropertyChanged(ShowedClientsCountProperty);
			}
		}
		private int _fieldShowedClientsCount;
		#endregion

		#region UpdatedEstatesCount - Описание свойства (summary)
		public const string UpdatedEstatesCountProperty = "UpdatedEstatesCount";

		/// <summary>Описание свойства (summary)</summary>
		public int UpdatedEstatesCount
		{
			get { return _fieldUpdatedEstatesCount; }
			set
			{
				if (_fieldUpdatedEstatesCount == value) return;
				_fieldUpdatedEstatesCount = value;
				OnPropertyChanged(UpdatedEstatesCountProperty);
			}
		}
		private int _fieldUpdatedEstatesCount;
		#endregion

		#region UpdatedDemandsCount - Описание свойства (summary)
		public const string UpdatedDemandsCountProperty = "UpdatedDemandsCount";

		/// <summary>Описание свойства (summary)</summary>
		public int UpdatedDemandsCount
		{
			get { return _fieldUpdatedDemandsCount; }
			set
			{
				if (_fieldUpdatedDemandsCount == value) return;
				_fieldUpdatedDemandsCount = value;
				OnPropertyChanged(UpdatedDemandsCountProperty);
			}
		}
		private int _fieldUpdatedDemandsCount;
		#endregion

		#region SoldEstatesCount - Описание свойства (summary)
		public const string SoldEstatesCountProperty = "SoldEstatesCount";

		/// <summary>Описание свойства (summary)</summary>
		public int SoldEstatesCount
		{
			get { return _fieldSoldEstatesCount; }
			set
			{
				if (_fieldSoldEstatesCount == value) return;
				_fieldSoldEstatesCount = value;
				OnPropertyChanged(SoldEstatesCountProperty);
			}
		}
		private int _fieldSoldEstatesCount;
		#endregion

		#region RentedEstatesCount - Описание свойства (summary)
		public const string RentedEstatesCountProperty = "RentedEstatesCount";

		/// <summary>Описание свойства (summary)</summary>
		public int RentedEstatesCount
		{
			get { return _fieldRentedEstatesCount; }
			set
			{
				if (_fieldRentedEstatesCount == value) return;
				_fieldRentedEstatesCount = value;
				OnPropertyChanged(RentedEstatesCountProperty);
			}
		}
		private int _fieldRentedEstatesCount;
		#endregion

		#region BrokerName - Описание свойства (summary)
		public const string BrokerNameProperty = "BrokerName";

		/// <summary>Описание свойства (summary)</summary>
		public string BrokerName
		{
			get { return _fieldBrokerName; }
			set
			{
				if (_fieldBrokerName == value) return;
				_fieldBrokerName = value;
				OnPropertyChanged(BrokerNameProperty);
			}
		}
		private string _fieldBrokerName;
		#endregion

		
	}
}
