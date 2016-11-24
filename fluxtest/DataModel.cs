using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fluxtest
{
    public class DataModel { }

    public class Data : INotifyPropertyChanged
    {
        private int count;
        private long counter;
        private float time;
        private float flow;

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                RaisePropertyChanged("Count");
            }
        }

        public long Counter
        {
            get { return counter; }
            set
            {
                counter = value;
                RaisePropertyChanged("Counter");
            }
        }

        public float Time
        {
            get { return time; }
            set
            {
                time = value;
                RaisePropertyChanged("Time");
            }
        }

        public float Flow
        {
            get { return flow; }
            set
            {
                flow = value;
                RaisePropertyChanged("Flow");
            }
        }

        private void RaisePropertyChanged(string v)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(v));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
