using System;
using System.Collections.Generic;
using ES.Data.Models.Bases;

namespace ES.Data.Models.EsModels
{
    public class EsCategoriesModel : TreeViewItemBaseModel
    {
        #region Internal properties
        #endregion

        #region External properties

        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public EsCategoriesModel Parent { get; set; }
        public List<EsCategoriesModel> Children { get; set; }
        #region Description and name

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (string.Equals(value, Name)) return;
                _name = value ?? string.Empty;
            }
        }

        private string _description;

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (string.Equals(value, _description)) return;
                _description = value;
            }
        }

        public string ShortDescription
        {
            get
            {
                return string.Format("{0} {1}", HcDcs, !string.IsNullOrWhiteSpace(Name) && Name.Length > 150
                    ? Name.Substring(0, 150) : Name);
            }
        }
        #endregion Description and name

        public string HcDcs { get; set; }
        public string Logo { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastModificationDate { get; set; }
        public int LastModifierId { get; set; }
        #endregion External properties

        #region Constructors
        public EsCategoriesModel()
        {
            Id = Guid.NewGuid();
            Initialize();
        }
        public EsCategoriesModel(Guid id)
        {
            Id = id;
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Children = new List<EsCategoriesModel>();
        }
        #endregion
    }
}
