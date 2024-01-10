using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TH
{
    public class ObjectPool<T>
        where T : class, new()
    {
        private Stack<T> __pool;

        public static ObjectPool<T> s_instance = new ObjectPool<T>();

        public ObjectPool()
        {
            __pool = new Stack<T>();
        }

        public T Allocate()
        {
            T item = null;
            if (__pool.Count > 0)
                item = __pool.Pop();
            else
                item = new T();

            return item;
        }

        public void Free(T item)
        {
            __pool.Push(item);
        }

        public void Dispose()
        {
            __pool = null;
        }
    }
}
