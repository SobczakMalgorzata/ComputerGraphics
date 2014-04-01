using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace FunctionFilters
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public Bitmap img;
        int[, ,] Matrix;
        int[, ,] MatrixZero;
        string currentimage;
        Uri currentImage;
        int myExt;

        int[] MIdentity = new int[256];
        int[] MInversion = new int[256];
        int[] MGamma = new int[256];
        int[] MBrightness = new int[256];
        int[] MContrast = new int[256];
        int[] MCustom = new int[256];
        int[] MIdentityPoints = new int[256];
        int[] MInversionPoints = new int[256];
        int[] MBrightnessPoints = new int[256];
        int[] MContrastPoints = new int[256];
        int[] MCustomPoints = new int[256];

        int[,] MBlur = new int[3, 3];
        int dBlur = 9;
        int[,] MGaussianSmoothing = new int[3, 3];
        int dGaussianSmoothing = 8;
        int[,] MSharpenFilter = new int[3, 3];
        int[,] MEdgeDetection = new int[3, 3];
        int[,] MEmboss = new int[3, 3];
        int[,] MCustomFunction = new int[256, 256];
        //MouseButtonEventArgs e1;

        public MainWindow()
        {
            InitializeComponent();
            int a = 50;
            int b = 50;
            for (int i = 0; i < 256; i++)
            {
                for (int h = 0; h < 256; h++)
                {
                    MCustomFunction[i, h]=255;
                }
                MGamma[i] = i;
                MIdentityPoints[i] = -1;
                MInversionPoints[i] = -1;
                MBrightnessPoints[i] = -1;
                MContrastPoints[i] = -1;
                MCustomPoints[i] = -1;
                MInversion[i] = 255 - i;
                MIdentity[i] = i;
                if (i<256-a)
                {
                    MBrightness[i] = i + a;
                }
                else 
                {
                    MBrightness[i] = 255;
                }
                if (i < b)
                {
                    MContrast[i] = 0;
                }
                else if (i < 256 - b)
                {
                    MContrast[i] = (((255 / (255 -(2 * b))) * i) - ((255 / (255 -(2 * b))) * b));
                }
                else 
                {
                    MContrast[i] = 255;
                }
            }
            MIdentityPoints[0] = 0;
            MIdentityPoints[255] = 255;
            MInversionPoints[0] = 255;
            MInversionPoints[255] = 0;
            MBrightnessPoints[0] = a;
            MBrightnessPoints[225-a] = 255;
            MBrightnessPoints[255] = 255;
            MContrastPoints[0] = 0;
            MContrastPoints[b] = 0;
            MContrastPoints[255-b] = 255;
            MContrastPoints[255] = 255;
            MCustomPoints[0] = 0;
            MCustomPoints[255] = 255;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MBlur[i, j] = 1;
                    MGaussianSmoothing[i, j] = 1;
                    MSharpenFilter[i, j] = -1;
                    MEdgeDetection[i, j] = 0;
                    MEmboss[i, j] = 1;
                }
            }
            MGaussianSmoothing[0, 0] = 0;
            MGaussianSmoothing[0, 2] = 0;
            MGaussianSmoothing[2, 0] = 0;
            MGaussianSmoothing[2, 2] = 0;
            MGaussianSmoothing[1, 1] = 4;
            MSharpenFilter[0, 0] = 0;
            MSharpenFilter[0, 2] = 0;
            MSharpenFilter[2, 0] = 0;
            MSharpenFilter[2, 2] = 0;
            MSharpenFilter[1, 1] = 5;
            MEdgeDetection[0, 0] = -1;
            MEdgeDetection[1, 1] = 1;
            MEmboss[0, 0] = -1;
            MEmboss[0, 1] = -1;
            MEmboss[0, 2] = -1;
            MEmboss[1, 0] = 0;
            MEmboss[1, 2] = 0;
        }
        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "Bit Map (*.bmp)|*.bmp|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png|" +
                "Portable Network Graphic (*.gif)|*.gif" ;
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.Stream myStream = null;
                try
                {
                    if ((myStream = op.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            currentimage = op.FileName; 
                            currentImage = new Uri(op.FileName);
                            img = new Bitmap(myStream);
                            this.board.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                            myExt = op.FilterIndex;
                            getRGB();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
                myStream.Dispose();    
            }
        }

        
        public void getRGB()
        {
            Matrix = new int[img.Width, img.Height, 4];
            MatrixZero = new int[img.Width, img.Height, 4];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    System.Drawing.Color pixel = img.GetPixel(i,j);
                    int p = pixel.ToArgb();

                    if (pixel != null)
                    {
                        Matrix[i, j, 0] = pixel.R;
                        MatrixZero[i, j, 0] = pixel.R;
                        Matrix[i, j, 1] = pixel.G;
                        MatrixZero[i, j, 1] = pixel.G;
                        Matrix[i, j, 2] = pixel.B;
                        MatrixZero[i, j, 2] = pixel.B;
                        Matrix[i, j, 3] = pixel.A;
                        MatrixZero[i, j, 3] = pixel.A;
                    }
                }
            }
        }

        public void ComeBack()
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (Matrix[i, j, 3] < 0)
                        Matrix[i, j, 3] = 0;
                    if (Matrix[i, j, 2] < 0)
                        Matrix[i, j, 2] = 0;
                    if (Matrix[i, j, 1] < 0)
                        Matrix[i, j, 1] = 0;
                    if (Matrix[i, j, 0] < 0)
                        Matrix[i, j, 0] = 0;
                    if (Matrix[i, j, 3] > 255)
                        Matrix[i, j, 3] = 255;
                    if (Matrix[i, j, 2] > 255)
                        Matrix[i, j, 2] = 255;
                    if (Matrix[i, j, 1] > 255)
                        Matrix[i, j, 1] = 255;
                    if (Matrix[i, j, 0] > 255)
                        Matrix[i, j, 0] = 255;
                    System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 3], Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    img.SetPixel(i, j, c);
                }
            }
            this.board.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
        }
        
        private void SaveImage(object sender, RoutedEventArgs e)
        {
            if (img != null)
            {
                if (myExt == 1)
                {
                    try
                    {
                        string y = currentImage.LocalPath;
                        bool x = System.IO.File.Exists(y);
                        if (x)
                            System.IO.File.Delete(y);
                        img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Bmp);
                        this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("There was a problem saving the file." +
                            "Check the file permissions.");
                    }

                    //img.Save(myStream, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                if (myExt == 2)
                {
                    string y = currentImage.LocalPath;
                    bool x = System.IO.File.Exists(y);
                    if (x)
                        System.IO.File.Delete(y);
                    img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                if (myExt == 3)
                {
                    string y = currentImage.LocalPath;
                    bool x = System.IO.File.Exists(y);
                    if (x)
                        System.IO.File.Delete(y);
                    img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Png);
                }
                if (myExt == 4)
                {
                    string y = currentImage.LocalPath;
                    bool x = System.IO.File.Exists(y);
                    if (x)
                        System.IO.File.Delete(y);
                    img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Gif);
                }
            }
        }

        private void SaveImageAs(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Image";
            dlg.DefaultExt = ".bmp";
            dlg.Title = "Save As";
            dlg.Filter = "Bitmap Image (.bmp)|*.bmp|" +
                "JPEG (*.jpg)|*.jpg|" +
                "Portable Network Graphic (*.png)|*.png|" +
                "Portable Network Graphic (*.gif)|*.gif";
            if (dlg.ShowDialog() == true)
            {

                if (dlg.FilterIndex == 1)
                {
                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }

                if (dlg.FilterIndex == 2)
                {
                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }
                if (dlg.FilterIndex == 3)
                {

                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }
                if (dlg.FilterIndex == 4)
                {

                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Gif);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }
            }
        }

        public void GraphPainting(int[] f, int p)
        {
            System.Windows.Point a1 = new System.Windows.Point(0, 0);
            System.Windows.Point a2 = new System.Windows.Point(0, 0);
            PointCollection myCollect = new PointCollection();
                a1 = new System.Windows.Point( 0,f[0] );

            for (int j = 0; j < 256; j++)
            {
                if (f[j] > -1 && j > 0)
                {
                    a2 = new System.Windows.Point(j,f[j]);
                    Line myLine = new Line();
                    myLine.Stroke = System.Windows.Media.Brushes.Black;
                    myLine.X1 = a1.X;
                    myLine.X2 = a2.X;
                    myLine.Y1 = a1.Y;
                    myLine.Y2 = a2.Y;
                    //myLine.MouseLeftButtonDown += PointAdd(myLine, e1);
                    myLine.StrokeThickness = 1;
                    InputFunction.Children.Add(myLine);
                    if (a1.Y < a2.Y)
                    {
                        for (int i = Convert.ToInt32(a1.X); i < Convert.ToInt32(a2.X); i++)
                        {
                            double n = (a2.Y - a1.Y) / (a2.X - a1.X);
                            MCustom[i] = Convert.ToInt32(a1.Y) + Convert.ToInt32(n * i);
                        }
                    }
                    if (a1.Y == a2.Y)
                    {
                        for (int i = Convert.ToInt32(a1.X); i < Convert.ToInt32(a2.X); i++)
                        {
                            MCustom[i] = Convert.ToInt32(a1.Y);
                        }
                    }
                    if (a1.Y > a2.Y)
                    {
                        for (int i = Convert.ToInt32(a1.X); i < Convert.ToInt32(a2.X); i++)
                        {
                            double n = (a1.Y - a2.Y) / (a2.X - a1.X);
                            MCustom[i] = Convert.ToInt32(a1.Y) - Convert.ToInt32(n * i);

                        }
                    }
                    a1 = a2;
                }
                //if (f[j] > -1)
                //{
                //    Line myLine = new Line();
                //    myLine.Stroke = System.Windows.Media.Brushes.Red;
                //    myLine.X1 = j;
                //    myLine.X2 = j;
                //    myLine.Y1 = f[j];
                //    myLine.Y2 = f[j];
                //    myLine.StrokeThickness = 2;
                //    InputFunction.Children.Add(myLine);
                //}
            }
        }
        public void FunctionFilter(int[] f)
        {

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (chanelR.IsChecked == true)
                        Matrix[i, j, 0] = f[Matrix[i, j, 0]];
                    if (chanelG.IsChecked == true)
                        Matrix[i, j, 1] = f[Matrix[i, j, 1]];
                    if (chanelB.IsChecked == true)
                        Matrix[i, j, 2] = f[Matrix[i, j, 2]];
                    if (chanelA.IsChecked == true)
                        Matrix[i, j, 3] = f[Matrix[i, j, 3]];
                }
            }
        }
        private void Inversion(object sender, RoutedEventArgs e)
        {
            FunctionFilter(MInversion);
            ComeBack();
        }
        
        private void Brightnes(object sender, RoutedEventArgs e)
        {
            FunctionFilter(MBrightness);
            ComeBack();
        }

        private void Contrast(object sender, RoutedEventArgs e)
        {
            FunctionFilter(MContrast);
            ComeBack();
        }

        
        public void Convolution(int[,] f, int d, int offset)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    int a1 = 0;
                    int a2 = 0;
                    int a3 = 0;
                    int a0 = 0;
                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {

                            if (i == 0 && k == -1 && j == 0 && l == -1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 2]);
                            }
                            else if (i == 0 && k == -1 && j == img.Height - 1 && l == 1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1 && j == 0 && l == -1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1 && j == img.Height - 1 && l == 1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 2]);
                            }
                            else if (i == 0 && k == -1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 2]);
                            }
                            else if (j == 0 && l == -1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 2]);
                            }
                            else if (j == img.Height - 1 && l == 1)
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 2]);
                            }
                            else
                            {
                                if (chanelA.IsChecked == true)
                                    a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 3]);
                                if (chanelR.IsChecked == true)
                                    a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 0]);
                                if (chanelG.IsChecked == true)
                                    a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 1]);
                                if (chanelB.IsChecked == true)
                                    a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 2]);
                            }
                        }
                    }
                    Matrix[i, j, 0] = ((a1 / d) + offset);
                    Matrix[i, j, 1] = ((a2 / d) + offset);
                    Matrix[i, j, 2] = ((a3 / d) + offset);
                    Matrix[i, j, 3] = ((a0 / d) + offset);
                }
            }
        }
        private void fBlur(object sender, RoutedEventArgs e)
        {
            Convolution(MBlur, dBlur, 0);
            ComeBack();
        }

        private void fGaussianSmotth(object sender, RoutedEventArgs e)
        {
            Convolution(MGaussianSmoothing, dGaussianSmoothing, 0);
            ComeBack();
        }

        private void fSharpen(object sender, RoutedEventArgs e)
        {
            Convolution(MSharpenFilter, 1, 0);
            ComeBack();
        }

        private void fedgeDetection(object sender, RoutedEventArgs e)
        {
            Convolution(MEdgeDetection, 1, 100);
            ComeBack();
        }

        private void fEmboss(object sender, RoutedEventArgs e)
        {
            Convolution(MEmboss, 1, 0);
            ComeBack();
        }

        private void PointAdd(object sender, MouseButtonEventArgs e)
        {

        }

        private void PointEnd(object sender, MouseButtonEventArgs e)
        {

        }

        private void CustomFilterApply(object sender, RoutedEventArgs e)
        {
            FunctionFilter(MCustom);
            ComeBack();
        }

        private void IdentityFilterApply(object sender, RoutedEventArgs e)
        {
            InputFunction.Children.Clear();
            GraphPainting(MIdentityPoints, 2);
        }

        private void InversionFilterApply(object sender, RoutedEventArgs e)
        {
            InputFunction.Children.Clear();
            GraphPainting(MInversionPoints, 2);
        }

        private void BrightnessCorrectionApply(object sender, RoutedEventArgs e)
        {
            InputFunction.Children.Clear();
            GraphPainting(MBrightnessPoints, 3);
        }

        private void ContrastEnchancementApply(object sender, RoutedEventArgs e)
        {
            InputFunction.Children.Clear();
            GraphPainting(MContrastPoints, 4);
        }

        private void Gamma(object sender, RoutedEventArgs e)
        {
            double o ;
            if (double.TryParse(GammaCoeficient.Text, out o))
            {
                if (double.Parse(GammaCoeficient.Text) >= 0 && double.Parse(GammaCoeficient.Text) <= 5)
                {
                    GammaApply(double.Parse(GammaCoeficient.Text));
                    FunctionFilter(MGamma);
                    ComeBack();
                }
                else
                    GammaCoeficient.Text = "Input Number from 0 to 5";
            }
            else
                GammaCoeficient.Text = "Input Number from 0 to 5";
        }
        
        private void GammaApply(double g)
        {
            double temp;
            int i = 0;
            for (double h = 0; h < 256; h++)
            {
                temp = (Math.Pow((h/255), g))*255;
                MGamma[i] = (int)temp;
                i++;
            }            
        }

        private void GSLightness(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    int gray = (Matrix[i, j, 0] + Matrix[i, j, 1] + Matrix[i, j, 2]) / 3;
                    Matrix[i, j, 0] = gray;
                    Matrix[i, j, 1] = gray;
                    Matrix[i, j, 2] = gray;
                }
            }
            ComeBack();
        }
        private void GSAverage(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < img.Width; i++)
            {

                for (int j = 0; j < img.Height; j++)
                {
                    int gray = (Math.Max(Matrix[i, j, 0], Math.Max(Matrix[i, j, 1], Matrix[i, j, 2])) + Math.Min(Matrix[i, j, 0], Math.Min(Matrix[i, j, 1], Matrix[i, j, 2]))) / 2;
                    Matrix[i, j, 0] = gray;
                    Matrix[i, j, 1] = gray;
                    Matrix[i, j, 2] = gray;
                }
            }
            ComeBack();
        }

        private void GSLuminosity(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    double gray = (0.299 * Matrix[i, j, 0]) + (0.587 * Matrix[i, j, 1]) + (0.114 * Matrix[i, j, 2]);
                    Matrix[i, j, 0] = (int) gray;
                    Matrix[i, j, 1] = (int) gray;
                    Matrix[i, j, 2] = (int) gray;
                }
            }
            ComeBack();
        }

        

        private void fMyDithering(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(DitheringCoeficient.Text, out o))
            {
                if (int.Parse(DitheringCoeficient.Text) >= 2 && int.Parse(DitheringCoeficient.Text) <= 16)
                {
                    fMyDitheringInput(int.Parse(DitheringCoeficient.Text));
                    ComeBack();
                }
                else
                    DitheringCoeficient.Text = "Input Number from 2 to 16";
            }
            else
                DitheringCoeficient.Text = "Input Number from 2 to 16";            
        }
        private void fMyDitheringInput(int k)
        {
            
            double x = 256 / k;
            double y = 256 / (k-1);
            
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    for (int l = 0; l < k; l++)
                    {
                        if (Matrix[i, j, 0] >= l * x && Matrix[i, j, 0] < (l+1)*x)
                        {
                            Matrix[i, j, 0] = (int)Math.Round(l * y);
                        }
                        if (Matrix[i, j, 1] >= l * x && Matrix[i, j, 1] < (l + 1) * x)
                        {
                            Matrix[i, j, 1] = (int)Math.Round(l * y);
                        }
                        if (Matrix[i, j, 2] >= l * x && Matrix[i, j, 2] < (l + 1) * x)
                        {
                            Matrix[i, j, 2] = (int)Math.Round(l * y);
                        }
                    }
                }
            }

        }

        private void fThreshold(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(ThresholdCoeficient.Text, out o))
            {
                if (int.Parse(ThresholdCoeficient.Text) >= 1 && int.Parse(ThresholdCoeficient.Text) <= 254)
                {
                    fThresholdInput(int.Parse(ThresholdCoeficient.Text));
                    ComeBack();
                }
                else
                    ThresholdCoeficient.Text = "Input Number from 1 to 254";
            }
            else
                ThresholdCoeficient.Text = "Input Number from 1 to 254";
        }
        private void fThresholdInput(int k)
        {
                      

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {

                    if (Matrix[i, j, 0] > k)
                    {
                        Matrix[i, j, 0] = 255;
                    }
                    else
                    {
                        Matrix[i, j, 0] = 0;
                    }
                    if (Matrix[i, j, 1] > k)
                    {
                        Matrix[i, j, 1] = 255;
                    }
                    else
                    {
                        Matrix[i, j, 1] = 0;
                    }
                    if (Matrix[i, j, 2] > k)
                    {
                        Matrix[i, j, 2] = 255;
                    }
                    else
                    {
                        Matrix[i, j, 2] = 0;
                    }
                                        
                }
            }
        }
        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        private void fRandomDitheringInput(int k)
        {
            int y = 256 / (k - 1);
            Random random = new Random();

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    double h = random.NextDouble() * 1.0;

                    if ((Matrix[i, j, 0] % y) <= (h * y))
                    {
                        Matrix[i, j, 0] = ((Matrix[i, j, 0] / y) * y);
                    }
                    else
                    {
                        Matrix[i, j, 0] = (((Matrix[i, j, 0] / y) + 1) * y);
                    }
                    if ((Matrix[i, j, 1] % y) <= (h * y))
                    {
                        Matrix[i, j, 1] = ((Matrix[i, j, 1] / y) * y);
                    }
                    else
                    {
                        Matrix[i, j, 1] = (((Matrix[i, j, 1] / y) + 1) * y);
                    }
                    if ((Matrix[i, j, 2] % y) <= (h * y))
                    {
                        Matrix[i, j, 2] = ((Matrix[i, j, 2] / y) * y);
                    }
                    else
                    {
                        Matrix[i, j, 2] = (((Matrix[i, j, 2] / y) + 1) * y);
                    }
                }
            }
        }

        private void fRandomDithering(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(DitheringCoeficient.Text, out o))
            {
                if (int.Parse(DitheringCoeficient.Text) >= 1 && int.Parse(DitheringCoeficient.Text) <= 254)
                {
                    fRandomDitheringInput(int.Parse(DitheringCoeficient.Text));
                    ComeBack();
                }
                else
                    DitheringCoeficient.Text = "Input Number from 1 to 254";
            }
            else
                DitheringCoeficient.Text = "Input Number from 1 to 254";
        }

        private void kMeans(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(DitheringCoeficient.Text, out o))
            {
                if (int.Parse(DitheringCoeficient.Text) >= 1 && int.Parse(DitheringCoeficient.Text) <= 128)
                {
                    fKMeansInput(int.Parse(KMeansCoeficient.Text));
                    //ComeBack();
                }
                else
                    DitheringCoeficient.Text = "Input Number from 1 to 128";
            }
            else
                DitheringCoeficient.Text = "Input Number from 1 to 128";
        }
        private void fKMeansInput(int k)
        {
            Random random = new Random();
            int difference = 10;
            Vector3D[] centroid;
            centroid = new Vector3D [k];
            int[,] colorSpace;
            List<Vector3D>[] groups = new List<Vector3D>[k];
            colorSpace = new int[256, 3];
            List<Vector3D> cSpace;
            cSpace= new List<Vector3D>();
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Vector3D asd = new Vector3D(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    //if (!cSpace.Contains(asd))
                    //{
                        cSpace.Add(asd);
                    //}
                   
                }
            }
            if (cSpace.Count > k)
            {
                for (int s = 0; s < k; s++)
                {
                    centroid[s] = new Vector3D(random.Next(255), random.Next(255), random.Next(255));
                    groups[s] = new List<Vector3D>();
                }

                while (difference > 0)
                {
                    difference = 0;
                    foreach(Vector3D vi in cSpace)
                    
                    //cSpace.ForEach(delegate(Vector3D vi)
                    {
                        double minDist = Math.Sqrt(Math.Pow(((int)vi.X - (int)centroid[0].X), 2) + Math.Pow(((int)vi.Y - (int)centroid[0].Y), 2) + Math.Pow(((int)vi.Z - (int)centroid[0].Z), 2));
                        int Centroid = 0;
                        for (int f = 1; f<k; f++)
                        {
                            double h = Math.Sqrt(Math.Pow(((int)vi.X - (int)centroid[f].X), 2) + Math.Pow(((int)vi.Y - (int)centroid[f].Y), 2) + Math.Pow(((int)vi.Z - (int)centroid[f].Z), 2));
                            if (minDist > h)
                            {
                                minDist = h;
                                Centroid = f;
                            }
                        }
                        if (!groups[Centroid].Contains(vi))
                        {
                            for(int sdf = 0; sdf < k;sdf++)
                            {
                                if (groups[sdf].Contains(vi) && sdf!=Centroid)
                                {
                                    groups[sdf].Remove(vi);
                                }
                            }
                            difference++;
                            groups[Centroid].Add(vi);
                        }


                    }
                    if (difference>0)
                    {
                        for (int f = 0; f < k; f++)
                        {
                            centroid[f] = FunctionFilters.centroid.Centroid(groups[f]);
                            
                        }
                    }

                }

                for (int i = 0; i < img.Width; i++)
                {
                    for (int j = 0; j < img.Height; j++)
                    {
                        Vector3D asd = new Vector3D(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                        for (int r = 0; r < k; r++)
                        {
                            if (groups[r].Contains(asd))
                            {
                                //Matrix[i, j, 0] = (int)Math.Round(centroid[r].X);
                                //Matrix[i, j, 1] = (int)Math.Round(centroid[r].Y);
                                //Matrix[i, j, 2] = (int)Math.Round(centroid[r].Z);
                                System.Drawing.Color c = System.Drawing.Color.FromArgb((int)Math.Round(centroid[r].X), (int)Math.Round(centroid[r].Y), (int)Math.Round(centroid[r].Z));
                                img.SetPixel(i, j, c);
                            }
                        }
                        //if (Matrix[i, j, 3] < 0)
                        //    Matrix[i, j, 3] = 0;
                        //if (Matrix[i, j, 2] < 0)
                        //    Matrix[i, j, 2] = 0;
                        //if (Matrix[i, j, 1] < 0)
                        //    Matrix[i, j, 1] = 0;
                        //if (Matrix[i, j, 0] < 0)
                        //    Matrix[i, j, 0] = 0;
                        //if (Matrix[i, j, 3] > 255)
                        //    Matrix[i, j, 3] = 255;
                        //if (Matrix[i, j, 2] > 255)
                        //    Matrix[i, j, 2] = 255;
                        //if (Matrix[i, j, 1] > 255)
                        //    Matrix[i, j, 1] = 255;
                        //if (Matrix[i, j, 0] > 255)
                        //    Matrix[i, j, 0] = 255;

                    }
                }
                this.board.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

            }
        }

    }
    public static class centroid
    {
        public static Vector3D Centroid(this List<Vector3D> path)
        {
            Vector3D result = path.Aggregate(new Vector3D(0, 0, 0), (current, point) => current + point);
            result /= path.Count;

            return result;
        }

    }
}
