/********************************************************************************
*文 件 名： NetClient.cs
*描    述： NetClient的功能说明
*作    者： hejinjiang
*创建时间： 2021.05.17
*修改记录:
*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using TH;

namespace Net
{
    public interface IPacket
    {
        int WriteTo(byte[] bytes);

        int ReadHeadSize(byte[] bytes, ref int offset);
        int ReadHead(byte[] bytes, ref int offset, int size);
        void ReadBody(byte[] bytes, ref int offset, int size);

        bool IsInvalid();
    }

    public interface IPacketFactory<T>
        where T : struct, IPacket
    {
        T CreatePacket();
    }

    public class ThreadContext
    {
        public static List<Thread> s_Theads = new List<Thread>();

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;

            int i, length = s_Theads.Count;
            for (i = 0; i < length; ++i)
            {
                var soc = s_Theads[i];
                soc.Abort();
            }
        }

        static ThreadContext()
        {
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        }
    }

    public abstract class NetClient<T, U> : INetSocketHandler
        where T : struct, IPacket
        where U : IPacketFactory<T>
    {
        //消息队列结构
        public struct MsgDataNode
        {
            public NetEventType eventType;
            public T msg;
            public ErrorType error;
        }

        private enum PacketState
        {
            READ_PACKET_HEADER_SIZE,
            READ_PACKET_HEADER,
            READ_PACKET_BODY,
        }

        private INetSocket __netPeer;
        //private List<T> __currentSendQueue;
        private List<MsgDataNode> __msgNodes;

        private Stack<byte[]> __sendBufferPool;

        private int __packetHeaderSize;
        private MemoryStream __receiveStream;

        //private Thread __sendThread;
        private PacketState __currentReadState;
        private int __maxSendSize;
        private int __readOffset;
        private int __currentHeaderSize;
        private int __currentBodySize;

        private T __currentPacket;

        private U __packetFactory;

        //maxSendSize -- 消息包发送最大字节数, 
        //maxReceiveSize -- 消息包接收最大字节数
        //packetHeaderSize 消息头结构大小
        public NetClient(int maxSendSize, int maxReceiveSize, int packetHeaderSize)
        {
            __msgNodes = new List<MsgDataNode>();
            __netPeer = new AsyncSocket(maxSendSize, maxReceiveSize);
            __sendBufferPool = new Stack<byte[]>();
            __maxSendSize = maxSendSize;

            for (int i = 0; i < 100; ++i)
                __sendBufferPool.Push( new byte[__maxSendSize] );

            __packetFactory = __GetFactory();
            __packetHeaderSize = packetHeaderSize;
            __netPeer.SetHandler(this);

            //__currentSendQueue = new List<T>();
        }

        byte[] __AllocateSendBuffer()
        {
            lock(__sendBufferPool)
            {
                if (__sendBufferPool.Count > 0)
                    return __sendBufferPool.Pop();
            }

            return new byte[__maxSendSize];
        }

        void __FreeSendBuffer(byte[] bytes)
        {
            lock (__sendBufferPool)
            {
                __sendBufferPool.Push(bytes);
            }
        }

        public bool IsMsgQueueEmpty()
        {
            lock(__msgNodes)
            {
                return __msgNodes.Count == 0;
            }
        }

        protected abstract U __GetFactory();
        protected abstract void __OnProcessNetEvent(in MsgDataNode node);

        public bool IsConnected()
        {
            return __netPeer.GetSocket() != null && __netPeer.GetSocket().Connected;
        }

        //连接
        public void Connect(string ip, int port)
        {
            __msgNodes.Clear();

            __receiveStream = new MemoryStream();

            __readOffset = 0;
            __currentHeaderSize = 0;

            __netPeer.Connect(ip, port);
            __currentReadState = PacketState.READ_PACKET_HEADER_SIZE;

            //if (__sendThread != null)
            //{
            //    __sendThread.Abort();
            //    ThreadContext.s_Theads.Remove(__sendThread);

            //    __sendThread = null;
            //}

            //__sendThread = new Thread(__OnSend);
            //__sendThread.Start();
            //ThreadContext.s_Theads.Add(__sendThread);
        }

        //void __OnSend()
        //{
        //    while(__netPeer != null && __netPeer.GetSocket() != null)
        //    {
        //        if (!__netPeer.GetSocket().Connected)
        //        {
        //            Thread.Sleep(30);
        //            continue;
        //        }

        //        lock (__currentSendQueue)
        //        {
        //            int i, length = __currentSendQueue.Count, msgLength;

        //            for (i = 0; i < length; ++i)
        //            {
        //                var data = __currentSendQueue[i];
        //                msgLength = data.WriteTo(__sendBuffer);
        //                __netPeer.Send(__sendBuffer, 0, msgLength);
        //            }

        //            __currentSendQueue.Clear();
        //        }

        //        Thread.Sleep(30);
        //    }
        //}

        public void OnAccept(INetSocket socket)
        {

        }

        public void Run()
        {
            lock(__msgNodes)
            {
                int i;

                //此处有可能外部会立即清理该数据
                for(i=0; i< __msgNodes.Count; ++i)
                {
                    __OnProcessNetEvent(__msgNodes[i]);
                }

                __msgNodes.Clear();
            }
        }

        public virtual void Send(T data)
        {
            var sendBuffer = __AllocateSendBuffer();
            int length = data.WriteTo(sendBuffer);
            __netPeer.Send(sendBuffer, 0, length);
            __FreeSendBuffer(sendBuffer);

            //lock(__currentSendQueue)
            //{
            //    __currentSendQueue.Add(data);
            //}
        }

        //关闭连接
        public void Close()
        {
            if (__netPeer != null)
                __netPeer.Stop();

            lock (__msgNodes)
            {
                __msgNodes.Clear();
            }
        }

        void PushMsgNode(MsgDataNode msgNode)
        {
            lock (__msgNodes)
            {
                __msgNodes.Add(msgNode);
            }
        }

        public void OnDataReceived(byte[] bytes, int offset, int length, SocketError error)
        {
            //主动断开的不发送通知
            if (length == 0)
            {
                MsgDataNode msgNode = new MsgDataNode();
                msgNode.error = ErrorType.NONE;
                msgNode.eventType = NetEventType.DIS_CONNECT;
                PushMsgNode(msgNode);

                return;
            }

            if (error == SocketError.Success)
            {
                __receiveStream.Write(bytes, offset, length);

                byte[] buffer = __receiveStream.GetBuffer();
                bool isWait = true;
                while (isWait && __readOffset <= __receiveStream.Position)
                {
                    switch(__currentReadState)
                    {
                        case PacketState.READ_PACKET_HEADER_SIZE:
                            if (__receiveStream.Position - __readOffset >= __packetHeaderSize)
                            {
                                __currentPacket = __packetFactory.CreatePacket();
                                __currentHeaderSize = __currentPacket.ReadHeadSize(__receiveStream.GetBuffer(), ref __readOffset);
                                __currentReadState = PacketState.READ_PACKET_HEADER;
                            }
                            else
                                isWait = false;

                            break;
                        case PacketState.READ_PACKET_HEADER:
                            if (__receiveStream.Position - __readOffset >= __currentHeaderSize)
                            {
                                __currentBodySize = __currentPacket.ReadHead(buffer, ref __readOffset, __currentHeaderSize);
                                __currentReadState = PacketState.READ_PACKET_BODY;
                            }
                            else
                                isWait = false;
                            break;
                        case PacketState.READ_PACKET_BODY:
                            if (__receiveStream.Position - __readOffset >= __currentBodySize)
                            {
                                __currentPacket.ReadBody(buffer, ref __readOffset, __currentBodySize);

                                MsgDataNode msgNode = new MsgDataNode();
                                msgNode.error = ErrorType.NONE;
                                msgNode.eventType = NetEventType.DATA_RECEIVE;
                                msgNode.msg = __currentPacket;
                                PushMsgNode(msgNode);

                                __currentPacket = default;
                                __currentReadState = PacketState.READ_PACKET_HEADER_SIZE;
                            }
                            else
                                isWait = false;
                            break;
                    }


                //    if (__currentHeaderSize > 0)
                //    {
                //        if (__receiveStream.Position - __readOffset >= __currentHeaderSize)
                //        {
                //            __currentPacket.ReadBody(buffer, __readOffset, __currentHeaderSize);

                //            MsgDataNode msgNode = new MsgDataNode();
                //            msgNode.error = ErrorType.NONE;
                //            msgNode.eventType = NetEventType.DATA_RECEIVE;
                //            msgNode.msg = __currentPacket;
                //            PushMsgNode(msgNode);

                //            __currentPacket = default;

                //            __readOffset += __currentHeaderSize;

                //            __currentHeaderSize = 0;
                //        }
                //        else
                //            break;
                //    }
                //    else
                //    {
                //        if (__receiveStream.Position - __readOffset >= __packetHeaderSize)
                //        {
                //            __currentPacket = __packetFactory.CreatePacket();
                //            currentPacketOffset = __readOffset;
                //            __currentPacket.ReadBodySize(__receiveStream.GetBuffer(), ref currentPacketOffset);
                //            __currentHeaderSize = currentPacketOffset - __readOffset;
                //            __readOffset = currentPacketOffset;
                //        }
                //        else
                //            break;
                //    }
                }

                if (__readOffset == __receiveStream.Position)
                {
                    __receiveStream.Seek(0, SeekOrigin.Begin);
                    __readOffset = 0;
                }
            }
        }

        public void OnDataSend(SocketError socketError)
        {
            if (socketError != SocketError.Success)
            {
                Close();
            }
        }

        public void OnConnect(SocketError socketError)
        {
            MsgDataNode msgNode = new MsgDataNode();
            msgNode.error = socketError == SocketError.Success ? ErrorType.NONE : ErrorType.ERROR;
            msgNode.eventType = NetEventType.CONNECT;

            PushMsgNode(msgNode);

            GLog.LogError("socket connect " + socketError);
        }
    }
}
