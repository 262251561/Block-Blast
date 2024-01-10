using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace TH
{
    public class ArrayPoolList<T>
    {
        internal struct HashNode
        {
            public int index;
        }

        public struct ListItemNode
        {
            public T data;
            public ulong hashHandle;
        }

        public struct Enumerator
        {
            private int __index;
            private ArrayPoolList<T> owner;
            private int __currentCount;
            private ListItemNode[] __list;

            public ref T CurrentData
            {
                get
                {
                    return ref __list[__index].data;
                }
            }

            public T CurrentCloneData
            {
                get
                {
                    return __list[__index].data;
                }
            }

            public ulong CurrentHandle
            {
                get
                {
                    return __list[__index].hashHandle;
                }
            }

            internal Enumerator(ArrayPoolList<T> container)
            {
                owner = container;
                __list = owner.__nodeArray;
                __currentCount = container.__currentCount;
                __index = -1;
            }

            public void SetValue(in T item)
            {
                ref var sourceItem = ref __list[__index];
                sourceItem.data = item;
                ++owner.version;
            }

            public bool MoveNext()
            {
                return ++__index < __currentCount;
            }
        }

        public int version
        {
            private set;
            get;
        }

        private int __currentCount;
        private ListItemNode[] __nodeArray;
        private ArrayHash<HashNode> __hashArray;

        public ArrayPoolList()
        {
            version = 0;
            __hashArray = new ArrayHash<HashNode>();
            __currentCount = 0;
            __nodeArray = new ListItemNode[256];
        }

        public ulong AddItem(in T item)
        {
            ++version;
            var node = new HashNode();
            node.index = __currentCount;

            if(__currentCount == __nodeArray.Length)
            {
                var newNodeArray = new ListItemNode[__nodeArray.Length << 1];
                for (int i = 0; i < __nodeArray.Length; ++i)
                    newNodeArray[i] = __nodeArray[i];

                __nodeArray = newNodeArray;
            }

            ulong handle = __hashArray.AddItem(node);
            __nodeArray[__currentCount++] = new ListItemNode { data = item, hashHandle = handle };

            return handle; 
        }


        public ref T this[ulong handle]
        {
            get
            {
                return ref Get(handle);
            }
        }

        public ref T Get(ulong handle)
        {
            ++version;
            return ref __nodeArray[__hashArray[handle].index].data;
        }

        public T GetCloneData(ulong handle)
        {
            ++version;
            return __nodeArray[__hashArray[handle].index].data;
        }

        public void SetData(ulong handle, ref T item)
        {
            ++version;
            var index = __hashArray[handle].index;
            ref var sourceItem = ref __nodeArray[index];
            sourceItem.data = item;
        }

        public bool IsExist(ulong handle)
        {
            return __hashArray.Exists(handle);
        }

        public void RemoveItem(ulong handle)
        {
            ++version;
            var removedNode = __hashArray[handle];
#if UNITY_EDITOR
            __hashArray.RemoveItem(handle, true);
#else
            __hashArray.RemoveItem(handle, false);
#endif

            if(__currentCount == 1)
                __currentCount = 0;
            else
            {
                ref var lastNode = ref __nodeArray[--__currentCount];
                ref var lastHashNode = ref __hashArray[lastNode.hashHandle];
                lastHashNode.index = removedNode.index;
                __nodeArray[removedNode.index] = lastNode;
            }
        }

        public void Clear()
        {
            ++version;
            __hashArray.Clear();
            __currentCount = 0;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
