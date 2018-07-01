using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ES.Common.ViewModels.Base;
using ES.Data.Models;

namespace UserControls.ViewModels.Managers
{
    public class PartnerServiceModel
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        public Guid PartnerId { get; set; }
        public PartnerModel Partner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public int? Port { get; set; }
        public string Key { get; set; }
        #endregion External properties

    }
    public class PartnersServersViewModel : ViewModelBase
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        public PartnerServiceModel PartnerService { get; set; }
        public ObservableCollection<PartnerServiceModel> PartnerServices { get; set; }

        #endregion External properties


    }
}
