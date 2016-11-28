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
        OptionWindow optionWindow;

        SerialPort serial;

        DataTable table = new DataTable();
        BindingSource tSource = new BindingSource();

        CultureInfo culture = new CultureInfo("en-us");
        public MainWindow()
        {
            InitializeComponent();
            optionWindow = new OptionWindow(this);

            measurementStatus.Text = "Waiting For Device";

            table.Columns.Add("Level", typeof(int));
            table.Columns.Add("Time", typeof(double));
            table.Columns.Add("DP", typeof(double));
            tSource.DataSource = table;
            dataGridView.DataSource = tSource;

            Title timer = new Title();
            timer.Text = "Flux";
            timer.Font = new Font("Arial", 12f, FontStyle.Bold);
            chart.Titles.Add(timer);
            chart.ChartAreas.Add("ChartArea1");
            chart.ChartAreas["ChartArea1"].AxisX.Title = "Level";
            chart.ChartAreas["ChartArea1"].AxisY.Title = "Time (s)";
            chart.Series.Add("Series1");
            chart.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart.Series["Series1"].XValueMember = "Level";
            chart.Series["Series1"].YValueMembers = "Time";
            chart.Series["Series1"].BorderWidth = 3;
            chart.DataSource = table;
            chart.DataBind();
            
            serial = new SerialPort();
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.DataBits = 8;
            serial.Handshake = Handshake.None;
            serial.DtrEnable = true;
            serial.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string readData = sp.ReadLine();
            Console.WriteLine(readData);
            try
            {
                string[] data = readData.Split('\t');
                if (dataGridView.InvokeRequired)
                {
                    dataGridView.BeginInvoke(new MethodInvoker(delegate
                    {
                        try
                        {
                            table.Rows.Add(int.Parse(data[0]), (((65535 * int.Parse(data[1])) + int.Parse(data[2])) * 0.000064), Double.Parse(data[3])/6000);
                            chart.DataBind();
                            if (int.Parse(data[0]) < 38) measurementStatus.Text = "Measuring";
                            else if (int.Parse(data[0]) == 38) measurementStatus.Text = "Done";
                        }
                        catch
                        {

                        }
                    }));
                }
            }
            catch
            {

            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show(this);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            optionWindow.ShowDialog();
        }

        public void ConnectDevice()
        {
            try
            {
                if (!serial.IsOpen)
                {
                    serial.Open();
                    portStatus.Text = serial.PortName;
                    measurementStatus.Text = "Ready";
                    Console.WriteLine(serial.PortName + " " + serial.BaudRate + " Connected");
                }
                else if (serial.IsOpen)
                {
                    Console.WriteLine("Device Connected");
                }
            }
            catch
            {
                Console.WriteLine("Connection Failed");
            }
        }

        public String port
        {
            set {
                serial.PortName = value;
                Console.WriteLine("PORT : {0}",value);
            }
        }

        public int baud
        {
            set
            {
                serial.BaudRate = value;
                Console.WriteLine("BAUD : {0}", value);
            }
        }

        private void dataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveData();
        }

        private void dataButton_Click(object sender, EventArgs e)
        {
            saveData();
        }

        private void saveData()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Text Files | *.txt";
            try
            {
                save.ShowDialog();
                List<string> export = new List<string>();
                foreach (DataRow data in table.Rows)
                {
                    export.Add(data["Level"].ToString() + "  " + data["Counter"].ToString());
                }
                File.WriteAllLines(save.FileName, export);
            }
            catch
            {

            }
        }

        private void graphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveGraph();
        }

        private void graphButton_Click(object sender, EventArgs e)
        {
            saveGraph();
        }

        private void saveGraph()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Portable Network Graphics | *.png";
            try
            {
                save.ShowDialog();
                chart.SaveImage(save.FileName, System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
            }
            catch
            {

            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            table.Clear();
            chart.DataBind();
            if (serial.IsOpen)
            {
                serial.DiscardInBuffer();
                serial.DiscardOutBuffer();
                serial.Close();
            }
            if (serial.PortName != "") optionWindow.ShowDialog();
            else ConnectDevice();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (serial.IsOpen) MessageBox.Show("Device already connected");
            else optionWindow.ShowDialog();
        }

        private void dataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            chart.DataBind();
        }
    }
}

