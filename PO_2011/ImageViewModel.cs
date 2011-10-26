using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UAM.PTO
{
    public class ImageViewModel : INotifyPropertyChanged
    {
        private Stack<PNM> undoList = new Stack<PNM>();
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

        public bool CanUndo { get { return undoList.Count > 0; } }

        public void EqualizeHistogram()
        {
            undoList.Push(image);
            image = image.Apply(Filters.HistogramEqualize(image));
            OnPropertyChanged("Image");
        }

        public void StretchHistogram()
        {
            undoList.Push(image);
            image = image.Apply(Filters.HistogramStretch(image));
            OnPropertyChanged("Image");
        }

        public void Undo()
        {
            image = undoList.Pop();
            OnPropertyChanged("Image");
        }

        public void ApplyGaussianBlur()
        {
            image.ApplyConvolutionMatrix(new double[]{ 1/9, 1/9, 1/9,
                                                     1/9, 1/9, 1/9,
                                                     1/9, 1/9, 1/9}, 3);
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
