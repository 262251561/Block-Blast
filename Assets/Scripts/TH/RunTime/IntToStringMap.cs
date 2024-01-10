using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TH
{
    public class IntToStringMap
    {
        private static IntToStringMap s_instance;
        public static IntToStringMap instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new IntToStringMap();

                return s_instance;
            }
        }

        private Dictionary<int, string> __intCache;

        public IntToStringMap()
        {
            __intCache = new Dictionary<int, string>();
        }

        public string GetCacheString(int key)
        {
            if(!__intCache.TryGetValue(key, out var value))
            {
                value = key.ToString();
                __intCache.Add(key, value);
            }

            return value;
        }
    }

    public static class StingHelperEx
    {
        public static string ToCacheString(this int refValue)
        {
            return IntToStringMap.instance.GetCacheString(refValue);
        }
    }
}
