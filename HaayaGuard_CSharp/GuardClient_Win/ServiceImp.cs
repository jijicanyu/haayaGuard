using GuardClient_Win;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Haaya.GuardClient
{
    internal class ServiceImp
    {
        private static ServiceImp _instance = new ServiceImp();

        internal static ServiceImp Instance
        {
            get { return _instance; }

        }
        private long _imgCount;
        private bool _isSendVedio;
        private ConcurrentQueue<Bitmap> _imgQueue = new ConcurrentQueue<Bitmap>();
        private Thread _heartThread;
        Socket _heartSocket;
        private MemoryStream _ms;
        private System.Threading.Timer _heartTimer;
        private byte[] _heartData = { 1, 0, 1 };
        private byte[] _cmdData;
        public static Form1 win;
         private ServiceImp()
        {
            _cmdData = new byte[512];
            _imgCount = 0;
            _isSendVedio = false;
            _imgQueue = new ConcurrentQueue<Bitmap>();
            _heartThread = new Thread(ServiceImp.HeartListen);
            _ms = new MemoryStream();
            _heartSocket = new Socket(new AddressFamily(), SocketType.Stream, ProtocolType.Tcp);
         
        }
         public void Init()
         {
             try
             {
                 IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(DefineTable.ServerHost), DefineTable.ServerPort);
                 _heartSocket.Connect(ipe);
             }
             catch (Exception ex)
             {
                 return;
             }
             Safe.Security(_heartSocket);
             _heartThread.IsBackground = true;
             _heartThread.Start();
             _heartTimer = new Timer(ServiceImp.Heart, null, 10 * 1000, 60 * 1000);
         }
        
         private static void Heart(object state)
         {
             if (ServiceImp._instance._heartSocket.Available < 1)
             ServiceImp._instance._heartSocket.Send(ServiceImp._instance._heartData);
             win.Log("心跳");
         }
         private static void HeartListen()
         {
             bool isLoop = true;
             while (isLoop)
             {
                 if (ServiceImp._instance._heartSocket.Available < 1)
                 {
                     win.Log("等待服务器指令");
                     Thread.Sleep(1000);
                     continue;
                 }
                 win.Log("收到服务器指令");
                 //接受指令
                 ServiceImp._instance._heartSocket.Receive(ServiceImp._instance._cmdData);
                 string cmd = System.Text.Encoding.ASCII.GetString(ServiceImp._instance._cmdData);
                 if (!string.IsNullOrEmpty(cmd))
                 {
                     string[] cmdDatas = cmd.Split(',');
                     if (cmdDatas[0] == CmdTable.SendImage)
                     {
                         ServiceImp.SendVedio();                         
                     }
                 }
             }
         }
         private static void SendVedio()
         {
             Bitmap data = null;
             while (ServiceImp._instance._isSendVedio)
             {
                 win.Log("发送图片");
                 ServiceImp._instance._imgQueue.TryDequeue(out data);
                 data.Save(ServiceImp._instance._ms, ImageFormat.Jpeg);
                 ServiceImp._instance._heartSocket.Send(ServiceImp._instance._ms.ToArray());
                 ServiceImp._instance._ms.Close();
                 win.Log("发送图片完毕");
             }
         }
        public void WriteImage(Bitmap image)
        {
            _imgCount++;
            if ((_isSendVedio) && (_imgQueue.Count < 1024) && ((_imgCount % 4) == 0))
            {
                _imgQueue.Enqueue(image);
                if (_imgCount == long.MaxValue)
                    _imgCount = 0;
            }
        }       
        public void Stop()
        {
            _isSendVedio = false;           
        }
        public void Clear()
        {
            _isSendVedio = false;
            _heartThread.Abort();
        }
    }
}
