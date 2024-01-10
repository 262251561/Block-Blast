using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TH;
using UnityEngine;

namespace TH
{
    public class CoroutineQueuePool : MonoBehaviour
    {
        public static CoroutineQueuePool s_Instance = null;

        public interface ICoroutNode
        {
            IEnumerator Run();
        }

        public int maxCount = 50;

        private Queue<ICoroutNode>[] __runQueuePool;

        private void Awake()
        {
            s_Instance = this;

            __runQueuePool = new Queue<ICoroutNode>[maxCount];
            for(int i=0; i<maxCount; ++i)
                __runQueuePool[i] = new Queue<ICoroutNode>();

            for(int i=0; i<maxCount; ++i)
                StartCoroutine(__RunQueue(i));
        }

        public void PushRun(ICoroutNode runIter)
        {
            int i, length = __runQueuePool.Length, minQueueNode = __runQueuePool[0].Count, minIndex = 0;
            for(i=1; i<length; ++i)
            {
                var queue = __runQueuePool[i];
                if(queue.Count < minQueueNode)
                {
                    minIndex = i;
                    minQueueNode = queue.Count;
                }
            }

            __runQueuePool[minIndex].Enqueue(runIter);
        }

        IEnumerator __RunQueue(int index)
        {
            var queue = __runQueuePool[index];
            while(true)
            {
                if (queue.Count > 0)
                {
                    while (queue.Count > 0)
                    {
                        var headNode = queue.Dequeue();
                        yield return headNode.Run();
                    }
                }
                else
                    yield return null;
            }
        }
    }
}
