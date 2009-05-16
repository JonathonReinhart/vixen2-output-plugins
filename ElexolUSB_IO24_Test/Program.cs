using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Vixen;
using ElexolUSB_IO24;

namespace ElexolUSB_IO24_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ElexolUSB_IO24.ElexolUSB_IO24 plugin = new ElexolUSB_IO24.ElexolUSB_IO24();
            
            // Dummy parameters
            SetupData setupData = new SetupData();
            Dummy dummyex = new Dummy();
            plugin.Initialize(dummyex, setupData, setupData.RootNode);

            plugin.Setup();

            Console.WriteLine("Press enter to send A{69}B{0}C{0}...");
            Console.Read();

            plugin.Startup();

            // A69 B0 C0
            // 01000101
            byte[] data1 = {0,255,0,0,0,255,0,255};
            plugin.Event(data1);

        }
    }

    class Dummy : IExecutable
    {
        #region IExecutable Members

        public int AudioDeviceIndex
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public int AudioDeviceVolume
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool CanBePlayed
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public List<Channel> Channels
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string FileName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public int Key
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string Name
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public List<Channel> OutputChannels
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public SetupData PlugInData
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool TreatAsLocal
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public object UserData
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion

        #region IMaskable Members

        public byte[][] Mask
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }
}
