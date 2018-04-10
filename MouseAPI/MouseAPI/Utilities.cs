using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseAPI
{
    public static class Utilities
    {
        public static List<string> SplitStringIntoParts(this string s, int partLength)
        {
            var list = new List<string>();

            if (!string.IsNullOrEmpty(s) && partLength > 0)
            {
                for (var i = 0; i < s.Length; i += partLength)
                {
                    list.Add(s.Substring(i, Math.Min(partLength, s.Length - i)));
                }
            }
            else list.Add(""+(char)32);
            list[list.Count-1] += new string((char)32, partLength- list.Last().Length);
            return list;
        }
    }
}
