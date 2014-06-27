using System;
using System.Collections.Generic;
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
using System.Net;
using System.IO;
using System.Windows.Threading;
using Microsoft.Win32;
namespace GC_VideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Rectangle rect;
        Window window;
        DispatcherTimer timer = new DispatcherTimer();
        TimeSpan updateInterval = new TimeSpan(0, 0, 3);
        Uri activatedUri, loopUri;
        Uri currentUri;
        string siteUrl = "http://mixitmedia.ca/proximity/writetest.php";
        WebRequest webRequest;
        bool activated = false;
        string location = "zacklocation";
        bool fullscreen = true;
        public MainWindow()
        {
            InitializeComponent();

        }
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Please enter a location.");
                return;
            }
            if (activatedUri == null || loopUri == null)
            {
                MessageBox.Show("Please browse for both videos before submitting.");
                return;
            }
            location = txtLocation.Text;
            InitializeVideoPlayer();
        }
        void InitializeVideoPlayer()
        {
            //loopUri = new Uri(@"..\..\myEyes.mp4", UriKind.Relative);
            //activatedUri = new Uri(@"..\..\myTree.mp4", UriKind.Relative);

            var area = SystemParameters.WorkArea;//System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
            window = new Window();
            var g = new Grid();
            rect = new Rectangle();
            var b = new DrawingBrush();
            var d = new VideoDrawing();

            this.Closed += (sender, e) => { window.Close(); };
            window.ShowInTaskbar = false;
            window.WindowStyle = WindowStyle.None;
            if (fullscreen)
            {
                window.Topmost = true;
                window.Loaded += (sender, e) => { window.WindowState = WindowState.Maximized; };
            }
            window.Left = area.Left;
            window.Top = area.Top;

            window.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            window.Content = g;
            g.Children.Add(rect);
            rect.Fill = b;
            b.Drawing = d;
            d.Player = VideoPlayer;
            d.Rect = new System.Windows.Rect(0, 0, 640, 480);
            window.Show();

            

            //VideoPlayer.Open(new Uri(@"c:\example.wmv", UriKind.Absolute));
            VideoPlayer.SpeedRatio = 3;
            VideoPlayer.MediaOpened += (sender, e) => Resize();
            VideoPlayer.MediaEnded += (sender, e) => MediaFinished();
            OpenVideo(loopUri);

            window.MouseDoubleClick += window_MouseDoubleClick;
            window.KeyDown += window_KeyDown;

            MakeWebRequest();

            timer.Interval = updateInterval;
            timer.Tick += (sender, e) => Update();
            timer.Start();
        }


        void Update()
        {
            string response = MakeWebRequest();
            if (response != null)
            {
                //if (response == "myEyes" && currentUri != myEyes) OpenVideo(myEyes);
                //else if (response == "myTree" && currentUri != myTree) OpenVideo(myTree);
                if (!activated && response == "activated")
                {
                    OpenVideo(activatedUri);
                    activated = true;
                    Console.WriteLine("it has been activated");
                }
                Console.WriteLine(response);
            }
            else
            {
                Console.WriteLine("nullified");
            }
        }
        string MakeWebRequest()
        {
            webRequest = WebRequest.Create(siteUrl + "?action=read&status=yoyo&location=" + location);
            Stream responseStream = webRequest.GetResponse().GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream);
            string line = streamReader.ReadLine();
            if (line != null)
            {
                //Console.WriteLine(line);
            }
            return line;
        }

        void OpenVideo(Uri uri)
        {
            currentUri = uri;
            VideoPlayer.Open(uri);
            VideoPlayer.Play();
        }
        void MediaFinished()
        {
            if (activated)
            {
                activated = false;
                OpenVideo(loopUri);
                webRequest = WebRequest.Create(siteUrl + "?action=write&status=inactive&location=" + location);
                webRequest.GetResponse();
                Console.WriteLine("it has been deactivated");
            }
            else
            {
                //OpenVideo(currentUri);
                OpenVideo(loopUri);
            }
        }

        void window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //swap the videos
            if (currentUri == loopUri) OpenVideo(activatedUri);
            else OpenVideo(loopUri);
        }

        void Resize()
        {
            //set aspect ratio 
            double ratio = Math.Min(240d / VideoPlayer.NaturalVideoHeight, 320d / VideoPlayer.NaturalVideoWidth);
            VideoDisplay.Height = VideoPlayer.NaturalVideoHeight * ratio;
            VideoDisplay.Width = VideoPlayer.NaturalVideoWidth * ratio;

            ratio = Math.Min(window.ActualHeight / VideoPlayer.NaturalVideoHeight,
                     window.ActualWidth / VideoPlayer.NaturalVideoWidth);
            rect.Height = VideoPlayer.NaturalVideoHeight * ratio;
            rect.Width = VideoPlayer.NaturalVideoWidth * ratio;
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
        void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void btnBrowseLooping_Click(object sender, RoutedEventArgs e)
        {
            loopUri = SetVideoUri(lblLooping);
        }
        private void btnBrowseActivated_Click(object sender, RoutedEventArgs e)
        {
            activatedUri = SetVideoUri(lblActivated);
        }
        Uri SetVideoUri(Label label)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            if ((bool)fileDialog.ShowDialog())
            {
                Uri u = new Uri(fileDialog.FileName, UriKind.Absolute);
                if (u != null)
                {
                    label.Content = fileDialog.SafeFileName;
                }
                return u;
            }
            return null;
        }

        

        
    }
}
