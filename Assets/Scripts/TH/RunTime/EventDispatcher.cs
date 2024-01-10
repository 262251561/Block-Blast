using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace TH
{
    public interface IEventHandler<T>
        where T : struct
    {
        void OnRun(in T parameter);
    }

    public class EventDispatcher
    {
        public interface IRunnerNode : IDisposable
        {
            void OnRun();
        }

        private abstract class ArgumentsRunner<T>
                where T : struct
        {
            public List<T> arguments;

            public ArgumentsRunner()
            {
                arguments = new List<T>();
            }
        }

        private class RunnerNode<T> : ArgumentsRunner<T>, IRunnerNode
            where T : struct
        {
            public List<IEventHandler<T>> handlers;

            public RunnerNode()
            {
                handlers = new List<IEventHandler<T>>();
            }

            public void ApplyHandler<U>(U handler) where U : class, IEventHandler<T>
            {
                handlers.Add(handler);
            }

            public void RemoveHandler<U>(U handler) where U : class, IEventHandler<T>
            {
                int i, length = handlers.Count;
                for (i = 0; i < length; ++i)
                {
                    if (handlers[i] == handler)
                    {
                        handlers.RemoveAtSwapBack(i);
                        break;
                    }
                }
            }

            public void OnRun()
            {
                int i, length = handlers.Count;
                for (i = 0; i < length; ++i)
                {
                    var handler = handlers[i];
                    for (int j = 0, len = arguments.Count; j < len; j++)
                    {
                        handler.OnRun(arguments[j]);
                    }
                }
                arguments.Clear();
            }

            public void Dispose()
            {

            }
        }

        private bool __isRunning;
        public bool isRunning { get { return __isRunning; } }

        private Dictionary<Type, IRunnerNode> __typeCache;

        public void OnCreate()
        {
            __isRunning = false;
            __typeCache = new Dictionary<Type, IRunnerNode>();
        }

        public void OnDestroy()
        {

        }

        public void AddEventHandler<T, U>(U handler)
            where T : struct
            where U : class, IEventHandler<T>
        {
            if (__isRunning)
            {
                GLog.LogError("Add Handler error, is not allowed itering add");
                return;
            }

            RunnerNode<T> runnerNode = null;
            if (!__typeCache.TryGetValue(typeof(T), out var innerNode))
            {
                runnerNode = new RunnerNode<T>();
                __typeCache.Add(typeof(T), runnerNode);
            }
            else
                runnerNode = innerNode as RunnerNode<T>;

            runnerNode.ApplyHandler(handler);
        }

        public void Remove<T, U>(U handler)
            where T : struct
            where U : class, IEventHandler<T>
        {
            if (__isRunning)
            {
                GLog.LogError("Remove Handler error, is not allowed itering remove");
                return;
            }

            RunnerNode<T> runnerNode = null;
            if (__typeCache.TryGetValue(typeof(T), out var innerNode))
            {
                runnerNode = innerNode as RunnerNode<T>;
                runnerNode.RemoveHandler(handler);
            }
        }

        public void DispatchArgument<T>(in T arugment)
            where T : struct
        {
            ArgumentsRunner<T> runnerNode = null;
            if (__typeCache.TryGetValue(typeof(T), out var innerNode))
            {
                runnerNode = innerNode as ArgumentsRunner<T>;
                runnerNode.arguments.Add(arugment);
            }
        }

        public void OnUpdate()
        {
            __isRunning = true;
            var iter = __typeCache.GetEnumerator();
            while (iter.MoveNext())
            {
                var currentRunner = iter.Current.Value;
                try
                {
                    currentRunner.OnRun();
                }
                catch (Exception e)
                {
                    GLog.LogException(e);
                }
            }
            __isRunning = false;
        }
    }
}
