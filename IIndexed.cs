using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace SeveQsDataBase
{
    public interface IIndexed
    {
        int Index { get; set; }
        IEnumerable<IIndexed> IndexGroup { get; set; }
    }
}