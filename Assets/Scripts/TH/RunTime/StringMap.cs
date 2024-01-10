using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TH
{
    public class StringMap
    {
        private List<string> __key2String;
        private Dictionary<string, int> __hashMap;
        public static readonly int NullOrEmpty = 0;

        private static StringMap __instance;
        public static StringMap instance
        {
            get
            {
                if (__instance == null)
                    __instance = new StringMap();

                return __instance;
            }
        }

        public StringMap()
        {
            __key2String = new List<string>();
            __hashMap = new Dictionary<string, int>();
        }

        public int GetKey(string value)
        {            
            if (string.IsNullOrEmpty(value))
                return NullOrEmpty;

            if( !__hashMap.TryGetValue(value, out var key) )
            {
                key = __key2String.Count;
                __hashMap.Add(value, key);
                __key2String.Add(value);
            }

            return key + 1;
        }

        public string GetValue(int key)
        {
            if (key <= 0)
                return string.Empty;

            return __key2String[key-1];
        }
    }
}
