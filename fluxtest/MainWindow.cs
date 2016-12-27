﻿using System;
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
        SerialPort serial;

        DataTable table = new DataTable();
        BindingSource tSource = new BindingSource();
        Series fit = new Series("Fit");
        double timer = 0.0;

        public MainWindow()
        {
            InitializeComponent();

            fitText.Text = "";
            table.Columns.Add("Level", typeof(double));
            table.Columns.Add("Time", typeof(double));
            table.Columns.Add("DP", typeof(double));
            tSource.DataSource = table;
            dataGridView.DataSource = tSource;

            PortRefresh();

            chart.ChartAreas.Add("ChartAreaFlow");
            chart.ChartAreas.Add("ChartAreaDP");
            chart.ChartAreas["ChartAreaFlow"].AxisX.Minimum = 0;
            chart.ChartAreas["ChartAreaFlow"].AxisY.Minimum = 0;
            chart.ChartAreas["ChartAreaFlow"].AxisY.Maximum = 2;
            chart.ChartAreas["ChartAreaFlow"].AxisX.Title = "Time (s)";
            chart.ChartAreas["ChartAreaFlow"].AxisY.Title = "Level (m)";
            chart.ChartAreas["ChartAreaDP"].AxisX.Minimum = 0;
            chart.ChartAreas["ChartAreaDP"].AxisX.Maximum = 2;
            chart.ChartAreas["ChartAreaDP"].AxisX.Title = "Level (m)";
            chart.ChartAreas["ChartAreaDP"].AxisY.Title = "DP (Pa)";            
            chart.Titles.Add("Flow");
            chart.Titles[0].DockedToChartArea = "ChartAreaFlow";
            chart.Titles[0].DockingOffset = -5;
            chart.Titles.Add("Differesial Pressure");
            chart.Titles[1].DockedToChartArea = "ChartAreaDP";
            chart.Titles[1].DockingOffset = -5;
            chart.Legends.Add("Legend");
            chart.Legends[0].Docking = Docking.Right;
            chart.Legends[0].LegendStyle = LegendStyle.Column;
            chart.Series.Add("Flow");
            chart.Series.Add("DP");
            chart.Series["Flow"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Flow"].ChartArea = "ChartAreaFlow";
            chart.Series["Flow"].Legend = "Legend";
            chart.Series["Flow"].XValueMember = "Time";
            chart.Series["Flow"].YValueMembers = "Level";
            chart.Series["Flow"].BorderWidth = 3;
            chart.Series["DP"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["DP"].ChartArea = "ChartAreaDP";
            chart.Series["DP"].Legend = "Legend";
            chart.Series["DP"].XValueMember = "Level";
            chart.Series["DP"].YValueMembers = "DP";
            chart.Series["DP"].BorderWidth = 3;
            chart.DataSource = table;
            chart.DataBind();

            serial = new SerialPort();
            serial.BaudRate = 1000000;
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
            Console.WriteLine(readData);
            try
            {
                string[] data = readData.Split(';');
                double level = Math.Round(2 - (int.Parse(data[0]) * 0.05), 3);
                timer += ((65535 * int.Parse(data[1])) + int.Parse(data[2])) * 0.000064;
                timer = Math.Round(timer, 3);
                double dp = Math.Round((double.Parse(data[3]) / 60), 3);
                if (dataGridView.InvokeRequired)
                {
                    dataGridView.Invoke(new MethodInvoker(delegate
                    {
                        table.Rows.Add(level, timer, dp);
                        chart.DataBind();
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
            save.Filter = "Comma Sepperated Value  | *.csv";
            try
            {
                save.ShowDialog();
                List<string> export = new List<string>();
                foreach (DataRow data in table.Rows)
                {
                    export.Add(data["Level"].ToString() + "," + data["Time"].ToString() + "," + data["DP"].ToString());
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
            fit.Points.Clear();
        }

        private void dataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            chart.DataBind();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Do you want to clear all the current data?");
                table.Rows.Clear();
            }
            openDialog.ShowDialog();
            string[] loadedData;
            try
            {
                loadedData = File.ReadAllLines(openDialog.FileName);
                double check;
                foreach (var line in loadedData)
                {
                    string[] data = line.Split(',');
                    if (double.TryParse(data[0], out check))
                    {
                        double h = double.Parse(data[0]);
                        double timer = double.Parse(data[1]);
                        double dp = double.Parse(data[2]);
                        table.Rows.Add(h, timer, dp);
                        chart.DataBind();
                        Console.WriteLine("{0} {1} {2}", h, timer, dp);
                    }
                }
            }
            catch
            {

            }
        }

        private void curveFitting()
        {
            Console.WriteLine("Fitting curve");
            int n = table.Rows.Count;
            double[] x = new double[n];
            double[] y = new double[n];
            int i = 0;
            foreach(DataRow row in table.Rows)
            {
                x[i] = double.Parse(row["Time"].ToString());
                y[i] = double.Parse(row["Level"].ToString());
                i++;
            }

            #region polynomial regression
            //Get matrix A
            double[,] A = new double[3,3];
            A[0, 0] = n;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    double temp = 0.0;
                    if (r == 0 && c == 0) continue;
                    else if ((r == 0 && c == 1) || (r == 1 && c == 0))
                    {
                        for (int l = 0; l < n; l++) temp += x[l];
                    }
                    else if ((r == 0 && c == 2) || (r == 1 && c == 1) || (r == 2 && c == 0))
                    {
                        for (int l = 0; l < n; l++) temp += Math.Pow(x[l], 2);
                    }
                    else if ((r == 1 && c == 2) || (r == 2 && c == 1))
                    {
                        for (int l = 0; l < n; l++) temp += Math.Pow(x[l], 3);
                    }
                    else if ((r == 2 && c == 2))
                    {
                        for (int l = 0; l < n; l++) temp += Math.Pow(x[l], 4);
                    }
                    A[r, c] = temp;
                }
            }
            
            //Get matrix M
            double[,] M = new double[3, 1] { { 0.0 }, { 0.0 }, { 0.0 } };
            for (int l = 0; l < n; l++)
            {
                M[0, 0] += y[l];
                M[1, 0] += (y[l] * x[l]);
                M[2, 0] += (y[l] * Math.Pow(x[l], 2));
            }

            //Forward elimination 1st step
            double t;
            for (int f = 1; f < 3; f++)
            {
                t = A[f, 0] / A[0, 0];
                for (int g = 0; g < 3; g++)
                {
                    A[f, g] -= (A[0, g] * t);
                }
                M[f, 0] -= (M[0, 0] * t);
            }
            //Forward elimination 2nd step
            for (int f = 2; f < 3; f++)
            {
                t = A[f, 1] / A[f-1, 1];
                for (int g = 0; g < 3; g++)
                {
                    A[f, g] -= (A[f-1, g] * t);
                }
                M[f, 0] -= M[f - 1, 0] * t;
            }

            //Back substitution
            double[] p = new double[3];
            p[2] = M[2, 0] / A[2, 2];
            p[1] = (M[1,0]-(A[1, 2] * p[2]))/A[1,1];
            p[0] = (M[0, 0] - (A[0, 1] * p[1]) - (A[0, 2] * p[2])) / A[0, 0];
            #endregion

            //Rounding coeeficient
            for (int c = 0; c < 3; c++)
            {
                p[c] = Math.Round(p[c], 3);
            }

            fitText.Clear();
            fitText.AppendText("Coefficient for y = a + bx + cx^2");
            fitText.AppendText(Environment.NewLine);
            fitText.AppendText("a\tb\tc");
            fitText.AppendText(Environment.NewLine);
            fitText.AppendText(p[0].ToString() + "\t" + p[1].ToString() + "\t" + p[2].ToString());
            fitText.AppendText(Environment.NewLine);

            chart.Series["Fit"].Points.Clear();
            foreach (var s in x)
            {
                chart.Series["Fit"].Points.AddXY(s, p[0] + p[1] * s + p[2] * Math.Pow(s, 2));
            }

            #region obtain coefficient of determination
            //Finding R square
            double y_ = 0.0;
            foreach(var z in y)
            {
                y_ += z;
            }
            y_ /= n;

            double sst = 0.0;
            foreach (var z in y)
            {
                sst += Math.Pow(z-y_,2);
            }

            double ssr = 0.0;
            for(int j=0; j< n;j++)
            { 
                ssr += Math.Pow(y[j] - (p[0] + p[1]*x[j] + p[2]* x[j] * x[j]), 2);
            }

            double rsquare = 1 - (ssr / sst);
            #endregion

            rsquare = Math.Round(rsquare, 3);
            fitText.AppendText("R-sqr : ");
            fitText.AppendText(rsquare.ToString());
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (!chart.Series.Contains(fit))
            {
                chart.Series.Add(fit);
                chart.Series["Fit"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                chart.Series["Fit"].Legend = "Legend";
                chart.Series["Fit"].BorderWidth = 3;
            }
            curveFitting();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void portBox_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                {
                    serial.PortName = portBox.SelectedItem.ToString();
                    serial.Open();
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

        private void portBox_Click(object sender, EventArgs e)
        {
            PortRefresh();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Write("a");
            }
        }
    }           
}
