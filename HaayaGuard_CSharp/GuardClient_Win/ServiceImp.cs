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
        private Thread _sendThread;
        private Thread _heartThread;
        Socket _heartSocket;
        Socket _dataSocket;
        private string _targetIp;
        private int _port;
        private MemoryStream _ms;
        private System.Threading.Timer _heartTimer;
        private byte[] _heartData = { 1, 0, 1 };
        private byte[] _cmdData;
         private ServiceImp()
        {
            _cmdData = new byte[512];
            _imgCount = 0;
            _isSendVedio = false;
            _imgQueue = new ConcurrentQueue<Bitmap>();
            _heartThread = new Thread(ServiceImp.HeartListen);
            _sendThread = new Thread(ServiceImp.SendVedio);            
            _ms = new MemoryStream();
            _heartSocket = new Socket(new AddressFamily(), SocketType.Stream, ProtocolType.Tcp);
            _dataSocket = new Socket(new AddressFamily(), SocketType.Stream, ProtocolType.Tcp);
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
             _sendThread.IsBackground = true;
             _heartThread.IsBackground = true;
             _heartThread.Start();
             _heartTimer = new Timer(ServiceImp.Heart, null, 10 * 1000, 60 * 1000);
         }
        
         private static void Heart(object state)
         {
             if (ServiceImp._instance._heartSocket.Available < 1)
             ServiceImp._instance._heartSocket.Send(ServiceImp._instance._heartData);
         }
         private static void HeartListen()
         {
             bool isLoop = true;
             while (isLoop)
             {
                 if (ServiceImp._instance._heartSocket.Available < 1)
                 {
                     Thread.Sleep(1000);
                     continue;
                 }
                 //接受指令
                 ServiceImp._instance._heartSocket.Receive(ServiceImp._instance._cmdData);
                 string cmd = System.Text.Encoding.ASCII.GetString(ServiceImp._instance._cmdData);
                 if (!string.IsNullOrEmpty(cmd))
                 {
                     string[] cmdDatas = cmd.Split(',');
                     if (cmdDatas[0] == CmdTable.SendImage)
                     {
                         string targetIp = cmdDatas[1];
                         int port = int.Parse(cmdDatas[2]);
                         ServiceImp._instance.Send(targetIp, port);
                         isLoop = false;
                     }
                 }
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
        private void Send(string targetIp, int port)
        {
            _targetIp = targetIp;
            _port = port;
            //连接服务器
            IPEndPoint dataIpe = new IPEndPoint(IPAddress.Parse(DefineTable.ServerHost), port);  
            _dataSocket.Connect(dataIpe);
            bool isConnect = false;
            byte[] cmds = new byte[256];
            string strCmd = String.Empty;
            while (!isConnect)
            {
                if (_dataSocket.Available < 1)
                {
                    Thread.Sleep(300);
                    continue;
                }
                _dataSocket.Receive(cmds);
                strCmd=System.Text.Encoding.ASCII.GetString(cmds);
                if (strCmd ==CmdTable.OtherClientConnected)
                    isConnect = true;
            }
            _dataSocket.Send(System.Text.Encoding.ASCII.GetBytes(CmdTable.OpenHole));
            //打洞
            IPEndPoint otherDataIpe = new IPEndPoint(IPAddress.Parse(targetIp), port);
            try
            {
                _dataSocket.Connect(otherDataIpe);
            }
            catch (Exception ex)
            {
                isConnect = false;
            }
            if (isConnect)
            {
                _isSendVedio = true;
                _sendThread.Start();
            }
        }       
        private static void SendVedio()
        {
            Bitmap data=null;
            while (ServiceImp._instance._isSendVedio)
            {
                ServiceImp._instance._imgQueue.TryDequeue(out data);
                data.Save(ServiceImp._instance._ms, ImageFormat.Jpeg);
                ServiceImp._instance._dataSocket.Send(ServiceImp._instance._ms.ToArray());
                ServiceImp._instance._ms.Close();
            }
            ServiceImp._instance._dataSocket.Close();
        }
        public void Stop()
        {
            _isSendVedio = false;
            _heartThread.Start();
        }
        public void Clear()
        {
            _isSendVedio = false;
            _sendThread.Abort();
            _heartThread.Abort();
        }
    }
}
