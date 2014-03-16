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

namespace FunctionFilters
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool editmode = false;
        public Bitmap img;
        int[, ,] Matrix;
        int[, ,] MatrixZero;
        int PointNumber;
        string currentimage;
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
                currentimage = op.FileName;
                this.board.Source = new BitmapImage(new Uri(op.FileName));
                this.board2.Source = new BitmapImage(new Uri(op.FileName));
                myExt = op.FilterIndex;
                img = new Bitmap(currentimage);

                
                getRGB();
            }
        }

        private void EditorMode(object sender, RoutedEventArgs e)
        {
            if (editmode == false)
            {
                ImageEditor.Width = new GridLength(1, GridUnitType.Star);
                editmode = true;
            }
            else
            {
                ImageEditor.Width = new GridLength(0);
                editmode = false;
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

        private void grayScaleFull(object sender, RoutedEventArgs e)
        {

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
                    double gray = (0.21 * Matrix[i, j, 0]) + (0.71 * Matrix[i, j, 1]) + (0.07 * Matrix[i, j, 2]);
                    Matrix[i, j, 0] = (int) gray;
                    Matrix[i, j, 1] = (int)gray;
                    Matrix[i, j, 2] = (int)gray;
                }
            }
            ComeBack();

        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            //if (System.IO.File.Exists(currentimage.ToString()))
                //System.IO.File.Delete(currentimage.ToString());

            //bmp1.Save("c:\\t.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            //string saveit = currentimage.ToString();
            
                if (img != null)
                {
                    if (myExt == 1)
                    {
                        bool x = System.IO.File.Exists("currentimage");
                        if (x)
                            System.IO.File.Delete("currentimage");
                        img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    if (myExt == 2)
                        img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Jpeg);
                    if (myExt == 3)
                        img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Png);
                    if (myExt == 4)
                        img.Save(currentimage, System.Drawing.Imaging.ImageFormat.Gif);
                }
            

        }

        private void SaveImageAs(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Image";
            dlg.DefaultExt = ".bmp";
            dlg.Title = "Save As";
            dlg.Filter = "Bitmap Image (.bmp)|*.bmp|"+
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

        private void fAverageDithering(object sender, RoutedEventArgs e)
        {

            double o;
            if (double.TryParse(GammaCoeficient.Text, out o))
            {
                if (int.Parse(GammaCoeficient.Text) >= 2 && int.Parse(GammaCoeficient.Text) <= 16)
                {
                    fAverageDitheringInput(int.Parse(DitheringCoeficient.Text));
                    ComeBack();
                }
                else
                    DitheringCoeficient.Text = "Input Number from 2 to 16";
            }
            else
                DitheringCoeficient.Text = "Input Number from 2 to 16";

            
        }
        private void fAverageDitheringInput(int k)
        {
            
            double x = 256 / k;
            double y = 256 / (k-1);
            
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    for (int l = 0; l < k; l++)
                    {
                        //if (Matrix[i, j, 0] == Matrix[i, j, 1] && Matrix[i, j, 0] == Matrix[i, j, 2])
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

    }
}
