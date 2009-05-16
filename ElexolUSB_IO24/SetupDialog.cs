using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace ElexolUSB_IO24
{
    public partial class SetupDialog : Form
    {
        private int _comPort = 1;
        private int _minIntensity = 1;

        public SetupDialog(int comPort, int minIntensity)
        {
            InitializeComponent();
            SetupComPortList();

            _comPort = comPort;
            comboCOMPort.SelectedItem = "COM" + _comPort;

            _minIntensity = minIntensity;
            sliderMinIntensity.Value = _minIntensity;
            lblMinIntensity.Text = _minIntensity.ToString();
        }

        public int PortNumber
        {
            get { return _comPort; }
        }

        public int MinIntensity
        {
            get { return _minIntensity; }
        }



        private void SetupComPortList()
        {
            comboCOMPort.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                comboCOMPort.Items.Add(port);
            }
            comboCOMPort.Sorted = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _comPort = Convert.ToInt32(comboCOMPort.SelectedItem.ToString().Substring(3));
        }

        private void sliderMinIntensity_ValueChanged(object sender, EventArgs e)
        {
            _minIntensity = sliderMinIntensity.Value;
            lblMinIntensity.Text = _minIntensity.ToString();
        }


    }
}