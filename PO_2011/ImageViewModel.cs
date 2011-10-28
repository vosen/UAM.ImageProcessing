using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

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

        private string path = String.Empty;
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (value != path)
                {
                    path = value;
                    OnPropertyChanged("Path");
                }
            }
        }

        private PNMFormat format;
        public PNMFormat Format
        {
            get
            {
                return format;
            }
            set
            {
                if (value != format)
                {
                    format = value;
                    OnPropertyChanged("Format");
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
            ApplyConvolutionMatrix(new double[]{ 0, 0, 0,
                                                 0, 1, 0,
                                                 0, 0, 0}, 1, 0);
        }

        public void ApplyUniformBlur()
        {
            ApplyConvolutionMatrix(new double[]{ 1/9d, 1/9d, 1/9d,
                                                 1/9d, 1/9d, 1/9d,
                                                 1/9d, 1/9d, 1/9d}, 1, 0);
        }

        public void ApplyConvolutionMatrix(double[] mask, double weight, double shift)
        {
            Trim(ref mask);
            undoList.Push(image);
            Image = image.ApplyConvolution(mask, weight, shift);
        }

        // remove useless zeroes on the edges
        private static void Trim(ref double[] mask)
        {
            if (mask.Length == 1)
                return;
            int length = (int)Math.Sqrt(mask.Length) - 1;
            int i = 0;
            // check upper cells
            for(; i < length; i++)
            {
                if (mask[i] != 0)
                    return;
            }
            // check side cells
            for (int j = 0; j < length; j++, i+= length)
            {
                if (mask[i] != 0 || mask[++i] != 0)
                    return;
            }
            i -= (length - 1);
            // check bottom cells
            for (int j = 0; j < length; j++, i++)
            {
                if (mask[i] != 0)
                    return;
            }
            // do the actual trimming
            int newSize = length - 1;
            double[] trimmed = new double[(int)Math.Pow(newSize,2)];
            for (int m = 1; m < length; m++)
            {
                Array.Copy(mask, (m * (length + 1)) + 1, trimmed, (m - 1) * newSize, newSize);
                //Buffer.BlockCopy(mask, (m * (length +1)) + 1, trimmed, (m - 1) * newSize, newSize);
            }
            mask = trimmed;
            Trim(ref mask);
        }

        public event PropertyChangedEventHandler  PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            var temp = PropertyChanged;
            if (temp != null)
                temp(this, new PropertyChangedEventArgs(name));

        }

        internal void ReplaceImage(string path)
        {
            Stream stream = File.Open(path, FileMode.Open);
            undoList.Clear();
            Tuple<PNM, PNMFormat> payload = PNM.LoadFileWithFormat(stream);
            Image = payload.Item1;
            Format = payload.Item2;
            Path = path;
        }

        internal void SaveImage(string path, PNMFormat format)
        {
            PNM.SaveFile(Image, path, format);
            Path = path;
            Format = format;
        }

        internal void SaveImage()
        {
            PNM.SaveFile(Image, Path, Format);
            Path = path;
        }
    }
}
