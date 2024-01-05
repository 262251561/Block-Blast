using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class IntStringMap
{
    private static IntStringMap s_Instance;
    public static IntStringMap instance
    {
        get
        {
            if (s_Instance == null)
                s_Instance = new IntStringMap();

            return s_Instance;
        }
    }

    private Dictionary<int, string> __intMap;

    public IntStringMap()
    {
        __intMap = new Dictionary<int, string>();
    }

    public string GetInt(int nValue)
    {
        string str = string.Empty;
        if(!__intMap.TryGetValue(nValue, out str))
        {
            str = nValue.ToString();
            __intMap.Add(nValue, str);
        }

        return str;
    }
}