using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace SeveQsDataBase
{
    public interface IHasParent
    {
        DataBase Parent { get; set; }
        IList<DataBase> OtherAncestors { get; }
    }
}