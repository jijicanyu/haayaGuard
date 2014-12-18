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
        private bool _isSendVedio;
        private ConcurrentQueue<Bitmap> _imgQueue = new ConcurrentQueue<Bitmap>();
        private ConcurrentQueue<Byte[]> _imgCacheQueue = new ConcurrentQueue<Byte[]>();
        private Thread _sendThread;
        private ServiceImp()
        {
            _isSendVedio = false;
            _imgQueue = new ConcurrentQueue<Bitmap>();
            _imgCacheQueue = new ConcurrentQueue<Byte[]>();
            _sendThread = new Thread(ServiceImp.SendVedio);
            
        }
        public void WriteImage(Bitmap image)
        {
            if(_isSendVedio)
            _imgQueue.Enqueue(image);
        }
        public void Send()
        {

        }
        private static void SendVedio()
        {

        }
    }
}
