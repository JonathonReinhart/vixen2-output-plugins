using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using Vixen;

namespace ElexolEther_IO24
{
    public class ElexolEther_IO24 : IEventDrivenOutputPlugIn
    {
        #region Private Data

        private UdpClient m_socket;         // The UDPClient object for communication
        private SetupData m_setupData;       // The SetupData object where settings are stored
        private XmlNode m_setupNode;         // The XML node at which to store settings

        private IPAddress m_RemoteIPAddr;
        
        private int m_minIntensity = 1;      // The minimum channel intensity to turn a channel ON

        // Strings used to read from/write to the SetupData object
        private const string MinIntensity = "min";
        private const string IPAddr = "ip_addr";

        public const int LocalPort = 2424;
        public const int RemotePort = 2424;

        #endregion


        // Constructor
        public ElexolEther_IO24()
        {
            m_socket = new UdpClient();
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
            int i = 0;                  // Buffer iterator
            byte[] buf = new byte[6];   // The data buffer. ("A[byte_val]B[byte_val]C[byte_val]" = 6bytes)

            for (char port = 'A'; port <= 'C'; ++port)
            {
                buf[i++] = (byte)port;  // Port specification
                buf[i] = 0;             // Initialize value to zero
                for (int bit = 0; (bit < 8 && chan < channelValues.Length); ++bit, ++chan)
                {
                    // If this channel's value is greater than minIntensity, turn on its bit for this port
                    buf[i] |= (byte)(((channelValues[chan] > m_minIntensity) ? 0x01 : 0x00) << bit);
                }
                i++;   
            }
            m_socket.Send(buf, buf.Length);

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
            m_setupData = setupData;
            m_setupNode = setupNode;

            // Load this plug-in's settings
            IPAddress.TryParse(m_setupData.GetString(m_setupNode, IPAddr, "10.10.10.10"), out m_RemoteIPAddr);
            m_minIntensity = m_setupData.GetInteger(m_setupNode, MinIntensity, 1);

        }

        #endregion


        #region IOutputPlugIn Members

        public HardwareMap[] HardwareMap
        {
            // Provide an array of the hardware resources this plugin is using.
            get
            {
                // Vixen only expects an integer value for the second argument of a HardwareMap.
                return new HardwareMap[] { };
                
                //return new HardwareMap[] { new HardwareMap("Serial", m_RemoteIPAddr) };
            }
        }

        public void Setup()
        {
            // Called when the user has requested to setup the plug-in instance.

            // Show the setup dialog
            SetupDialog setup = new SetupDialog(m_RemoteIPAddr, m_minIntensity);
            
            // Save the port number from the dialog box in this instance data, and the XML node.
            if(setup.ShowDialog() == DialogResult.OK)
            {
                m_RemoteIPAddr = setup.IPAddr;
                m_setupData.SetString(m_setupNode, IPAddr, m_RemoteIPAddr.ToString());

                m_minIntensity = setup.MinIntensity;
                m_setupData.SetInteger(m_setupNode, MinIntensity, m_minIntensity);
            }
            
        }

        public void Shutdown()
        {
            // Called when execution is stopped or the plug-in instance is no longer going to be referenced.

            // Turn off all channels ("A\0")
            m_socket.Send(new byte[] { (byte)'A', 0, (byte)'B', 0, (byte)'C', 0 }, 6);

            // Close the serial port
            m_socket.Close();
        }


        public List<System.Windows.Forms.Form> Startup()
        {
            /* Called when a sequence is executed.
             * Returns a list of forms that the plug-in needs to have injected into Vixen's MDI interface.
             * The previews are examples of plug-ins that return forms for this purpose.
             */

            m_socket.Connect(m_RemoteIPAddr, RemotePort);

            // Set the ports to output mode ("!A\0"), and initialize each of them to zero ("A\0"):
            //"!A\0A\0..."
            m_socket.Send(new byte[] {  (byte)'!', (byte)'A', 0,    // Set port A to output mode    (3 bytes)
                                        (byte)'A', 0,               // Initialize port A to 0       (2 bytes)
                                        (byte)'!', (byte)'B', 0,    // Set port B to output mode    (3 bytes)
                                        (byte)'B', 0,               // Initialize port B to 0       (2 bytes)
                                        (byte)'!', (byte)'C', 0,    // Set port C to output mode    (3 bytes)
                                        (byte)'C', 0 },             // Initialize port C to 0       (2 bytes)
                            15);

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
            get { return "Elexol Ether I/O 24"; }
        }


        public string Name
        {
            // Name of the plug-in.
            get { return "ElexolEther_IO24"; }
        }

        #endregion
    }

}
