using System;
using System.Collections.Generic;

namespace TH
{
    /// <summary>
    /// 哈希方式存储的数组,提高访问速度
    /// </summary>
    /// <typeparam name="T">class</typeparam>
    public class ArrayHash<T>
    {
        public struct Chunk
        {
            public T item;
            public uint version;
            public int removedState;
        }

        private uint __count;
        private Chunk[] __items;
        private Stack<uint> __freeItems;

        public ArrayHash(int size = 256)
        {
            __count = 0;
            __items = new Chunk[size];
            for(int i=0; i<size; ++i)
            {
                __items[i] = new Chunk { version = 1, removedState = 1 };
            }

            __freeItems = new Stack<uint>();
        }

        public ref T Get(ulong handle)
        {
            return ref this[handle];
        }

        public T GetClone(ulong handle)
        {
            uint index = (uint)(handle >> 32);
            Chunk chunk = __items[index];
            return chunk.item;
        }

        public ref T this[ulong handle]
        {
            get
            {
                uint index = (uint)(handle >> 32);

                ref Chunk chunk = ref __items[index];
                if(chunk.version != (uint)(handle & 0xffffffff))
                {
                    throw new Exception("Arrayhash check error index: " + index + " version: " + ((uint)(handle & 0xffffffff)) + " destVersion: " + chunk.version);
                }

                return ref chunk.item;
            }
        }


        public bool Exists(ulong handle)
        {
            uint index = (uint)(handle >> 32);

            ref Chunk chunk = ref __items[index];
            return chunk.version == (uint)(handle & 0xffffffff) && chunk.removedState == 0;
        }
        
        public ulong AddItem(in T item)
        {
            ulong index = 0;
            ulong entity = 0;
            if (__freeItems.Count > 0)
            {
                index = __freeItems.Pop();

                ref Chunk chunk = ref __items[index];
                ++chunk.version;
                chunk.item = item;
                chunk.removedState = 0;

                entity = (index << 32) | chunk.version;
            }
            else
            {
                if (__count == __items.Length)
                {
                    if((__count << 1) >= int.MaxValue)
                    {
                        throw new Exception("Arrayhash arrayLength too large! error!");
                    }

                    Array.Resize(ref __items, (int)(__count << 1));
                }

                index = __count++;

                ref Chunk chunk = ref __items[index];
                chunk.item = item;
                chunk.removedState = 0;

                entity = (index << 32) | chunk.version;
            }

            return entity;
        }
        public bool RemoveItem(ulong handle)
        {
#if UNITY_EDITOR
            return RemoveItem(handle, true);
#else
            return RemoveItem(handle, false);
#endif
        }

        public bool RemoveItem(ulong handle, bool isCheckRemoveState)
        {
            uint index = (uint)(handle >> 32);

            ref Chunk chunk = ref __items[index];

            if (chunk.version != (uint)(handle & 0xffffffff))
            {
                throw new Exception("Arrayhash check error!");
            }

            if (isCheckRemoveState)
            {
                if (chunk.removedState == 1)
                    throw new Exception("Arrayhash repeat remove error!");
            }
            else
            {
                if (chunk.removedState == 1)
                    return false;
            }

            chunk.removedState = 1;
            __freeItems.Push(index);

            return true;
        }

        public void Clear()
        {
            int size = __items.Length;
            for (int i = 0; i < size; ++i)
            {
                __items[i] = new Chunk { version = 0, removedState = 1 };
            }
            __count = 0;
            __freeItems.Clear();
        }
    }
}
