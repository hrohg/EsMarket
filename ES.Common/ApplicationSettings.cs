using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ES.Common.Cultures;
using ES.Common.Enumerations;

namespace ES.Common
{
    [Serializable]
    public class ApplicationSettings : INotifyPropertyChanged
    {
        public ApplicationSettings()
        {
            //InitializeGridColumns();
        }

        //public void InitializeGridColumns()
        //{
        //    ShowingColumns = GetEstateColumns();
        //    var columnsFromSettings = GetShowableColumnsFromSettingsFile();
        //    if (columnsFromSettings != null)
        //    {
        //        foreach (int column in columnsFromSettings)
        //        {
        //            ShowingColumns.Single(s => s.ID == column).Show = false;
        //        }
        //    }
        //    OnPropertyChanged("ShowingColumns");
        //}

        public static ObservableCollection<ShowableColumn> GetEstateColumns()
        {
            return new ObservableCollection<ShowableColumn>
			       	{
			       		new ShowableColumn{ ID = 1, ColumnName = CultureResources.Inst["Code"], Show=true},
						new ShowableColumn{ ID = 2, ColumnName = CultureResources.Inst["SellOrRent"], Show=true},
						new ShowableColumn{ ID = 3, ColumnName = CultureResources.Inst["Room"], Show=true},
						new ShowableColumn{ ID = 4, ColumnName = CultureResources.Inst["TheEstateType"], Show=true},
						new ShowableColumn{ ID = 5, ColumnName = CultureResources.Inst["StateOrRegion"], Show=true},
						new ShowableColumn{ ID = 6, ColumnName = CultureResources.Inst["PlaceV"], Show=true},
						new ShowableColumn{ ID = 7, ColumnName = CultureResources.Inst["Street"], Show=true},
						new ShowableColumn{ ID = 8, ColumnName = CultureResources.Inst["Address"], Show=true},
						new ShowableColumn{ ID = 9, ColumnName = CultureResources.Inst["AddressFull"], Show=true},
						new ShowableColumn{ ID = 10, ColumnName = CultureResources.Inst["Square"], Show=true},
						new ShowableColumn{ ID = 11, ColumnName = CultureResources.Inst["BuildingFloor"], Show=true},
						new ShowableColumn{ ID = 12, ColumnName = CultureResources.Inst["Floor"], Show=true},
						new ShowableColumn{ ID = 13, ColumnName = CultureResources.Inst["Price"], Show=true},
						new ShowableColumn{ ID = 14, ColumnName = CultureResources.Inst["PricePerDay"], Show=true},
						new ShowableColumn{ ID = 15, ColumnName = CultureResources.Inst["Currency"], Show=true},
						new ShowableColumn{ ID = 16, ColumnName = CultureResources.Inst["Added"], Show=true},
						new ShowableColumn{ ID = 17, ColumnName = CultureResources.Inst["Updated"], Show=true},
						new ShowableColumn{ ID = 18, ColumnName = CultureResources.Inst["Height"], Show=true},
						new ShowableColumn{ ID = 19, ColumnName = CultureResources.Inst["Seller"], Show=true},
						new ShowableColumn{ ID = 20, ColumnName = CultureResources.Inst["AdditionalDetails"], Show=true},
						new ShowableColumn{ ID = 21, ColumnName = CultureResources.Inst["Rating"], Show=true},
						new ShowableColumn{ ID = 22, ColumnName = CultureResources.Inst["Broker"], Show=true},
						new ShowableColumn{ ID = 23, ColumnName = CultureResources.Inst["InfSource"], Show=true},
						new ShowableColumn{ ID = 24, ColumnName = CultureResources.Inst["OtherDetails"], Show=true},
						new ShowableColumn{ ID = 25, ColumnName = CultureResources.Inst["Status"], Show=true},
						new ShowableColumn{ ID = 26, ColumnName = CultureResources.Inst["RemontType"], Show=true},
						new ShowableColumn{ ID = 27, ColumnName = CultureResources.Inst["TheProject"], Show=true},
						new ShowableColumn{ ID = 28, ColumnName = CultureResources.Inst["BuildingType"], Show=true},
			       	};
        }

        //private CustomCultureInfo _selectedCulture;
        //public CustomCultureInfo SelectedCulture
        //{
        //    get
        //    {
        //        if (_selectedCulture == null)
        //        {
        //            _selectedCulture = new CustomCultureInfo("ru-RU");
        //        }
        //        return _selectedCulture;
        //    }
        //    set
        //    {
        //        _selectedCulture = value;
        //        CultureResources.ChangeCulture(value);
        //        OnPropertyChanged("SelectedCulture");
        //    }
        //}

        //private ObservableCollection<UserDisplayColumn> _showingColumns;
        //public ObservableCollection<UserDisplayColumn> ShowingColumns
        //{
        //    get { return _showingColumns; }
        //    set
        //    {
        //        if (_showingColumns == value) return;
        //        _showingColumns = value;
        //        OnPropertyChanged("ShowingColumns");
        //    }
        //}

        private string _BackupFolderPath;
        public string BackupFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_BackupFolderPath))
                {
                    _BackupFolderPath = Constants.ApplicationExecutablePath;
                }
                return _BackupFolderPath;
            }
            set
            {
                _BackupFolderPath = value;
                OnPropertyChanged("BackupFolderPath");
            }
        }

        private string _databaseDataFolderPath;
        public string DatabaseDataFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_databaseDataFolderPath))
                {
                    _databaseDataFolderPath = @"C:\Program Files\Microsoft SQL Server\MSSQL.1\MSSQL\DATA";
                }
                return _databaseDataFolderPath;
            }
            set
            {
                _databaseDataFolderPath = value;
                OnPropertyChanged("DatabaseDataFolderPath");
            }
        }

        public static ApplicationSettings Default
        {
            get
            {
                return new ApplicationSettings
                        {
                            //SelectedCulture = new CustomCultureInfo("ru-RU"),
                        };
            }
        }

        public static ApplicationSettings GetSettingsFromFile()
        {
            string settingsFilePath = string.Format(@"{0}\{1}", Constants.ApplicationExecutablePath, Constants.SettingsFilePath);
            if (File.Exists(settingsFilePath))
            {
                FileStream fileStream = new FileStream(settingsFilePath, FileMode.Open);
                try
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    return (ApplicationSettings)serializer.Deserialize(fileStream);
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    fileStream.Close();
                }

            }
            return null;
        }

        public bool SaveSettings()
        {
            string settingsFilePath = string.Format(@"{0}\{1}", Constants.ApplicationExecutablePath, Constants.SettingsFilePath);
            FileStream fs = new FileStream(settingsFilePath, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
                return true;
                //todo: save app settings => yesiminch@ is not serializable 
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        //public void SaveShowingColumns()
        //{
        //    string settingsFilePath = string.Format(@"{0}\{1}", Constants.ApplicationExecutablePath, Constants.ShowableColumnsFilePath);
        //    FileStream fs = new FileStream(settingsFilePath, FileMode.Create);

        //    BinaryFormatter formatter = new BinaryFormatter();
        //    try
        //    {
        //        var showedColumns = ShowingColumns.Where(s => s.Show == false).Select(s => s.ID).ToList();
        //        formatter.Serialize(fs, showedColumns);
        //        OnPropertyChanged("ShowingColumns");
        //    }
        //    catch (Exception ex)
        //    {
        //        var v = ex.ToString();
        //    }
        //    finally
        //    {
        //        fs.Close();
        //    }
        //}

        //private static IEnumerable<int> GetShowableColumnsFromSettingsFile()
        //{
        //    string showableColumnsFilePath = string.Format(@"{0}\{1}", Constants.ApplicationExecutablePath, Constants.ShowableColumnsFilePath);
        //    if (File.Exists(showableColumnsFilePath))
        //    {
        //        FileStream fileStream = new FileStream(showableColumnsFilePath, FileMode.Open);
        //        try
        //        {
        //            BinaryFormatter serializer = new BinaryFormatter();
        //            return (List<int>)serializer.Deserialize(fileStream);
        //        }
        //        catch (Exception ex)
        //        {
        //            var v = ex.ToString();
        //            return null;
        //        }
        //        finally
        //        {
        //            fileStream.Close();
        //        }

        //    }
        //    return null;
        //}

        //public static List<UserDisplayColumn> GetEmptyDisplayColumns()
        //{
        //    return new List<UserDisplayColumn>
        //            {
        //                new UserDisplayColumn {ColumnID = 1, ColumnName = "Code", Show = true },
        //                new UserDisplayColumn {ColumnID = 2, ColumnName = "SellOrRent", Show = true },
        //                new UserDisplayColumn {ColumnID = 3, ColumnName = "Room", Show = true },
        //                new UserDisplayColumn {ColumnID = 4, ColumnName = "TheEstateType", Show = true },
        //                new UserDisplayColumn {ColumnID = 5, ColumnName = "StateOrRegion", Show = true },
        //                new UserDisplayColumn {ColumnID = 6, ColumnName = "Address", Show = true },
        //                new UserDisplayColumn {ColumnID = 7, ColumnName = "AddressFull", Show = true },
        //                new UserDisplayColumn {ColumnID = 8, ColumnName = "Square", Show = true },
        //                new UserDisplayColumn {ColumnID = 9, ColumnName = "Floor", Show = true },
        //                new UserDisplayColumn {ColumnID = 10, ColumnName = "Price", Show = true },
        //                new UserDisplayColumn {ColumnID = 11, ColumnName = "PricePerDay", Show = true },
        //                new UserDisplayColumn {ColumnID = 12, ColumnName = "Currency", Show = true },
        //                new UserDisplayColumn {ColumnID = 13, ColumnName = "Added", Show = true },
        //                new UserDisplayColumn {ColumnID = 14, ColumnName = "Updated", Show = true },
        //                new UserDisplayColumn {ColumnID = 15, ColumnName = "Height", Show = true },
        //                new UserDisplayColumn {ColumnID = 16, ColumnName = "Seller", Show = true },
        //                new UserDisplayColumn {ColumnID = 17, ColumnName = "AdditionalDetails", Show = true },
        //                new UserDisplayColumn {ColumnID = 18, ColumnName = "Broker", Show = true },
        //                new UserDisplayColumn {ColumnID = 19, ColumnName = "InfSource", Show = true },
        //                new UserDisplayColumn {ColumnID = 20, ColumnName = "Status", Show = true },
        //                new UserDisplayColumn {ColumnID = 21, ColumnName = "RemontType", Show = true },
        //                new UserDisplayColumn {ColumnID = 22, ColumnName = "TheProject", Show = true },
        //                new UserDisplayColumn {ColumnID = 23, ColumnName = "BuildingType", Show = true },
        //            };
        //}
    }
}
