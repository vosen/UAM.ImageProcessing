using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UAM.PTO
{
    class ConvolutionViewModel : INotifyPropertyChanged
    {
        private float[] matrix = new float[25];
        private float weight = 1;
        private float shift;

        public event PropertyChangedEventHandler PropertyChanged;
        public float[] Matrix 
        { 
            get 
            { 
                return matrix; 
            }
        }

        public float Weight
        { 
            get { return weight; }
            set { weight = value; }
        }

        public float Shift
        {
            get { return shift; }
            set { shift = value; }
        }

        public ConvolutionViewModel()
        {
            matrix[12] = 1;
        }

        private void OnPropertyChanged(string name)
        {
            var temp = PropertyChanged;
            if (temp != null)
                temp(this, new PropertyChangedEventArgs(name));
        }
    }
}
