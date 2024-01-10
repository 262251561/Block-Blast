/********************************************************************************
*文 件 名： ArugmentPool.cs
*描    述： ArugmentPool的功能说明
*作    者： hejinjiang
*创建时间： 2021.05.17
*修改记录:
*********************************************************************************/

using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Net
{
    public class ArgumentPool
    {
        private int __maxBufferSize;
        private List<NetSocketArgument> __argumentsPool;

        public ArgumentPool(int buffersize)
        {
            __maxBufferSize = buffersize;
            __argumentsPool = new List<NetSocketArgument>();
        }

        public NetSocketArgument AllocateArgument()
        {
            NetSocketArgument args = null;
            lock (__argumentsPool)
            {
                if (__argumentsPool.Count > 0)
                {
                    int lastIndex = __argumentsPool.Count - 1;
                    args = __argumentsPool[lastIndex];
                    __argumentsPool.RemoveAt(lastIndex);
                }
            }

            if (args == null)
            {
                args = new NetSocketArgument();
                args.SetBuffer(new byte[__maxBufferSize], 0, __maxBufferSize);
            }

            return args;
        }

        public void FreeArgument(NetSocketArgument item)
        {
            item.complete_handler = null;

            lock (__argumentsPool)
            {
                __argumentsPool.Add(item);
            }
        }

        public void Clear()
        {
            lock (__argumentsPool)
            {
                __argumentsPool.Clear();
            }
        }

        public void Destroy()
        {
            Clear();
        }
    }
}
