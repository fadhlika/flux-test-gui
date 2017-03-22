using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace fluxtest
{
    public partial class MainWindow : Form
    {
        LogWindow logWindow = new LogWindow();
        SerialPort serial;
        Chart chart = new Chart();
        TextBox fitText = new TextBox();

        List<String> log = new List<string>();
        double timer = 0.0;
        int nosample = -1;

        int currentTab;

        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine(DateTime.Now.ToLongTimeString());
            logWindow.InsertLog(String.Format("{0} {1} : {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), "[Program] Start"));
           
            PortRefresh();

            serial = new SerialPort();
            serial.BaudRate = 115200;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.DataBits = 8;
            serial.Handshake = Handshake.None;
            serial.DtrEnable = true;
            serial.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        private void PortRefresh()
        {
            portBox.Items.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                portBox.Items.Add(port);
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string readData = sp.ReadLine();
            logWindow.InsertLog(String.Format("{0} {1} : {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), "COM] " + readData));
            try
            {
                string[] data = readData.Split(';');
                int no = int.Parse(data[0]);
                double level = Math.Round(2 - (int.Parse(data[0]) * 0.05), 3);
                timer += ((65535 * int.Parse(data[1])) + int.Parse(data[2])) * 0.000064;
                timer = Math.Round(timer, 3);
                (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.insertDataGrid(no, level, timer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataReceivedHandler: {0}", ex.ToString());
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show(this);
        }

        private void dataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.saveData();
        }

        private void dataButton_Click(object sender, EventArgs e)
        {
            (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.saveData();
        }        

        private void graphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.saveGraph();
        }

        private void graphButton_Click(object sender, EventArgs e)
        {
            (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.saveGraph();
        }

       

        private void clearButton_Click(object sender, EventArgs e)
        {
            timer = 0.0;
            (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.clearData();
        }        

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.loadData();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            (tabControl.TabPages[currentTab] as ExtendedTabPage).UserControl.fitting();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void portBox_DropDownClosed(object sender, EventArgs e)
        {
            
        }

        private void portBox_Click(object sender, EventArgs e)
        {
            PortRefresh();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Write("a");
            }
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logWindow.Show();
        }

        private void newSampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer = 0.0;
            nosample++;
            InputForm input = new InputForm();
            input.ShowDialog();
            if(input.DialogResult == DialogResult.OK)
            {
                tabControl.TabPages.Add(new ExtendedTabPage(new SampleControl()));
                tabControl.TabPages[nosample].Text = input.sampleName;
                tabControl.SelectedIndex = nosample;
                currentTab = nosample;
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                {
                    serial.PortName = portBox.SelectedItem.ToString();
                    serial.Open();
                    Console.WriteLine(serial.PortName + " " + serial.BaudRate + " Connected");
                    logWindow.InsertLog(String.Format("{0} {1} : {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), "[COM] " + serial.PortName + " " + serial.BaudRate + " Connected\n"));
                    portBox.Enabled = false;
                    connectButton.Text = "Disconnect";
                }
                else if (serial.IsOpen)
                {
                    serial.Close();
                    portBox.Enabled = true;
                    connectButton.Text = "Connect";
                    Console.WriteLine("Device diconnected");
                }
            }
            catch
            {
                Console.WriteLine("Connection Failed");
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentTab = tabControl.SelectedIndex;  
        }
    }           
}
