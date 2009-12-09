using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EventLogger2
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();

            fd.AddExtension = true;
            fd.CheckPathExists = true;
            fd.DefaultExt = "csv";
            fd.FileName = "VixenEventLog.csv";
            fd.Filter = "CSV Files (*.csv)|*.csv";
            fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fd.ShowHelp = false;
            fd.Title = "Save log to..";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                tbFileName.Text = fd.FileName;
            }
            
        }





        public FileFormat OutputFormat
        {
            get
            {
                if (rbOutputCSV.Checked) return FileFormat.CSV;
                throw new Exception("No/Unknown Output Format Checked");
            }

            set
            {
                switch (value)
                {
                    case FileFormat.CSV:
                        rbOutputCSV.Checked = true;
                        break;
                    default:
                        throw new Exception("Unknown Output Format Provided");
                }
            }
        }

        public string FileName
        {
            get { return tbFileName.Text; }
            set { tbFileName.Text = value; }
        }
    }
}
