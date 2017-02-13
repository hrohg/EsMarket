using System.IO;
using System.Windows;
using ES.Common.Cultures;
using ES.DataAccess.Helpers;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    //[ServiceContract]
    //public interface IEstateService
    //{
    //    [OperationContract]
    //    City[] GetCities();

    //    [OperationContract]
    //    int SaveEstate(Estate estate, ref StringBuilder errorMessage);
    //}

    public class DataManager
    {
        public static DataManager GetInstance()
        {
            return new DataManager();
        }

        //public List<City> GetCities(State state, bool isOfflineMode)
        //{
        //    //ChannelFactory<IEstateService> fact = new ChannelFactory<IEstateService>("*");
        //    //IEstateService serv = fact.CreateChannel();
        //    //var cities = serv.GetCities().ToList();
        //    var cities = CityManager.GetCities(state.ID, isOfflineMode);
        //    //Translator.TranslateCities(cities);
        //    return cities;
        //}

        //public ObservableCollection<City> GetCitiesByCountry(int country, bool isOfflineMode)
        //{
        //    //ChannelFactory<IEstateService> fact = new ChannelFactory<IEstateService>("*");
        //    //IEstateService serv = fact.CreateChannel();
        //    //var cities = serv.GetCities().ToList();
        //    var cities = CityManager.GetCitiesByCountry(country, isOfflineMode);
        //    //Translator.TranslateCities(cities);
        //    return cities;
        //}

        //public List<Remont> GetRemonts(bool isOfflineMode)
        //{
        //    var remonts = RemontManager.GetRemonts(isOfflineMode);
        //    //Translator.TranslateRemonts(remonts);
        //    return remonts;
        //}



        //public List<BuildingType> GetBuildingTypes(bool isOfflineMode)
        //{
        //    var buildingTypes = BuildingTypeManager.GetBuildingTypes(isOfflineMode);
        //    //Translator.TranslateBuildingTypes(buildingTypes);
        //    return buildingTypes;
        //}

        //public List<OrderType> GetOrderTypesForEstate(User user, bool isOfflineMode)
        //{
        //    var orderTypes = OrderTypeManager.GetOrderTypesForEstate(user, isOfflineMode);
        //    //Translator.TranslateOrderTypes(orderTypes);
        //    return orderTypes;
        //}

        //public List<EstateType> GetEstateTypes(EsUser user, bool isOfflineMode)
        //{
        //    var estateTypes = EstateTypeManager.GetEstateTypes(user, isOfflineMode);
        //    //Translator.TranslateEstateTypes(estateTypes);
        //    return estateTypes;
        //}

        //public List<Currency> GetCurrencies(bool isOfflineMode)
        //{
        //    return CurrencyManager.GetCurrencies(isOfflineMode);
        //}

        //public bool UpdateEstate(Estate estate, ref StringBuilder errorMessage)
        //{
        //    return EstateManager.Update(estate, ref errorMessage);
        //}

        //public bool UpdateImages(int estateId, ObservableCollection<EstateImage> estateImages, ref StringBuilder errorMessage)
        //{
        //    return ImageManager.UpdateImages(estateId, estateImages, ref errorMessage);
        //}

        //public int SaveImages(int estateId, ObservableCollection<EstateImage> estateImages, ref StringBuilder errorMessage)
        //{
        //    return ImageManager.SaveImages(estateId, estateImages.ToList(), ref errorMessage);
        //}

        //public int SaveRealEstate(Estate estate, ref StringBuilder errorMessage)
        //{
        //    return EstateManager.SaveRealEstate(estate, ref errorMessage);
        //}

        //public List<Project> GetProjects(bool isOfflineMode)
        //{
        //    var projects = ProjectManager.GetProjects(isOfflineMode);
        //    //Translator.TranslateProjects(projects);
        //    return projects;
        //}

        //public List<Street> GetStreets(Region region, bool isOfflineMode)
        //{
        //    var streets = StreetManager.GetStreets(region, isOfflineMode);
        //    //Translator.TranslateStreets(streets);
        //    return streets;
        //}

        //public ObservableCollection<Estate> GetLastAdded20RealEstates(User user, bool isOfflineMode)
        //{
        //    var lastAdded20RealEstates = EstateManager.GetLastAdded20RealEstates(user, isOfflineMode);
        //    if (lastAdded20RealEstates != null)
        //    {
        //        return new ObservableCollection<Estate>(lastAdded20RealEstates);
        //    }

        //    return new ObservableCollection<Estate>();
        //}

        //public bool DeleteEstate(Estate estate)
        //{
        //    return EstateManager.Delete(estate);
        //}

        //public ObservableCollection<Estate> GetSearchedRealEstates(RealEstateSearchParameters searchParameters, User user, out int totalItems, bool isOfflineMode)
        //{
        //    return EstateManager.GetSearchedRealEstates(searchParameters, user, out totalItems, isOfflineMode);
        //}

        //public void RemoveImage(int imageId)
        //{
        //    ImageManager.RemoveImage(imageId);
        //}

        ///// <summary>
        ///// Returns regions
        ///// </summary>
        ///// <param name="cityID">ID of City (1=Yerevan)</param>
        ///// <param name="user">Logged in user</param>
        ///// <returns></returns>
        //public List<Region> GetRegions(int cityID, User user, bool isOfflineMode)
        //{
        //    var regions = RegionManager.GetRegions(cityID, user, isOfflineMode);
        //    //Translator.TranslateRegions(regions);
        //    return regions;
        //}

        //public List<Street> GetStreets(bool isOfflineMode)
        //{
        //    var streets = StreetManager.GetStreets(isOfflineMode);
        //    //Translator.TranslateStreets(streets);
        //    return streets;
        //}

        //public ObservableCollection<Estate> GetEstatesByIDs(List<int> ids, bool isOfflineMode, User user)
        //{
        //    List<Estate> list = EstateManager.GetEstatesByIDs(ids, isOfflineMode, user);
        //    return new ObservableCollection<Estate>(list);
        //}

        //public List<State> GetStates(User user, bool isOfflineMode)
        //{
        //    List<State> states = StateManager.GetStates(user, isOfflineMode);
        //    //Translator.TranslateStates(states);
        //    return states;
        //}

        //public List<EarthPlaceTypeEntity> GetEarthPlaceTypes()
        //{
        //    var earthPlaceTypes = EarthPlaceTypeEntity.GetEarthPlaceTypes();
        //    Translator.TranslateEarthPlaceTypes(earthPlaceTypes);
        //    return earthPlaceTypes;
        //}

        //public List<GarageTypeEntity> GetGarageTypes()
        //{
        //    List<GarageTypeEntity> garageTypes = GarageTypeEntity.GetGarageTypes();
        //    Translator.TranslateGarageTypes(garageTypes);
        //    return garageTypes;
        //}

        //public List<OfficePlaceTypeEntity> GetOfficePlaceTypes()
        //{
        //    List<OfficePlaceTypeEntity> officeTypes = OfficePlaceTypeEntity.GetOfficePlaceTypes();
        //    Translator.TranslateOfficeTypes(officeTypes);
        //    return officeTypes;
        //}

        //public List<ReportTypeEntity> GetReportTypes()
        //{
        //    List<ReportTypeEntity> reportTypes = ReportTypeEntity.GetReportTypes();
        //    Translator.TranslateReportTypes(reportTypes);
        //    return reportTypes;
        //}

        //public List<Role> GetRoles()
        //{
        //    var roles = Role.GetRoles();
        //    Translator.TranslateRoles(roles);
        //    return roles;
        //}

        //public ObservableCollection<EsUsers> GetUsers(bool isOfflineMode)
        //{
        //    var users = UsersManager.GetUsers(isOfflineMode);
        //    //Translator.TranslateUsers(users);
        //    return new ObservableCollection<EsUsers>(users);
        //}

        //public bool AddUser(EsUsers user)
        //{
        //    return UsersManager.AddUser(user);
        //}

        //public bool UpdateUser(EsUser user)
        //{
        //    return UserManager.UpdateUser(user);
        //}

        //public EsUser Login(string userName, string password, bool isOfflineMode)
        //{
        //    var user = UserManager.Login(userName, password, isOfflineMode);
        //    //Translator.TranslateUsers(new List<User> { user });
        //    return user;
        //}

        //public ObservableCollection<NeededEstate> GetDemandsByBroker(int brokerID, bool isOfflineMode)
        //{
        //    List<NeededEstate> demands = DemandManager.GetDemandsByBroker(brokerID, isOfflineMode);
        //    return new ObservableCollection<NeededEstate>(demands);
        //}

        //public ObservableCollection<NeededEstate> GetAllDemands(EsUser user, DemandSearchCriteria criteria, bool isOfflineMode)
        //{
        //    List<NeededEstate> demands = DemandManager.GetAllDemands(user, criteria, isOfflineMode);
        //    return new ObservableCollection<NeededEstate>(demands);
        //}

        //public bool SaveDemand(NeededEstate demand)
        //{
        //    return DemandManager.SaveDemand(demand);
        //}

        //public bool UpdateDemand(NeededEstate demand)
        //{
        //    return DemandManager.UpdateDemand(demand);
        //}

        //public List<EsUser> GetBrokers(bool isOfflineMode)
        //{
        //    var brokers = UserManager.GetBrokers(isOfflineMode);
        //    //Translator.TranslateUsers(brokers);
        //    return brokers;
        //}

        //public int GetEstatesCount(bool isOfflineMode)
        //{
        //    return EstateManager.GetEstatesCount(isOfflineMode);
        //}

        //public List<EstateType> GetEstateTypes(IEnumerable<int> estateTypeIDs, bool isOfflineMode)
        //{
        //    try
        //    {
        //        DataClassesDataContext db = new DataClassesDataContext(GetConnectionString(isOfflineMode));
        //        return db.EstateTypes.Where(s => estateTypeIDs.Contains(s.EstateTypeID)).ToList();
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        //public void AddToFavoriteEstates(int userID, List<int> estateIDs)
        //{
        //    EstateManager.AddToFavoriteEstates(userID, estateIDs);
        //}

        //public ObservableCollection<FavoriteEstate> GetFavoriteEstates(int userID, bool isOfflineMode)
        //{
        //    return new ObservableCollection<FavoriteEstate>(EstateManager.GetFavoriteEstates(userID, isOfflineMode));
        //}

        //public void DeleteFavoriteEstate(List<int> favoriteEstatesIDs)
        //{
        //    EstateManager.DeleteFavoriteEstate(favoriteEstatesIDs);
        //}

        //public bool DeleteDemand(NeededEstate demand)
        //{
        //    return DemandManager.DeleteDemand(demand);
        //}

        //public int DeleteDemands(IEnumerable<NeededEstate> demands)
        //{
        //    return DemandManager.DeleteDemands(demands);
        //}

        //public bool UpdateBuildingType(BuildingType buildingType)
        //{
        //    return BuildingTypeManager.UpdateBuildingType(buildingType);
        //}

        //public bool AddBuildingType(BuildingType buildingType)
        //{
        //    return BuildingTypeManager.AddBuildingType(buildingType);
        //}

        //public bool DeleteBuildingType(BuildingType buildingType)
        //{
        //    return BuildingTypeManager.DeleteBuildingType(buildingType);
        //}

        //public bool DeleteState(State state)
        //{
        //    return StateManager.DeleteState(state);
        //}

        //public bool UpdateState(State state)
        //{
        //    return StateManager.UpdateState(state);
        //}

        //public bool AddState(State state)
        //{
        //    return StateManager.AddState(state);
        //}

        //public bool UpdateRemont(Remont remont)
        //{
        //    return RemontManager.UpdateRemont(remont);
        //}

        //public bool AddRemont(Remont remont)
        //{
        //    return RemontManager.AddRemont(remont);
        //}

        //public bool DeleteRemont(Remont remont)
        //{
        //    return RemontManager.DeleteRemont(remont);
        //}

        //public bool DeleteProject(Project project)
        //{
        //    return ProjectManager.DeleteProject(project);
        //}

        //public bool UpdateProject(Project project)
        //{
        //    return ProjectManager.UpdateProject(project);
        //}

        //public bool AddProject(Project project)
        //{
        //    return ProjectManager.AddProject(project);
        //}

        //public bool DeleteCurrency(Currency currency)
        //{
        //    return CurrencyManager.DeleteCurrency(currency);
        //}

        //public bool AddCurrency(Currency currency)
        //{
        //    return CurrencyManager.AddCurrency(currency);
        //}

        //public bool UpdateCurrency(Currency currency)
        //{
        //    return CurrencyManager.UpdateCurrency(currency);
        //}

        //public bool DeleteCity(City city)
        //{
        //    return CityManager.DeleteCity(city);
        //}

        //public bool UpdateCity(City city)
        //{
        //    return CityManager.UpdateCity(city);
        //}

        //public bool AddCity(City city)
        //{
        //    return CityManager.AddCity(city);
        //}

        //public bool DeleteRegion(Region region)
        //{
        //    return RegionManager.DeleteRegion(region);
        //}

        //public bool AddRegion(Region region)
        //{
        //    return RegionManager.AddRegion(region);
        //}

        //public bool UpdateRegion(Region region)
        //{
        //    return RegionManager.UpdateRegion(region);
        //}

        //public bool DeleteStreet(Street street)
        //{
        //    return StreetManager.DeleteStreet(street);
        //}

        //public bool UpdateStreet(Street street)
        //{
        //    return StreetManager.UpdateStreet(street);
        //}

        //public bool AddStreet(Street street)
        //{
        //    return StreetManager.AddStreet(street);
        //}

        //public bool CreateDatabaseBackup(string backupFilePath, ref string errorMessage)
        //{
        //    if (File.Exists(backupFilePath))
        //    {
        //        try
        //        {
        //            File.Delete(backupFilePath);
        //        }
        //        catch { }
        //    }
        //    return DatabaseTools.BackupDatabase(backupFilePath, ref errorMessage);
        //}

        //public bool RestoreDatabase(string backupFilePath, string dataFolderPath, ref string errorMessage)
        //{
        //    //if (!File.Exists(backupFilePath))
        //    //{
        //    //    MessageBox.Show(CultureResources.Inst["PleaseSelectArchiveFile"]);
        //    //    return false;
        //    //}
        //    //TODO: move it to UI

        //    return DatabaseTools.RestoreDatabase(backupFilePath, dataFolderPath, ref errorMessage);
        //}

       
        public bool CreateDatabaseBackup(string backupFilePath,string con, ref string errorMessage)
        {
            if (File.Exists(backupFilePath))
            {
                try
                {
                    File.Delete(backupFilePath);
                }
                catch { }
            }
            return DatabaseTools.BackupDatabase(backupFilePath , con, ref errorMessage);
        }

        public bool RestoreDatabase(string backupFilePath, string dataFolderPath, ref string errorMessage)
        {
            if (!File.Exists(backupFilePath))
            {
                MessageBox.Show(CultureResources.Inst["PleaseSelectArchiveFile"]);
                return false;
            }
            //TODO:

            return DatabaseTools.RestoreDatabase(backupFilePath, dataFolderPath, ref errorMessage);
        }
        public bool DeleteUser(EsUsers user)
        {
            return false;
        }

        //public List<SelledEstate> GetSoldEstates(SoldRentedEstateCriteria SearchCriteria, bool isOfflineMode)
        //{
        //    return EstateManager.GetSoldEstates(SearchCriteria, isOfflineMode);
        //}

        //public bool EstateMarkAsSelled(SelledEstate estate)
        //{
        //    return EstateManager.MarkAsSelled(estate);
        //}

        //public bool UpdateSelledEstate(SelledEstate sellEstate)
        //{
        //    return EstateManager.UpdateSelledEstate(sellEstate);
        //}

        //public bool EstateMarkAsRented(RentedEstate rentedEstate)
        //{
        //    return EstateManager.MarkAsRented(rentedEstate);
        //}

        //public bool UpdateRentedEstate(RentedEstate rentEstate)
        //{
        //    return EstateManager.UpdateRentedEstate(rentEstate);
        //}

        //public List<RentedEstate> GetRentedEstates(SoldRentedEstateCriteria searchCriteria, bool isOfflineMode)
        //{
        //    return EstateManager.GetRentedEstates(searchCriteria, isOfflineMode);
        //}

        //public bool UploadEstate(Estate estate, IEnumerable<EstateImage> images)
        //{
        //    EstateDto dto = GetEstateDto(estate);

        //    Stream[] streams = images.Select(s => (Stream)new FileStream(s.ImageFilePath, FileMode.Open, FileAccess.Read)).ToArray();

        //    //using (UploadEstateService.UploadServiceClient client = new UploadEstateService.UploadServiceClient())
        //    //{
        //    //    List<int> imagesIDs = new List<int>();

        //    //    client.UploadEstate(string.Empty, dto);
        //    //}
        //    //TODO: upload estate
        //    return true;
        //}

        //public EstateDto GetEstateDto(Estate estate)
        //{
        //    EstateDto dto = new EstateDto();

        //    return dto;
        //}

        //public bool ReturnSelledEstateToEstatesList(SelledEstate estate)
        //{
        //    return EstateManager.ReturnSelledEstateToEstatesList(estate);
        //}

        //public bool ReturnRentedEstateToEstatesList(RentedEstate rentedEstate)
        //{
        //    return EstateManager.ReturnRentedEstateToEstatesList(rentedEstate);
        //}

        //public int GetAllDemansCount(bool isOfflineMode)
        //{
        //    return DemandManager.GetAllDemandsCount(isOfflineMode);
        //}
        //public Currency GetCurrency(int id, bool isOfflineMode)
        //{
        //    return CurrencyManager.GetCurrency(id, isOfflineMode);
        //}

        //public List<NeededEstate> GetDemandsForEstate(Estate estate, EsUser user, bool isOfflineMode)
        //{
        //    return DemandManager.GetDemandsForEstate(estate, user, isOfflineMode);
        //}

        //public bool ValidateUser(string userName)
        //{
        //    return UserManager.ValidateUserName(userName);
        //}

        //public bool CheckEstate(Estate realEstate)
        //{
        //    return EstateManager.CheckEstate(realEstate);
        //}

        //public List<SignificanceOfTheUse> GetSignificanceOfTheUses(EstateType estateType, bool isOfflineMode)
        //{
        //    return OperationalSignificanceManager.GetSignificanceOfTheUses(estateType, isOfflineMode);
        //}

        //public List<OperationalSignificance> GetOperationalSignificances(SignificanceOfTheUse significanceOfTheUse, bool isOfflineMode)
        //{
        //    return OperationalSignificanceManager.GetOperationalSignificances(significanceOfTheUse, isOfflineMode);
        //}

        //public bool ValidateEstateCode(Estate estate)
        //{
        //    return EstateManager.ValidateEstateCode(estate);
        //}

        //public List<Roofing> GetRoofings(bool isOfflineMode)
        //{
        //    return RoofingManager.GetRoofings(isOfflineMode);
        //}

        //public bool DeleteRoofing(Roofing roofing)
        //{
        //    return RoofingManager.DeleteRoofing(roofing);
        //}

        //public bool UpdateRoofing(Roofing roofing)
        //{
        //    return RoofingManager.UpdateRoofing(roofing);
        //}

        //public bool AddRoofing(Roofing roofing)
        //{
        //    return RoofingManager.AddRoofing(roofing);
        //}

        //public bool DeleteSignificanceOfTheUse(SignificanceOfTheUse bt)
        //{
        //    return OperationalSignificanceManager.DeleteSignificanceOfTheUse(bt);
        //}

        //public bool UpdateSignificanceOfTheUse(SignificanceOfTheUse significanceOfTheUse)
        //{
        //    return OperationalSignificanceManager.UpdateSignificanceOfTheUse(significanceOfTheUse);
        //}

        //public bool AddSignificanceOfTheUse(SignificanceOfTheUse significanceOfTheUse)
        //{
        //    return OperationalSignificanceManager.AddSignificanceOfTheUse(significanceOfTheUse);
        //}

        //public bool UpdateoperationalSignificance(OperationalSignificance operationalSignificance)
        //{
        //    return OperationalSignificanceManager.UpdateOperationalSignificance(operationalSignificance);
        //}

        //public bool AddOperationalSignificance(OperationalSignificance operationalSignificance)
        //{
        //    return OperationalSignificanceManager.AddOperationalSignificance(operationalSignificance);
        //}

        //public bool DeleteOperationalSignificance(OperationalSignificance operationalSignificance)
        //{
        //    return OperationalSignificanceManager.DeleteOperationalSignificance(operationalSignificance);
        //}

        //public void EstateMarkAsUploaded(Estate estate)
        //{
        //    EstateManager.ChangeEstateUploadMark(estate, true);
        //}

        //public List<Estate> CheckRentedEstates()
        //{
        //    return EstateManager.MoveExpiredEstatesFromRentedEstates();
        //}

        //public int SaveVideos(int estateId, ObservableCollection<string> estateVideos, ref StringBuilder errorMessage)
        //{
        //    return ImageManager.SaveVideos(estateId, estateVideos, ref errorMessage);
        //}

        //public void DeleteVideo(string videoPath)
        //{
        //    if (string.IsNullOrEmpty(videoPath)) return;
        //    ImageManager.DeleteVideo(videoPath);
        //}

        //public ObservableCollection<Convenient> GetConvenients(bool isOfflineMode)
        //{
        //    return EstateManager.GetConvenients(isOfflineMode);
        //}

        //public ObservableCollection<NeededEstate> GetDemands(DemandSearchCriteria criteria)
        //{
        //    DataClassesDataContext db = new DataClassesDataContext();
        //    var demands = db.NeededEstates.Where(s => s.IsDeleted == null || s.IsDeleted == false).AsQueryable();
        //    if (criteria == null) return new ObservableCollection<NeededEstate>(demands);
        //    if (!string.IsNullOrEmpty(criteria.Name))
        //    {
        //        demands = demands.Where(s => s.SellerName.Contains(criteria.Name));
        //    }
        //    if (!string.IsNullOrEmpty(criteria.Phone))
        //    {
        //        demands = demands.Where(s => s.Telephone1.Contains(criteria.Phone) || s.Telephone2.Contains(criteria.Phone));
        //    }
        //    return new ObservableCollection<NeededEstate>(demands);
        //}

        //public bool CheckDemandExisting(NeededEstate demand)
        //{
        //    DataClassesDataContext db = new DataClassesDataContext();
        //    var numbers = new List<string>();
        //    if (!string.IsNullOrEmpty(demand.Telephone1))
        //    {
        //        numbers.AddRange(demand.Telephone1.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
        //    }
        //    if (!string.IsNullOrEmpty(demand.Telephone2))
        //    {
        //        numbers.AddRange(demand.Telephone2.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
        //    }
        //    return numbers.Select(number => db.NeededEstates.Where(s => s.Telephone1.Contains(number) || s.Telephone2.Contains(number))).Any(demands => demands.Count() > 0);
        //}

        //public ObservableCollection<BlackListItem> GetBlackListItems(BlackListSearchCriteria searchCriteria, bool isOfflineMode)
        //{
        //    return BlackListManager.GetBlackListItems(searchCriteria, isOfflineMode);
        //}

        //public bool AddBlackListItem(BlackListItem number)
        //{
        //    return BlackListManager.AddBlackListItem(number);
        //}

        //public bool UpdateBlackListNumber(BlackListNumber number)
        //{
        //    return BlackListManager.UpdateBlackListNumber(number);
        //}

        //public bool DeleteBlackListNumber(BlackListNumber number)
        //{
        //    return BlackListManager.DeleteBlackListNumber(number);
        //}

        //public bool UpdateBlackListItem(BlackListItem blackListItem)
        //{
        //    return BlackListManager.UpdateBlackListItem(blackListItem);
        //}

        //public bool DeleteBlackListItem(BlackListItem item)
        //{
        //    return BlackListManager.DeleteBlackListItem(item);
        //}

        //public bool AddEstateShowInfo(EstatesAndDemand showInfo)
        //{
        //    return EstatesAndDemandManager.AddShowInfo(showInfo);
        //}

        //public ObservableCollection<Estate> SearchEstatesForShowDemand(DemandSearchCriteria searchParameters, bool isOfflineMode)
        //{
        //    return EstateManager.SearchEstatesForShowDemand(searchParameters, isOfflineMode);
        //}

        //public ObservableCollection<EstatesAndDemand> GetAllDemandsWithShowedEstates(User user, DemandSearchCriteria demandSearchCriteria, bool isOfflineMode)
        //{
        //    return EstatesAndDemandManager.GetAllDemandsWithShowedEstates(user, demandSearchCriteria, isOfflineMode);
        //}

        //public ObservableCollection<EstatesAndDemand> GetShowedToClientsEstates(RealEstateSearchParameters realEstateSearchParameters, User user, bool isOfflineMode)
        //{
        //    return EstatesAndDemandManager.GetShowedToClientsEstates(realEstateSearchParameters, user, isOfflineMode);
        //}

        //public ObservableCollection<EstatesAndDemand> GetShowedEstatesForDemand(int demandID, bool isOfflineMode)
        //{
        //    return EstatesAndDemandManager.GetShowedEstatesForDemand(demandID, isOfflineMode);
        //}

        //public ObservableCollection<EstatesAndDemand> GetShowedClientsForEstate(int estateID, bool isOfflineMode)
        //{
        //    return EstatesAndDemandManager.GetShowedClientsForEstate(estateID, isOfflineMode);
        //}

        //public bool DeleteShoingInfo(EstatesAndDemand item)
        //{
        //    return EstatesAndDemandManager.DeleteShoingInfo(item);
        //}

        //public bool DeleteRentedEstate(RentedEstate rentedEstate)
        //{
        //    return EstateManager.DeleteRentedEstate(rentedEstate);
        //}

        //public ReportData GetBrokerReportData(StatisticSearchCriteria criteria, bool isOfflineMode)
        //{
        //    return StatisticsManager.GetBrokerReportData(criteria, isOfflineMode);
        //}

        //public ReportData GetAgencyReportData(StatisticSearchCriteria searchCriteria, bool isOfflineMode)
        //{
        //    return StatisticsManager.GetAgencyReportData(searchCriteria, isOfflineMode);
        //}

        //public DateTime GetFirstAddedObjectDate(bool isOfflineMode)
        //{
        //    return StatisticsManager.GetFirstAddedObjectDate(isOfflineMode);
        //}

        //public List<ReportData> GetReportByBrokers(StatisticSearchCriteria searchCriteria, bool isOfflineMode)
        //{
        //    return StatisticsManager.GetReportByBrokers(searchCriteria, isOfflineMode);
        //}

        //public List<User> GetUsersByRights(User user, bool isOfflineMode)
        //{
        //    return UserManager.GetUsersByRights(user, isOfflineMode);
        //}

        //public bool DeleteConvenient(Convenient convenient)
        //{
        //    return ConvenientManager.DeleteConvenient(convenient);
        //}

        //public bool UpdateConvenient(Convenient convenient)
        //{
        //    return ConvenientManager.UpdateConvenient(convenient);
        //}

        //public bool AddConvenient(Convenient convenient)
        //{
        //    return ConvenientManager.AddConvenient(convenient);
        //}

        //public ObservableCollection<Estate> GetDeletedEstates(SoldRentedEstateCriteria searchCriteria, bool offlineMode)
        //{
        //    return EstateManager.GetDeletedEstates(searchCriteria, offlineMode);
        //}

        //public bool ReturnDeletedEstateToEstatesList(Estate estate)
        //{
        //    return EstateManager.ReturnDeletedEstateToEstatesList(estate);
        //}

        //public ObservableCollection<Country> GetCountries(bool offlineMode)
        //{
        //    return CountryManager.GetCountries(offlineMode);
        //}

        //public bool UpdateCountry(Country country)
        //{
        //    return CountryManager.UpdateCountry(country);
        //}

        //public bool AddCountry(Country country)
        //{
        //    return CountryManager.AddCountry(country);
        //}

        //public bool DeleteCountry(Country country)
        //{
        //    return CountryManager.DeleteCountry(country);
        //}

        //public void DeleteFavoriteDemands(int userID, IEnumerable<NeededEstate> demands)
        //{
        //    using (var db = new DataClassesDataContext())
        //    {
        //        var ids = demands.Select(s => s.ID);
        //        var favClients = db.FavoriteClients.Where(s => s.UserID == userID && ids.Contains(s.ClientID));
        //        db.FavoriteClients.DeleteAllOnSubmit(favClients);
        //        db.SubmitChanges();
        //    }

        //}
    }
}
