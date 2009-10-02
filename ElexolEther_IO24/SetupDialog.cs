using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ElexolEther_IO24
{
    public partial class SetupDialog : Form
    {
        private IPAddress m_IPAddress;
        private int _minIntensity = 1;

        public SetupDialog(IPAddress ipAddr, int minIntensity)
        {
            InitializeComponent();

            m_IPAddress = ipAddr;
            tbIPAddress.Text = m_IPAddress.ToString();

            _minIntensity = minIntensity;
            sliderMinIntensity.Value = _minIntensity;
            lblMinIntensity.Text = _minIntensity.ToString();
        }

        public IPAddress IPAddr
        {
            get { return m_IPAddress; }
        }

        public int MinIntensity
        {
            get { return _minIntensity; }
        }




        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (IPAddress.TryParse(tbIPAddress.Text, out m_IPAddress))
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Invalid IP Address.");
            }

        }

        private void sliderMinIntensity_ValueChanged(object sender, EventArgs e)
        {
            _minIntensity = sliderMinIntensity.Value;
            lblMinIntensity.Text = _minIntensity.ToString();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            UdpClient sock = new UdpClient();
            IPEndPoint remoteEP = new IPEndPoint(IPAddr, ElexolEther_IO24.RemotePort);
            sock.Connect(remoteEP);

            // Use "`" command (0x60) to cause device to echo byte back.
            byte[] sendPckt = new byte[] {0x60, 0x69};
            
            DateTime begin = DateTime.Now;
            sock.Send(sendPckt, sendPckt.Length);

            // Wait for data to be available or a timeout to ocurr.
            while( (sock.Available==0) && (((TimeSpan)(DateTime.Now - begin)).TotalSeconds < 1) )
                Thread.Sleep(10);

            if (sock.Available==0)
                MessageBox.Show("No Reply.");
            else
            {
                IPEndPoint rcvdEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] rcvdPckt = sock.Receive(ref rcvdEP);
                if (arrayEqual(rcvdPckt,sendPckt))
                    MessageBox.Show("Connection Successful!");
                else
                    MessageBox.Show("Device replied, but with incorrect data:\n" + printBytes(sendPckt));
            }
        }


        private static string printBytes(byte[] array)
        {
            string retval = "";
            foreach(byte c in array)
                retval += String.Format("[0x{0:X2}]", c);
            return retval;
        }

        private static bool arrayEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }


    }
}