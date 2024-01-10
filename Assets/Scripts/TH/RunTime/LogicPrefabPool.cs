using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TH
{
    public class LogicPrefabPool<T>
        where T  : Component
    {
        private T __prefab;
        private Transform __parent;
        private Stack<T> __pool;

        public LogicPrefabPool(T prefab, Transform parent)
        {
            __prefab = prefab;
            __parent = parent;
            __pool = new Stack<T>();
        }

        public void Preheat(int count)
        {
            if (count <= 0)
                count = 32;

            T com = __prefab;

            while(count > 0)
            {
                com = UnityEngine.Object.Instantiate(__prefab);
                com.transform.SetParent(__parent);
                com.gameObject.SetActive(false);
                __pool.Push(com);

                --count;
            }
        }

        public void FreePool(float percent)
        {
            int length = Mathf.CeilToInt( __pool.Count * percent );
            while(length > 0)
            {
                GameObject.Destroy(__pool.Pop().gameObject);
                --length;
            }
        }

        public void Clear()
        {
            while (__pool.Count > 0)
                GameObject.Destroy(__pool.Pop().gameObject);
        }

        public T Allocate()
        {
            T com = null;
            if (__pool.Count > 0)
                com = __pool.Pop();
            else
            {
                Preheat(32);
                return Allocate();
            }

            com.transform.SetParent(null);
            com.gameObject.SetActive(true);

            return com;
        }

        public void Free(T com)
        {
            com.gameObject.SetActive(false);
            com.transform.SetParent(__parent);
            com.transform.localPosition = Vector3.zero;
            com.transform.localRotation = Quaternion.identity;
            com.transform.localScale = Vector3.one;

            __pool.Push(com);
        }
    }
}
