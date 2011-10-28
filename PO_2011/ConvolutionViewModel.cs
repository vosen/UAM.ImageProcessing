using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UAM.PTO
{
    class ConvolutionViewModel : INotifyPropertyChanged
    {
        private double[] matrix = new double[25];
        private double weight = 1;
        private double shift;

        public event PropertyChangedEventHandler PropertyChanged;
        public double[] Matrix 
        { 
            get 
            { 
                return matrix; 
            }
        }

        public double Weight
        { 
            get { return weight; }
            set { weight = value; }
        }

        public double Shift
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
