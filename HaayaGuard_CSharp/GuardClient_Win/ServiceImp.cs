using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private ConcurrentQueue<Byte[]> _imgCacheQueue = new ConcurrentQueue<Byte[]>();
        private Thread _sendThread;
        private ServiceImp()
        {
            _imgCount = 0;
            _isSendVedio = false;
            _imgQueue = new ConcurrentQueue<Bitmap>();
            _imgCacheQueue = new ConcurrentQueue<Byte[]>();
            _sendThread = new Thread(ServiceImp.SendVedio);
            
        }
        public void WriteImage(Bitmap image)
        {
            _imgCount++;
            if ((_isSendVedio) && (_imgQueue.Count < 1024) && ((_imgCount%4)==0))
            {
                _imgQueue.Enqueue(image);
                if (_imgCount == long.MaxValue)
                    _imgCount = 0;
            }
        }
        public void Send()
        {
            _isSendVedio = true;
            _sendThread.Start();
        }
        private static void SendVedio()
        {
            while (ServiceImp._instance._isSendVedio)
            {

            }
        }
    }
}
