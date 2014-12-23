using AForge.Video.DirectShow;
using Haaya.GuardClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GuardClient_Win
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        public Form1()
        {
            InitializeComponent();
            ServiceImp.Instance.Init();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 枚举所有视频输入设备
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo device in videoDevices)
                {
                    tscbxCameras.Items.Add(device.Name);
                }

                tscbxCameras.SelectedIndex = 0;
            }
            catch (ApplicationException)
            {
                tscbxCameras.Items.Add("No local capture devices");
                videoDevices = null;
            }
        }
        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            ServiceImp.Instance.WriteImage(image);
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Safe.key = secretKey.Text;
            CameraConn();
        }

        private void CameraConn()
        {
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[tscbxCameras.SelectedIndex].MonikerString);
            videPlayer.VideoSource = videoSource;
            videPlayer.Start();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            videPlayer.SignalToStop();
            videPlayer.WaitForStop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            toolStripButton2_Click(null, null);
        }
    }
}
