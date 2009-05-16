using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.IO.Ports;
using Vixen;

namespace ElexolUSB_IO24
{
    public class ElexolUSB_IO24 : IEventDrivenOutputPlugIn
    {
        #region Private Data

        private SerialPort _comPort;        // The SerialPort object for communication
        private SetupData _setupData;       // The SetupData object where settings are stored
        private XmlNode _setupNode;         // The XML node at which to store settings
        
        private int _comPortNum = 1;        // The number of the COM port used by this plugin
        private int _minIntensity = 1;      // The minimum channel intensity to turn a channel ON

        #endregion


        // Constructor
        public ElexolUSB_IO24()
        {
            _comPort = new SerialPort();
        }

        
        public override string ToString()
        {
            /*
             * Vixen expects that a plug-in will provide a ToString override returning the Name member.
             * The return value of ToString will be what's displayed in any listboxes in which the plug-in is listed.
             */
            return Name;
        }

        //public enum PortType { }
        /*
         * Enumeration of the hardware ports specifically displayed in and tracked by the plug-in setup dialog.
         */


        #region IEventDrivenOutputPlugIn Members

        public void Event(byte[] channelValues)
        {
            /*
             * Called when the plug-in needs to update the hardware state during execution.
             * channelValues: Event values in channel order, 1 byte per channel. 
             */

            int chan=0;                 // Current channel being processed.
            byte[] buf = new byte[2];   // The serial data buffer.

            for (char port = 'A'; port <= 'C'; ++port)
            {
                buf[0] = (byte)port;
                buf[1] = 0;
                for (int bit = 0; (bit < 8 && chan < channelValues.Length); ++bit, ++chan)
                {
                    // If this channel's value is greater than 50%, turn on its bit for this port
                    buf[1] |= (byte)(((channelValues[chan] > _minIntensity) ? 0x01 : 0x00) << bit);
                }
                _comPort.Write(buf, 0, 2);
            }

        }

        public void Initialize(IExecutable executableObject, SetupData setupData, XmlNode setupNode)
        {
            
            /*
             * Called anytime Vixen needs to make sure the plug-in's setup or other initialization is up to date.
             * Initialize is called before the plug-in is setup, before sequence execution, and other times.
             * It's called from multiple places at any time, therefore the plug-in can make no assumptions
             * about the state of the program or sequence due to a call to Initialize.
             * 
             * channels: An array of Channel objects representing the channels present in the owning sequence.
             * setupData: A SetupData reference that provides some plug-incentric convenience methods.
             *            It can be safely ignored.
             * setupNode: An XmlNode representing the root of the plug-in's setup data in the sequence.
             *            Please see the Vixen.Xml class for some convenience methods.
             */

            // Store the SetupData and root XML node passed by Vixen
            _setupData = setupData;
            _setupNode = setupNode;

            // Load this plug-in's settings
            _comPortNum = _setupData.GetInteger(_setupNode, "port", 1);
            _minIntensity = _setupData.GetInteger(_setupNode, "min", 1);

        }

        #endregion


        #region IOutputPlugIn Members

        public HardwareMap[] HardwareMap
        {
            // Provide an array of the hardware resources this plugin is using.
            get
            {
                return new HardwareMap[] { new HardwareMap("Serial", _comPortNum) };
            }
        }

        public void Setup()
        {
            // Called when the user has requested to setup the plug-in instance.

            // Show the setup dialog
            SetupDialog setup = new SetupDialog(_comPortNum, _minIntensity);
            
            // Save the port number from the dialog box in this instance data, and the XML node.
            if(setup.ShowDialog() == DialogResult.OK)
            {
                _comPortNum = setup.PortNumber;
                _setupData.SetInteger(_setupNode, "port", _comPortNum);

                _minIntensity = setup.MinIntensity;
                _setupData.SetInteger(_setupNode, "min", _minIntensity);
            }
            
        }

        public void Shutdown()
        {
            // Called when execution is stopped or the plug-in instance is no longer going to be referenced.

            // Turn off all channels
            _comPort.Write("A" + (char)0);
            _comPort.Write("B" + (char)0);
            _comPort.Write("C" + (char)0);

            // Close the serial port
            _comPort.Close();
        }

        public List<System.Windows.Forms.Form> Startup()
        {
            /* Called when a sequence is executed.
             * Returns a list of forms that the plug-in needs to have injected into Vixen's MDI interface.
             * The previews are examples of plug-ins that return forms for this purpose.
             */

            // Setup COM Port
            _comPort.PortName = "COM" + _comPortNum;
            _comPort.BaudRate = 115200;
            _comPort.Handshake = Handshake.None;
            _comPort.Parity = Parity.None;
            _comPort.StopBits = StopBits.One;
            
            // Open COM Port
            _comPort.Open();

            // Set the ports to output mode (!A)
            _comPort.Write( "!A" + (char)0 );
            _comPort.Write( "!B" + (char)0 );
            _comPort.Write( "!C" + (char)0 );

            // Return an empty list. (No windows are to be shown during execution.)
            return new List<Form>();
        }

        #endregion

        #region IPlugIn Members

        public string Author
        {
            // Author's name.
            get { return "Jonathon Reinhart"; }
        }

        public string Description
        {
            // Plug-in description, such as "PIC-based 8-channel dimming controller"
            get { return "Elexol USB I/O 24"; }
        }


        public string Name
        {
            // Name of the plug-in.
            get { return "ElexolUSB_IO24"; }
        }

        #endregion
    }

}
