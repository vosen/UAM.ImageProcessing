using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UAM.PTO
{
    public class ImageViewModel : INotifyPropertyChanged
    {
        private PNM image;
        public PNM Image 
        {
            get
            {
                return image;
            }
            set
            {
                if(value != image)
                {
                    image = value;
                    OnPropertyChanged("Image");
                }
            }
        }

        public void EqualizeHistogram()
        {
            image.ApplyFilter(Filters.HistogramEqualize(image));
            OnPropertyChanged("Image");
        }

        public event PropertyChangedEventHandler  PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            var temp = PropertyChanged;
            if (temp != null)
                temp(this, new PropertyChangedEventArgs(name));

        }
    }
}
