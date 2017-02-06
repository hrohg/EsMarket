using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using ESL.DataAccess.Helpers;
using ESL.DataAccess.Interfaces;

namespace ESL.DataAccess.Helpers
{
    public static class StaticMemebers
    {
        public static CultureInfo CurrentCulture
        {
            get { return Thread.CurrentThread.CurrentCulture; }
        }


        /// <summary>Возвращает значение одного из переданных аргументов в зависимости от культуры потока (полезен в geter'е свойства).</summary>
        /// <param name="valueKZ">Значение на казахском языке.</param>
        /// <param name="valueRU">Значение на русском языке.</param>
        /// <returns>Возвращает valueKZ, если культура потока "kk-KZ", в противном случае возвращает valueRU.</returns>
        public static string CultureChoiseGet(this object obj, string valueAM, string valueEN, string valueRU,
            string valueCZ, string valueKZ)
        {
            if (CurrentCulture != null)
            {
                switch (CurrentCulture.Name)
                {
                    case "kk-KZ":
                        return valueKZ;
                    case "hy-AM":
                        return valueAM;
                    case "ru-RU":
                        return valueRU;
                    case "en-US":
                        return valueEN;
                    case "cs-CZ":
                        return valueCZ;
                }
            }
            return string.Empty;
        }

        public static void CultureChoiseSet(this object obj, string value, ref string valueAM, ref string valueEN,
            ref string valueRU, ref string valueCZ, ref string valueKZ)
        {
            if (CurrentCulture != null)
            {
                switch (CurrentCulture.Name)
                {
                    case "kk-KZ":
                        valueKZ = value;
                        break;
                    case "hy-AM":
                        valueAM = value;
                        break;
                    case "ru-RU":
                        valueRU = value;
                        break;
                    case "en-US":
                        valueEN = value;
                        break;
                    case "cs-CZ":
                        valueCZ = value;
                        break;
                }
            }
            else
            {
                valueAM = value;
            }
        }
    }

    partial class DataClassesDataContext
    {
        partial void OnCreated()
        {
            this.CommandTimeout = 300; //5 minutes
        }
    }

    public partial class ClientSuggestedEstate
    {
        public List<int> EstatesIds { get; set; }
    }

    public partial class EstateVideo
    {
        public string VideoFilePath { get; set; }
    }

    public partial class Region
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class EstateType
    {
        public string TypeName
        {
            get { return this.CultureChoiseGet(TypeNameAm, TypeNameEn, TypeNameRu, TypeNameCz, TypeNameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _TypeNameAm, ref _TypeNameEn, ref _TypeNameRu, ref _TypeNameCz,
                    ref _TypeNameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class State
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class OrderType
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class City
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class Country
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, string.Empty, string.Empty); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameRu, ref _NameRu);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class Currency
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class Street
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    [Serializable]
    public partial class Estate : IDemandEstateDisplayData
    {
        public string ShortAddressString
        {
            get
            {
                var placeName = !string.IsNullOrEmpty(PlaceName) ? (string.Format("({0})", PlaceName)) : string.Empty;
                return string.Format("{0} {1}", Region != null ? Region.Name : string.Empty, placeName);
            }
        }

        string IDemandEstateDisplayData.Price
        {
            get
            {
                string price = string.Empty;
                if (this.Price.HasValue)
                {
                    price = Price.ToString();
                }
                if (this.Currency != null)
                {
                    price += " " + this.Currency.Name;
                }
                return price;
            }
        }

        string IDemandEstateDisplayData.Square
        {
            get
            {
                string square = string.Empty;
                if (this.TotalSquare.HasValue)
                {
                    square = TotalSquare.ToString();
                }
                if (this.Square.HasValue)
                {
                    square += "/" + Square;
                }
                return square;
            }
        }

        string IDemandEstateDisplayData.EstateTypes
        {
            get { return this.EstateType.TypeName; }
        }

        public string SellerInfo
        {
            get
            {
                string info = SellerName;
                if (!string.IsNullOrEmpty(PhonePrimary))
                {
                    info += string.Format(", {0}", PhonePrimary);
                }
                if (!string.IsNullOrEmpty(PhoneSecondary))
                {
                    info += string.Format(", {0}", PhoneSecondary);
                }
                return info.TrimStart(',', ' ');
            }
        }

        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", this.EstateType.TypeName, this.ShortAddressString);
        }

        public string ClientName
        {
            get { return this.SellerName; }
        }

        public string Rooms
        {
            get
            {
                var roomsCount = string.Empty;
                if (this.RoomCount.HasValue)
                {
                    roomsCount = this.RoomCount.ToString();
                }
                if (this.MakedRooms.HasValue)
                {
                    roomsCount += " -> " + this.MakedRooms;
                }
                return roomsCount;
            }
        }

        public string Regions
        {
            get
            {
                if (this.Region != null)
                {
                    return this.Region.Name;
                }
                return string.Empty;
            }
        }

        public string StateAndOrRegion
        {
            get
            {
                string str = string.Empty;
                if (State != null && State.ID != 1)
                {
                    if (!string.IsNullOrEmpty(State.Name))
                    {
                        str += State.Name;
                    }
                    if (City != null && !string.IsNullOrEmpty(City.Name))
                    {
                        str += string.Format("{0}{1}", string.IsNullOrEmpty(str) ? string.Empty : ", ", City.Name);
                    }
                    if (Region != null && !string.IsNullOrEmpty(Region.Name))
                    {
                        str += string.Format("{0}{1}", string.IsNullOrEmpty(str) ? string.Empty : ", ", Region.Name);
                    }
                    return str;
                }
                return Region != null ? Region.Name : string.Empty;
            }
        }

        public string FLoorFull
        {
            get
            {
                var addFloor = FloorAdditional.HasValue ? " - " + FloorAdditional : string.Empty;
                if (BuildingFloorsCount.HasValue && Floor.HasValue)
                {
                    return string.Format("{0}{2}/{1}", Floor, BuildingFloorsCount, addFloor);
                }
                if (Floor.HasValue)
                {
                    return string.Format("{0}{1}", Floor, addFloor);
                }
                if (BuildingFloorsCount.HasValue)
                {
                    return BuildingFloorsCount.ToString();
                }
                return string.Empty;
            }
        }

        public int ID
        {
            get { return this.EstateID; }
        }

        public string RentSell
        {
            get { return this.OrderType.Name; }
        }

        #region ShowDate - Описание свойства (summary)

        public const string ShowDateProperty = "ShowDate";

        /// <summary>Описание свойства (summary)</summary>
        [System.Runtime.Serialization.DataMember]
        public DateTime? ShowDate
        {
            get { return fieldShowDate; }
            set
            {
                if (fieldShowDate == value) return;
                fieldShowDate = value;
            }
        }

        private DateTime? fieldShowDate;

        #endregion

        [DataMember]
        public List<EstateImage> ImagesList { get; set; }

        [DataMember]
        public List<EstateVideo> VideosList { get; set; }

        [DataMember]
        public List<EstateConvenient> ConvenientsList { get; set; }

        public string SquareString
        {
            get
            {
                var str = string.Empty;
                if (TotalSquare.HasValue)
                {
                    str = TotalSquare.ToString();
                }
                if (this.Square.HasValue)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        str = Square.ToString();
                    }
                    else
                    {
                        str += "/" + Square;
                    }
                }
                return str;
            }
        }
    }


    public partial class EsUser
    {
        public EslRoles Role
        {
            get { return (EslRoles)RoleID.GetValueOrDefault(2); }
        }

        public string FullName
        {
            get { return string.Format("{0}  {1}", Name, Email); }
        }

        public bool IsAdmin
        {
            get { return RoleID.HasValue && RoleID.Value == (int)EslRoles.Admin; }
        }

        public bool IsAdminOrDirector
        {
            get { return RoleID.HasValue && (RoleID.Value == (int)EslRoles.Admin || RoleID.Value == (int)EslRoles.Operator); }
        }

        public bool IsDirector
        {
            get { return RoleID.HasValue && RoleID.Value == (int)EslRoles.Operator; }
        }

        public bool IsBroker
        {
            get { return RoleID.GetValueOrDefault(0) == (int)EslRoles.Manager; }
        }

        [DataMember]
        public List<BrokerEstateType> EstateTypes { get; set; }

        [DataMember]
        public List<BrokerOrderType> OrderTypes { get; set; }

        [DataMember]
        public List<BrokersRegion> Regions { get; set; }

        [DataMember]
        public List<BrokerState> States { get; set; }
    }

    public enum EslRoles
    {
        Admin = 1,
        Manager = 2,
        Operator = 3
    };

    public partial class BuildingType
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }

    }

    public partial class Remont
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class Project
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class Convenient
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }
    }

    public partial class OperationalSignificance
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class SignificanceOfTheUse
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class Roofing
    {
        public string Name
        {
            get { return this.CultureChoiseGet(NameAm, NameEn, NameRu, NameCz, NameKz); }
            set
            {
                this.CultureChoiseSet(value, ref _NameAm, ref _NameEn, ref _NameRu, ref _NameCz, ref _NameKz);
                this.SendPropertyChanged("Name");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class SelledEstate
    {
        public bool IsValid
        {
            get
            {
                return this.EstateID > 0 && this.Price.HasValue && this.SellDate.HasValue;
            }
        }
    }

    public partial class RentedEstate
    {
        public bool IsValid
        {
            get
            {
                if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
                {
                    return false;
                }
                return this.EstateID > 0 && (this.Price.HasValue || this.PricePerDay.HasValue) && this.StartDate.HasValue;
            }
        }
    }

    public partial class EstateImage
    {
        public string ImageFilePath { get; set; }
    }

    public partial class BlackListItem
    {
        public BlackListItem Clone()
        {
            return new BlackListItem
                    {
                        ID = this.ID,
                        Name = this.Name,
                        Comment = this.Comment,
                        BlackListNumbers = this.BlackListNumbers
                    };
        }

        [DataMember]
        public List<BlackListNumber> Numbers { get; set; }
    }

    public partial class UserDisplayColumn
    {
        #region Constants

        public const string Code = "Code";
        public const string SellOrRent = "SellOrRent";
        public const string Room = "Room";
        public const string TheEstateType = "TheEstateType";
        public const string StateOrRegion = "StateOrRegion";
        public const string Address = "Address";
        public const string AddressFull = "AddressFull";
        public const string Square = "Square";
        public const string Floor = "Floor";
        public const string Price = "Price";
        public const string PricePerDay = "PricePerDay";
        public const string Currency = "Currency";
        public const string Added = "Added";
        public const string Updated = "Updated";
        public const string Height = "Height";
        public const string Seller = "Seller";
        public const string AdditionalDetails = "AdditionalDetails";
        public const string Broker = "Broker";
        public const string InfSource = "InfSource";
        public const string Status = "Status";
        public const string RemontType = "RemontType";
        public const string TheProject = "TheProject";
        public const string BuildingType = "BuildingType";
        public const string OtherDetails = "OtherDetails";
        public const string Street = "Street";

        #endregion

        public string BindingPath { get; set; }
        public string IdName { get; set; }

        public static List<UserDisplayColumn> GetEmptyDisplayColumns()
        {
            return new List<UserDisplayColumn>
			       	{
			       		new UserDisplayColumn {ColumnID = 1, ColumnName = Code, Show = true },
			       		new UserDisplayColumn {ColumnID = 2, ColumnName = SellOrRent, Show = true },
			       		new UserDisplayColumn {ColumnID = 3, ColumnName = Room, Show = true },
			       		new UserDisplayColumn {ColumnID = 4, ColumnName = TheEstateType, Show = true },
			       		new UserDisplayColumn {ColumnID = 5, ColumnName = StateOrRegion, Show = true },
			       		new UserDisplayColumn {ColumnID = 6, ColumnName = Address, Show = true },
			       		new UserDisplayColumn {ColumnID = 7, ColumnName = AddressFull, Show = true },
			       		new UserDisplayColumn {ColumnID = 8, ColumnName = Square, Show = true },
			       		new UserDisplayColumn {ColumnID = 9, ColumnName = Floor, Show = true },
			       		new UserDisplayColumn {ColumnID = 10, ColumnName = Price, Show = true },
			       		new UserDisplayColumn {ColumnID = 11, ColumnName = PricePerDay, Show = true },
			       		new UserDisplayColumn {ColumnID = 12, ColumnName = Currency, Show = true },
			       		new UserDisplayColumn {ColumnID = 13, ColumnName = Added, Show = true },
			       		new UserDisplayColumn {ColumnID = 14, ColumnName = Updated, Show = true },
			       		new UserDisplayColumn {ColumnID = 15, ColumnName = Height, Show = true },
			       		new UserDisplayColumn {ColumnID = 16, ColumnName = Seller, Show = true },
			       		new UserDisplayColumn {ColumnID = 17, ColumnName = AdditionalDetails, Show = true },
			       		new UserDisplayColumn {ColumnID = 18, ColumnName = Broker, Show = true },
			       		new UserDisplayColumn {ColumnID = 19, ColumnName = InfSource, Show = true },
			       		new UserDisplayColumn {ColumnID = 20, ColumnName = Status, Show = true },
			       		new UserDisplayColumn {ColumnID = 21, ColumnName = RemontType, Show = true },
			       		new UserDisplayColumn {ColumnID = 22, ColumnName = TheProject, Show = true },
			       		new UserDisplayColumn {ColumnID = 23, ColumnName = BuildingType, Show = true },
			       		new UserDisplayColumn {ColumnID = 24, ColumnName = OtherDetails, Show = true },
			       		new UserDisplayColumn {ColumnID = 25, ColumnName = Street, Show = true },
			       	};
        }

        public string GetBindingPath()
        {
            switch (this.IdName)
            {
                case SellOrRent:
                    return "OrderType.Name";
                case TheEstateType:
                    return "EstateType.TypeName";
                case Address:
                    return "ShortAddressString";
                case Street:
                    return "Street.Name";
                case Square:
                    return "SquareString";
                case RemontType:
                    return "Remont.Name";
                case BuildingType:
                    return "BuildingType.Name";
                case Currency:
                    return "Currency.Name";
                case Added:
                    return "AddDate";
                case Updated:
                    return "LastModifiedDate";
                case Seller:
                    return "SellerInfo";
                case InfSource:
                    return "InformationSource";
                case Broker:
                    return "User.FullName";
                case TheProject:
                    return "Project.Name";
            }
            return IdName;
        }
    }
}
