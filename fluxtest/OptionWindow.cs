using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fluxtest
{
    public partial class OptionWindow : Form
    {
        MainWindow mwindow;
        BindingSource pSource = new BindingSource();
        BindingSource bSource = new BindingSource();
        public OptionWindow(MainWindow m)
        {
            mwindow = m;
            InitializeComponent();
            int[] baudRate = { 9600, 19200, 115200, 384000, 250000, 1000000 };
            bSource.DataSource = baudRate;
            baudBox.DataSource = bSource;
            baudBox.SelectedIndex = 5;
        }

        public String port
        {
            get { return portBox.Text.ToString(); }
        }

        private void acceptButton_Click(object sender, EventArgs e)
        {
            mwindow.port = portBox.Text.ToString();
            mwindow.baud = int.Parse(baudBox.Text.ToString());
            mwindow.ConnectDevice();
            this.Close();
        }

        private void OptionWindow_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            pSource.DataSource = ports;
            portBox.DataSource = pSource;
        }
    }
}
