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

        public bool IsImageOpen { get { return Image != null; } }
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
            Image = image.Apply(Filters.HistogramStretch(image));
        }

        public void Undo()
        {
            Image = undoList.Pop();
        }

        public void ApplyGaussianBlur()
        {
            undoList.Push(image);
            Image = image.ApplyConvolution(new double[]{ 0, 0, 0,
                                                         0, 1, 0,
                                                         0, 0, 0}, 1, 0);
        }

        public void ApplyUniformBlur()
        {
            undoList.Push(image);
            Image = image.ApplyConvolution(new double[]{ 1/9d, 1/9d, 1/9d,
                                                         1/9d, 1/9d, 1/9d,
                                                         1/9d, 1/9d, 1/9d}, 1, 0);
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
