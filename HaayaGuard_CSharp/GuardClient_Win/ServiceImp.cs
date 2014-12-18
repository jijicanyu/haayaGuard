using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        private TcpClient _tcpClient;
        private string _targetIp;
        private int _port;
        private MemoryStream _ms;
         private ServiceImp()
        {
            _imgCount = 0;
            _isSendVedio = false;
            _imgQueue = new ConcurrentQueue<Bitmap>();
            _sendThread = new Thread(ServiceImp.SendVedio);
            _tcpClient = new TcpClient();
            _ms = new MemoryStream();
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
        public void Send(string targetIp, int port)
        {
            _targetIp = targetIp;
            _port = port;
            _isSendVedio = true;
            _sendThread.Start();
        }
        private static void SendVedio()
        {
            ServiceImp._instance._tcpClient.Connect(ServiceImp._instance._targetIp, ServiceImp._instance._port);
            Bitmap data=null;
            while (ServiceImp._instance._isSendVedio)
            {
                ServiceImp._instance._imgQueue.TryDequeue(out data);
                NetworkStream stream = ServiceImp._instance._tcpClient.GetStream();
                data.Save(ServiceImp._instance._ms, ImageFormat.Jpeg);
                var datas=ServiceImp._instance._ms.ToArray();
                ServiceImp._instance._ms.Close();
                stream.Write(datas, 0, datas.Length);
            }
            ServiceImp._instance._tcpClient.Close();
        }
    }
}
