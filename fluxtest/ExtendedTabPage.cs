using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fluxtest
{
    class ExtendedTabPage : TabPage
    {
        public SampleControl UserControl { get; private set; }
        
        public ExtendedTabPage(SampleControl userControl)
        {
            UserControl = userControl;
            this.Controls.Add(userControl);
        }
    }
}
