using System.Collections.Generic;

namespace ES.Data.Models.Bases
{
    public class DynamicModel<T, F>
    {
        private IDictionary<T, F> _fields;

        public IDictionary<T, F> Fields
        {
            get { return _fields; }
        }
        public DynamicModel()
        {
            _fields = new Dictionary<T, F>();
        }

        
    }
}
