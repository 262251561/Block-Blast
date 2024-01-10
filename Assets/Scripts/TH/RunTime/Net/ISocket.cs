/********************************************************************************
*文 件 名： ISocket.cs
*描    述： ISocket的功能说明
*作    者： hejinjiang
*创建时间： 2021.05.17
*修改记录:
*********************************************************************************/

using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;


namespace Net
{
    //网络事件
    public enum NetEventType
    {
        //连接事件
        CONNECT,

        //数据接收事件
        DATA_RECEIVE,

        //断开连接事件
        DIS_CONNECT
    }

    //错误类型
    public enum ErrorType
    {
        //无错误
        NONE,

        //有错误
        ERROR
    }

    public interface INetSocketHandler
    {
        void OnDataReceived(byte[] bytes, int offset, int length, SocketError socketError);

        void OnDataSend(SocketError socketError);

        void OnConnect(SocketError soceketError);

        void OnAccept(INetSocket socket);
    }

    public interface INetSocket
    {
        Socket GetSocket();

        void SetHandler(INetSocketHandler handler);

        void Connect(string ip, int port);

        void StartListen(string ip, int port, int maxConnections);

        void StartReceive();

        void Send(byte[] msgBytes, int offset, int length);

        void Stop();
    }
}
