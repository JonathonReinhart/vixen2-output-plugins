using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Vixen;

namespace EventLogger2
{
    public enum FileFormat { CSV };

    public class EventLogger2 : IEventDrivenOutputPlugIn
    {
        
        private string m_filename;
        private StreamWriter m_stream;
        private SetupData m_setupData;       // The SetupData object where settings are stored
        private XmlNode m_setupNode;         // The XML node at which to store settings
        private const string FILENAME_NODE = "filename";
        private List<Channel> m_channels;


        #region IEventDrivenOutputPlugIn Members

        public void Event(byte[] channelValues)
        {
            string line = "";
            DateTime now = DateTime.Now;
            //12-09-2009 00:41:01.397
            line += String.Format("\"{0:00}-{1:00}-{2} {3:00}:{4:00}:{5:00}.{6:000}\"",
                now.Month, now.Day, now.Year,
                now.Hour, now.Minute, now.Second, now.Millisecond);

            line += "," + BitConverter.ToString(channelValues).Replace('-', ',');
            m_stream.WriteLine(line);
        }

        public void Initialize(IExecutable executableObject, SetupData setupData, XmlNode setupNode)
        {
            // Store the SetupData and root XML node passed by Vixen
            m_setupData = setupData;
            m_setupNode = setupNode;

            // Load this plug-in's settings
            m_filename = m_setupData.GetString(m_setupNode, FILENAME_NODE, String.Empty);
           
            // Get the number of channels
            m_channels = executableObject.OutputChannels;
        }

        #endregion

        #region IHardwarePlugin Members

        public HardwareMap[] HardwareMap
        {
            get { return new HardwareMap[] { }; }
        }

        public void Shutdown()
        {
            m_stream.Close();
            m_stream.Dispose();
        }

        public void Startup()
        {
            if (m_filename == "")
            {
                throw new Exception("No filename selected.");
            }

            string dir = Path.GetDirectoryName(m_filename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // The date/time is 25 bytes (with quotes)
            // Each channel, we write 3 bytes (comma and two hex chars)
            // For 128 channels at 50ms, we'll be writing (25 + 128*3) / 0.050 = 8180 bytes/sec
            // So for a 128k buffer, expect an IO Write every (131072/8180) ~= 16 sec
            m_stream = new StreamWriter(m_filename, false, Encoding.ASCII, 128*1024);

            StringBuilder line = new StringBuilder();
            line.Append("\"Channels:\"");
            foreach (Channel c in m_channels)
            {
                line.Append(",\"");
                line.Append(c.Name);
                line.Append("\"");
            }
            m_stream.WriteLine(line.ToString());
            m_stream.Flush();
        }

        #endregion

        #region IPlugIn Members

        public string Author
        {
            get { return "Jonathon Reinhart"; }
        }

        public string Description
        {
            get { return "Data Logger which outputs to CSV for easy importing into Excel, etc."; }
        }

        public string Name
        {
            get { return "Event Logger 2"; }
        }

        #endregion

        #region ISetup Members

        public void Setup()
        {
            SetupForm sf = new SetupForm();
            sf.FileName = m_filename;

            if (sf.ShowDialog() == DialogResult.OK)
            {
                m_filename = sf.FileName;
                m_setupData.SetString(m_setupNode, FILENAME_NODE, m_filename);
            }
        }

        #endregion
    }
}
