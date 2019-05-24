using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
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
using AForge.Video;
using AForge.Video.DirectShow;
using System.Net;
using System.Net.Http;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace Facecheck_mini
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LoaclWebCamsCollection;
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
            LocalWebCam.Start();
        }

        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameHolder.Source = bi;
                }));
            }
            catch (Exception ex)
            {
            }
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string time;
            string fname;
            string res;
            time = DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
            fname = "C:\\Users\\Investigación\\Pictures\\" + time + "-record.png";
            //Agregar source a imagen estática para procesarla
            imgReg.Source = frameHolder.Source;
            //Visualizar imagen estática y esconder imagen de camara
            //imgReg.Visibility = Visibility.Visible;
            //frameHolder.Visibility = Visibility.Hidden;

            SaveToPng(gridmain, fname);
            res = POST_Persons(fname);
            
            if (res == null)
                File.WriteAllText("C:\\Users\\Investigación\\Documents\\Appeon\\PowerBuilder 17.0\\res.txt", "error");
            else
                File.WriteAllText("C:\\Users\\Investigación\\Documents\\Appeon\\PowerBuilder 17.0\\res.txt", res);

            LocalWebCam.Stop();
            LocalWebCam = null;
            Application.Current.Shutdown();
            return;
        }

        void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        string POST_Persons(string fname)
        {
            var client = new RestClient("https://api.identix.one/v1/persons/");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Token 26546c0170c304fa27adc9165de1f3c0e3012335a1b2f5d9fbb52843d7568667d151c515f65a6126c4f56d5e906cfdbf53bbbba95ed724a77d80d2148c013011");
            request.AddHeader("content-type", "multipart/form-data;");
            request.AddFile("photo", fname);
            request.AddParameter("source", "source-test");
            request.AddParameter("facesize", 0);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)

                return response.Content;
            else
                return null;
        }


        void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            System.Windows.Size visualsize = new System.Windows.Size((int)visual.ActualWidth, (int)visual.ActualHeight);
            visual.Measure(visualsize);
            visual.Arrange(new Rect(visualsize));

            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
    }
}
