using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Xml.Linq;

namespace SeveQsDataBase
{
    using System.Globalization;
    using System.Text;

    public static class Helper
    {
        public static string Indent(this string str, int steps, char fillChar = '\t')
        {
            var sb = new StringBuilder();
            sb.Append(fillChar, steps);
            string indentation = sb.ToString();
            return indentation + str;
        }

        public static string GetValidISBN(string ISBN)
        {
            ISBN = ISBN.Trim().Replace("-", string.Empty).Replace(".", string.Empty);
            if (ISBN.Length < 10)
            {
                return null;
            }

            ISBN = ISBN.Substring(ISBN.Length - 10, 9);
            ISBN += checksum(ISBN).ToString(CultureInfo.InvariantCulture);
            return ISBN;
        }

        static int checksum(string isbn)
        {
            int csum = 0;
            for (int i = 0; i < 9; i++)
            {
                csum += (i + 1) * System.Convert.ToInt32(isbn[i].ToString());
            }
            csum = csum % 11;
            return csum;
        }
        public static T AttValue<T>(this XElement elm, string attributeName, string NullValue = "NULL")
        {
            var attr = elm.Attribute(attributeName);
            if (attr == null || attr.Value == NullValue) return default(T);
            return (T) Convert.ChangeType(attr.Value, typeof (T));
        }

        public static string Times(this string input, int count)
        {
            string ret = "";
            for (int i = 0; i < count; i++) ret += input;
            return ret;
        }
    }
}
