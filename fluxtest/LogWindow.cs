using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fluxtest
{
    public partial class LogWindow : Form
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        public void InsertLog(string log)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new MethodInvoker(delegate
                {
                    logBox.AppendText(log);
                    logBox.AppendText(Environment.NewLine);
                }));
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Text Files  | *.txt";
            try
            {
                save.ShowDialog();
                File.WriteAllText(save.FileName, logBox.Text);
            }
            catch
            {

            }
        }
    }
}
