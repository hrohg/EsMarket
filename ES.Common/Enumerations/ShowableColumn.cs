using System;
using System.ComponentModel;

namespace ES.Common.Enumerations
{
	[Serializable]
	public class ShowableColumn: INotifyPropertyChanged
	{
		#region ID - Описание свойства (summary)
		public const string IDProperty = "ID";

		/// <summary>Описание свойства (summary)</summary>
		public int ID
		{
			get { return _fieldId; }
			set
			{
				if (_fieldId == value) return;
				_fieldId = value;
				OnPropertyChanged(IDProperty);
			}
		}
		private int _fieldId;
		#endregion

		private string _columnName;
		public string ColumnName
		{
			get { return _columnName; }
			set
			{
				if (_columnName == value) return;
				_columnName = value;
				OnPropertyChanged("ColumnName");
			}
		}

		private bool _show;
		public bool Show
		{
			get { return _show; }
			set
			{
				if (_show == value) return;
				_show = value;
				OnPropertyChanged("Show");
			}
		}

		public override string ToString()
		{
			return this.ColumnName;
		}

        #region IdName - Описание свойства (summary)
        public const string IdNameProperty = "IdName";

        public string IdName
        {
            get { return fieldIdName; }
            set
            {
                if (fieldIdName == value) return;
                fieldIdName = value;
                OnPropertyChanged(IdNameProperty);
            }
        }
        private string fieldIdName;
        #endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
