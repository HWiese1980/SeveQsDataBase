using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeveQsDataBase
{
    using System.Reflection;

    public class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(PropertyInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
