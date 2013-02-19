using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace SeveQsDataBase
{
    public class ValueEventArgs<T> : EventArgs
    {
        public ValueEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}