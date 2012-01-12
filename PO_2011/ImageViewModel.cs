using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using UAM.PTO.Filters;

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
                    if(image != null)
                        undoList.Push(image);
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
            Image = image.ApplyPointProcessing(Histogram.Equalize(image));
        }

        public void StretchHistogram()
        {
            Image = image.ApplyPointProcessing(Histogram.Stretch(image));
        }

        public void Undo()
        {
            image = undoList.Pop();
            OnPropertyChanged("Image");
        }

        public void ApplyGaussianBlur()
        {
            ApplyConvolutionMatrix(new float[]{    0, 0.01F, 0.02F, 0.01F,    0,
                                                 0.01F, 0.06F,  0.1F, 0.06F, 0.01F,
                                                 0.02F,  0.1F, 0.16F,  0.1F, 0.02F,
                                                 0.01F, 0.06F,  0.1F, 0.06F, 0.01F,
                                                    0, 0.01F, 0.02F, 0.01F,    0}, 1, 0);
        }

        public void ApplyUniformBlur()
        {
            ApplyConvolutionMatrix(new float[]{ 1/9F, 1/9F, 1/9F,
                                                1/9F, 1/9F, 1/9F,
                                                1/9F, 1/9F, 1/9F}, 1, 0);
        }

        public void ApplyConvolutionMatrix(float[] mask, float weight, float shift)
        {
            Trim(ref mask);
            Image = image.ApplyConvolutionMatrix(mask, weight, shift);
        }

        public void ChangeBrightnessContrast(float brightness, float contrast)
        {
            Image =Image.ApplyPointProcessing(Color.BrightnessContrast(brightness, contrast));
        }

        public void ChangeGamma(float value)
        {
            Image = Image.ApplyPointProcessing(Color.Gamma(value));
        }

        public void ToGrayscale()
        {
            Image = Image.ApplyPointProcessing(Color.ToGrayscale);
        }

        // remove useless zeroes on the edges
        private static void Trim(ref float[] mask)
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
            float[] trimmed = new float[(int)Math.Pow(newSize,2)];
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
            Tuple<PNM, PNMFormat> payload = PNM.LoadFileWithFormat(stream);
            Image = payload.Item1;
            Format = payload.Item2;
            undoList.Clear();
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

        internal void DetectEdgesGradient()
        {
            Image = Image.ApplyGradientEdgesDetection();
        }

        internal void DetectEdgesLaplace()
        {
            ApplyConvolutionMatrix(new float[]{  0, -1,  0,
                                                 -1,  4, -1,
                                                  0, -1,  0}, 1, 0);
        }

        internal void DetectEdgesSobel()
        {
            Image = Image.ApplyPixelFunction(3, EdgeDetection.Sobel);
        }

        internal void DetectEdgesRoberts()
        {
            Image = Image.ApplyPixelFunction(3, EdgeDetection.Roberts);
        }

        internal void DetectEdgesPrewitt()
        {
            Image = Image.ApplyPixelFunction(3, EdgeDetection.Prewitt);
        }

        internal void DetectEdgesLoG()
        {
            ApplyConvolutionMatrix(new float[]{ 0, 1, 1,   2,   2,   2, 1, 1, 0,
                                                1, 2, 4,   5,   5,   5, 4, 2, 1,
                                                1, 4, 5,   3,   0,   3, 5, 4, 1,
                                                2, 5, 3, -12, -24, -12, 3, 5, 2,
                                                2, 5, 0, -24, -40, -24, 0, 5, 2,
                                                2, 5, 3, -12, -24, -12, 3, 5, 2,
                                                1, 4, 5,   3,   0,   3, 5, 4, 1,
                                                1, 2, 4,   5,   5,   5, 4, 2, 1,
                                                0, 1, 1,   2,   2,   2, 1, 1, 0}, 1, 0);
        }

        internal void DetectEdgesDoG()
        {
            ApplyConvolutionMatrix(new float[]{  0,  0, -1, -1, -1,  0,  0,
                                                 0, -2, -3, -3, -3, -2,  0,
                                                -1, -3,  5,  5,  5, -3, -1,
                                                -1, -3,  5, 16,  5, -3, -1,
                                                -1, -3,  5,  5,  5, -3, -1,
                                                 0, -2, -3, -3, -3, -2,  0,
                                                 0,  0, -1, -1, -1,  0,  0}, 1, 0);
        }

        internal void DetectEdgesZero()
        {
            Image = image.ApplyZeroCrossingDetector();
        }

        internal void DenoiseMedian()
        {
            Image = image.ApplyPixelFunction(3, Blur.Median);
        }

        internal void MorphDilation()
        {
            Image = image.ApplyPixelFunction(3, Morphology.Dilation);
        }

        internal void MorphErosion()
        {
            Image = image.ApplyPixelFunction(3, Morphology.Erosion);
        }

        internal void MorphOpening()
        {
            Image = image.ApplyPixelFunction(3, Morphology.Erosion)
                         .ApplyPixelFunction(3, Morphology.Dilation);
        }

        internal void MorphClosing()
        {
            Image = image.ApplyPixelFunction(3, Morphology.Dilation)
                         .ApplyPixelFunction(3, Morphology.Erosion);
        }
        internal void ThresholdPlain(byte threshold)
        {
            Image = image.ApplyPointProcessing((r,g,b) => Thresholding.Plain(r,g,b,threshold));
        }

        internal void ThresholdOtsu()
        {
            Image = image.ApplyPointProcessing(Thresholding.Otsu(image));
        }

        internal void ThresholdTriangle()
        {
            Image = image.ApplyPointProcessing(Thresholding.Triangle(image));
        }

        internal void ThresholdEntropy()
        {
            Image = image.ApplyPointProcessing(Thresholding.Entropy(image));
        }

        internal void ThresholdNiblack()
        {
            Image = image.ApplyPixelFunction(15, (img,idx) => Thresholding.Niblack(img,idx));
        }

        #region artistic
        internal void Oil()
        {
            Image = image.ApplyPixelFunction(7, Filters.Artistic.Oil);
        }

        internal void FishEye()
        {
            Image = image.ApplyPixelFunction(0, Filters.Artistic.GenerateFishEye(image));
        }

        internal void Mirror()
        {
            Image = image.ApplyPixelFunction(0, Filters.Artistic.Mirror);
        }

        internal void Negative()
        {
            Image = image.ApplyPixelFunction(0, Filters.Artistic.Negative);
        }

        internal void Emboss()
        {
            ApplyConvolutionMatrix(new float[]{ -1, 0, 0,
                                                 0, 0, 0,
                                                 0, 0, 1}, 1, 127);
        }

        #endregion

        internal void NormalMapping()
        {
            Image = image.ApplyHeightMapFunction(3, Filters.Mapping.Normal);
        }

        internal void HoughTransform()
        {
            Image = image.ApplyHoughTransform();
        }
    }
}
