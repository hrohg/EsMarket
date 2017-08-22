using System;
using System.IO;
using System.Reflection;

namespace ES.Common
{
    public static class Constants
    {
        public static string AppName = "EsMarket";
        public const string DataServerSettingsDataPath = "Dss";
        public const string GeneralSettingsDataPath = "Gsd";
        public const string MemberSettingsDataPath = "Msd";
        public const string ConfigFileExtantion = "conf";
        public const string DataFileExtantion = "dat";
        public const string TempInvoiceDataPath = "Msd";

        public const string LastLoggedUserName = "LLUNM.dat";
        public static string ShowableColumnsFilePath { get { return "SCols.dat"; } }
        public static string SettingsFilePath { get { return "Settings.dat"; } }
        public static string FavoritesFilePath { get { return "FavoriteEstates.xml"; } }

        public const string SettingsContainerFilePath = "SetCon.dat";

        public static string ApplicationExecutablePath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
            }
        }

        public static string BusinessDatFilePath
        {
            get { return ApplicationExecutablePath + "Business.dat"; }
        }

        public static string BusinessDatTempFilePath
        {
            get { return ApplicationExecutablePath + "BusinessTemp.dat"; }
        }

        static string _imagesFolderPath;
        public static string ImagesFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_imagesFolderPath))
                {
                    _imagesFolderPath = System.Configuration.ConfigurationManager.AppSettings["ImagesFolder"] + @"\";
                }
                return _imagesFolderPath;
            }
        }

        static string _videosFolder;

        private static string _localImagesFolderPath;
        public static string LocalImagesFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_localImagesFolderPath))
                {
                    _localImagesFolderPath = System.Configuration.ConfigurationManager.AppSettings["LocalImagesFolder"] + @"\";
                }
                return _localImagesFolderPath;
            }
        }


        private static string _localVideosFolderPath;

        private static DateTime _sqlAcceptedMinDateValue;
        public static DateTime SqlAcceptedMinDateValue
        {
            get
            {
                if (_sqlAcceptedMinDateValue == DateTime.MinValue)
                {
                    _sqlAcceptedMinDateValue = new DateTime(1753, 1, 2);
                }
                return _sqlAcceptedMinDateValue;
            }
        }

        public static string LocalVideosFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_localVideosFolderPath))
                {
                    _localVideosFolderPath = System.Configuration.ConfigurationManager.AppSettings["LocalVideosFolder"] + @"\";
                }
                return _localVideosFolderPath;
            }
        }

        public static string PrintColumnsFilePath
        {
            get { return "PCols.dat"; }
        }
        public const string BusinessFileName = "RealEstate.Business.dll";

        public static string ConfigFilePath
        {
            get { return ApplicationExecutablePath + "RealEstateApp.exe.config"; }
        }

        public static string VideosFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_videosFolder))
                {
                    _videosFolder = System.Configuration.ConfigurationManager.AppSettings["VideosFolder"] + @"\";
                }
                return _videosFolder;
            }
        }

        public static string WebImagesFolderPath
        {
            get
            {
                return "~/EstateImages/";
            }
        }


    }

    public struct DefaultControls
    {
        public const string Customer = "Customer";
        public const string Provider = "Provider";
        public const string Dealer = "Dealer";
        public const string Branch = "Branch";
    }

    public struct ProductProperties
    {
        public const string Id = "Id";
        public const string Code = "Code";
        public const string Barcode = "Barcode";
        public const string Description = "Description";
        public const string Mu = "Mu";
        public const string Note = "Note";
        public const string CostPrice = "CostPrice";
        public const string OldPrice = "OldPrice";
        public const string Price = "Price";
        public const string Discount = "Discount";
        public const string DealerPrice = "DealerPrice";
        public const string DealerDiscount = "DealerDiscount";
        public const string MinCount = "MinQuantity";
        public const string ImagePath = "ImagePath";
        public const string EsMemberId = "EsMemberId";
        public const string IsEnable = "IsEnable";
        public const string BrandId = "BrandId";
        public const string LastModifierId = "LastModifierId";
    }
}
