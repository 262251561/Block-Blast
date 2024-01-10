/********************************************************************************
*文 件 名： NetSocketArgument.cs
*描    述： NetSocketArgument的功能说明
*作    者： hejinjiang
*创建时间： 2021.05.17
*修改记录:
*********************************************************************************/

using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Net
{
    public class NetSocketArgument : SocketAsyncEventArgs
    {
        public Action<NetSocketArgument> complete_handler;

        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            base.OnCompleted(e);

            complete_handler?.Invoke(this);
        }
    }
}