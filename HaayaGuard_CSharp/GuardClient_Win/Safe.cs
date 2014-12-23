using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Haaya.GuardClient
{
  internal class Safe
    {
      public static string key=String.Empty;
        internal static void Security(Socket socket)
        {
            socket.Send(System.Text.Encoding.ASCII.GetBytes(key));
            bool isLoop = true;
            int count=0;
            byte[] cmd = new byte[256];
            while (isLoop)
            {
                count++;
            if (socket.Available < 1)
                {
                    if (count > 10) isLoop = false;
                    Thread.Sleep(300);
                    continue;
                }
            socket.Receive(cmd);
            string strCmd = System.Text.Encoding.ASCII.GetString(cmd);
                if(strCmd==CmdTable.Passed)
                    isLoop = false;
            }
        }
    }
}
