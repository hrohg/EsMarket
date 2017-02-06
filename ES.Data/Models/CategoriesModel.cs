using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES.Data.Models
{
    public class CategoriesModel
    {
        #region External properties
        public int Id { get; set; }
        public int? PartnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        #endregion
    }
}
