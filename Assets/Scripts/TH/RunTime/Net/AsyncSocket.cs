/********************************************************************************
*文 件 名： AsyncSocket.cs
*描    述： AsyncSocket的功能说明
*作    者： hejinjiang
*创建时间： 2021.05.17
*修改记录:
*********************************************************************************/

using System;
using System.Collections.Generic;

using System.Net.Sockets;
using System.Net;
using UnityEngine;

namespace Net
{
    public class AsyncSocket : INetSocket
    {
        private Socket __socket;
        private ArgumentPool __sendPool;
        private NetSocketArgument __receiveArgument;
        private NetSocketArgument __connectArgument;
        private INetSocketHandler __handler;

        private int __maxSendSize;
        private int __maxReceiveSize;

        private static List<Socket> s_sockets = new List<Socket>();

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;

            int i, length = s_sockets.Count;
            for (i = 0; i < length; ++i)
            {
                var soc = s_sockets[i];

                if (soc.Connected)
                    soc.Shutdown(SocketShutdown.Both);

                soc.Close(30);
                soc.Dispose();
            }
        }

        static AsyncSocket()
        {
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        }

        public AsyncSocket(int maxSendSize, int maxReceiveSize, Socket soc = null)
        {
            __sendPool = new ArgumentPool(maxSendSize);

            __receiveArgument = new NetSocketArgument();
            __receiveArgument.SetBuffer(new byte[maxReceiveSize], 0, maxReceiveSize);
            __receiveArgument.complete_handler = __OnReceive;

            __maxSendSize = maxSendSize;
            __maxReceiveSize = maxReceiveSize;

            __socket = soc;
        }

        public void SetHandler(INetSocketHandler handler)
        {
            __handler = handler;
        }

        public void Connect(string ip, int port)
        {
            if (__socket != null)
            {
                __socket.Close(30);

                s_sockets.Remove(__socket);
            }

            __socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s_sockets.Add(__socket);

            __connectArgument = new NetSocketArgument();
            __connectArgument.SetBuffer(new byte[__maxReceiveSize], 0, __maxReceiveSize);
            __connectArgument.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            __connectArgument.complete_handler = __OnConnect;
            __socket.ConnectAsync(__connectArgument);
        }

        void __OnConnect(NetSocketArgument s)
        {
            if (__handler != null)
                __handler.OnConnect(s.SocketError);

            if (s.SocketError == SocketError.Success)
            {
                s.complete_handler = __OnReceive;

                bool isWait = __socket.ReceiveAsync(s);
                if (!isWait)
                    __OnReceive(s);
            }
        }

        public void StartListen(string ip, int port, int maxConnections)
        {
            if (__socket != null)
                return;

            __socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            __socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            __socket.Listen(maxConnections);

            NetSocketArgument acceptArgument = new NetSocketArgument();
            acceptArgument.complete_handler = __OnAccept;

            if (!__socket.AcceptAsync(acceptArgument))
            {
                __OnAccept(acceptArgument);
            }
        }

        void __OnAccept(NetSocketArgument s)
        {
            if (s.SocketError != SocketError.Success)
                return;

            AsyncSocket sock = new AsyncSocket(__maxSendSize, __maxReceiveSize, s.AcceptSocket);
            if (__handler != null)
                __handler.OnAccept(sock);

            sock.StartReceive();
            
            s.AcceptSocket = null;

            if (!__socket.AcceptAsync(s))
            {
                __OnAccept(s);
            }
        }

        public Socket GetSocket()
        {
            return __socket;
        }

        void __OnSend(NetSocketArgument s)
        {
            if (__handler != null)
                __handler.OnDataSend(s.SocketError);

            __sendPool.FreeArgument(s);
        }

        void __OnReceive(NetSocketArgument s)
        {
            if (__handler != null && s.ConnectSocket != null && s.ConnectSocket == __socket)
                __handler.OnDataReceived(s.Buffer, s.Offset, s.BytesTransferred, s.SocketError);

            if (s.BytesTransferred == 0 || s.SocketError != SocketError.Success)
                return;

            bool isWait = __socket.ReceiveAsync(s);
            if (!isWait)
                __OnReceive(s);
        }

        public void Send(byte[] msgBytes, int offset, int length)
        {
            NetSocketArgument argument = __sendPool.AllocateArgument();

            Array.Copy(msgBytes, offset, argument.Buffer, 0, length);
            argument.SetBuffer(0, length);
            argument.complete_handler = __OnSend;

            bool isWait = __socket.SendAsync(argument);
            if (!isWait)
                __OnSend(argument);
        }

        public void StartReceive()
        {
            bool isWait = __socket.ReceiveAsync(__receiveArgument);
            if (!isWait)
                __OnReceive(__receiveArgument);
        }

        public void Stop()
        {
            if (__socket != null)
            {
                __socket.Close(10);
                s_sockets.Remove(__socket);
                __socket = null;
            }
        }
    }
}
