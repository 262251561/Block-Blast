using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    static private List<Transform> __children = new List<Transform>();

    public T GetCT<T>(string nodeName) where T : Component
    {
        __children.Clear();
        gameObject.GetComponentsInChildren(true,__children);
        int i, length = __children.Count;
        for(i=0; i<length; ++i)
        {
            var node = __children[i];
            if (node.name == nodeName)
                return node.GetComponent<T>();
        }

        return null;
    }


    public GameObject Get(string nodeName)
    {
        __children.Clear();
        gameObject.GetComponentsInChildren(true, __children);
        int i, length = __children.Count;
        for (i = 0; i < length; ++i)
        {
            var node = __children[i];
            if (node.name == nodeName)
                return node.gameObject;
        }

        return null;
    }
}