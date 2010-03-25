using System;
using Vixen;
using System.Xml;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace LPT2DMX
{
    public partial class LPT2DMX : IEventDrivenOutputPlugIn
    {

        // Fields
        private ushort m_portAddress = 0;
        private SetupData m_setupData;
        private XmlNode m_setupNode;

        // Methods
        public void Event(byte[] channelValues)
        {
            if (this.m_portAddress == 0)
            {
                throw new Exception("No port has been specified.");
            }

            // When data is ready to be sent by Vixen, Linefeed bit 1 should be set
            // true (0) by sending 0x1 to the LPT control port. (Linefeed=0, Strobe=1)
            Out(ControlPort, 0x01);

            // Next I want the plugin to send 3 sync bytes so we are sure this is
            // the beginning of a valid data transmission.

            // Send 0xAA (b'10101010') to the LPT data port.
            Out(DataPort, 0xAA);
            Strobe();

            // Send 0x55 (b'01010101') to the LPT data port.
            Out(DataPort, 0x55);
            Strobe();

            // Send 0xAA (b'10101010') to the LPT data port.
            Out(DataPort, 0xAA);
            Strobe();


            //Next send 512 channel values.
            /*
            int n=0;
            foreach (byte channel in channelValues)
            {
                Out(DataPort, channel);
                Strobe();

                if (++n >= 512) // Limit to 512 channels.
                    break;
            }
            */

            Debug.WriteLine("channelValues.Length = " + channelValues.Length.ToString());

            for (int i=0; (i<channelValues.Length && i<512); i++)
            {
                Debug.WriteLine(String.Format("Writing channelValues[{0}] = {1}", i, channelValues[i]));
                Out(DataPort, channelValues[i]);
                Strobe();
            }

            // Last set control port to Idle state 0x3 (Linefeed=1, Strobe=1)
            Out(ControlPort, 0x03);
        }

        private void Strobe()
        {
            // Set Strobe bit true (0) by sending 0x0 to the control port (Linefeed=0, Strobe=0)
            Out(ControlPort, 0x00);

            // Set Strobe bit false (1) by sending 0x1 to the control port (Linefeed=0, Strobe=1)
            Out(ControlPort, 0x01);
        }

        private void startupInternal()
        {
            // Idle state of the LPT control port should be set to 0x3. (Linefeed=1, Strobe=1)
            Out(ControlPort, 0x3);
        }

        
        public void Initialize(IExecutable executableObject, SetupData setupData, XmlNode setupNode)
        {
            this.m_setupData = setupData;
            this.m_setupNode = setupNode;
            this.m_portAddress = (ushort)this.m_setupData.GetInteger(this.m_setupNode, "address", 0x378);
        }

        public void Setup()
        {
            var dialog = new Vixen.Dialogs.ParallelSetupDialog(this.m_portAddress);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.m_portAddress = dialog.PortAddress;
                this.m_setupData.SetInteger(this.m_setupNode, "address", this.m_portAddress);
            }
        }



        public void Shutdown()
        {
        }

        public override string ToString()
        {
            return this.Name;
        }


        // Imported DLL Functions
        [DllImport("inpout32", EntryPoint="Out32")]
        private static extern void Out(ushort port, short data);

        [DllImport("inpout32", EntryPoint="Inp32")]
        private static extern short In(ushort port);



        // Properties
        private ushort DataPort
        {
            get { return m_portAddress; }
        }

        private ushort ControlPort
        {
            get { return (ushort)(this.m_portAddress + 2); }
        }


        public HardwareMap[] HardwareMap
        {
            get
            {
                return new HardwareMap[] { new HardwareMap("Parallel", this.m_portAddress, "X") };
            }
        }

        public string Author
        {
            get { return "Jonathon Reinhart"; }
        }

        public string Description
        {
            get { return "Parallel Port to DMX Plugin"; }
        }

        public string Name
        {
            get { return "LPT2DMX"; }
        }

    }
}
