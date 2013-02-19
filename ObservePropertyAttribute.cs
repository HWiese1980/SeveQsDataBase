using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace SeveQsDataBase
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ObservePropertyAttribute : Attribute
    {
        public string Dependency { get; set; }

        public static bool Logging { get; set; }

        static ObservePropertyAttribute()
        {
            Logging = false;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IsGlobalPropertyAttribute : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RefreshPropertyAttribute : Attribute
    {
        public string Tag { get; set; }
    }

}
