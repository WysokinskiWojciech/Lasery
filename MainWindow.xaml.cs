using System;
using System.IO;
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
using System.IO.Ports;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;


namespace Lasery
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> lasery = new List<string>();
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        volatile bool state = false;
        volatile int count = 0;
        public MainWindow()
        {
            InitializeComponent();
            POMIAR.Visibility = Visibility.Hidden;
            ZERUJ.Visibility = Visibility.Hidden;
            POMIAR_CIAGLY.Visibility = Visibility.Hidden;
            ON.Visibility = Visibility.Hidden;
            OFF.Visibility = Visibility.Hidden;
            ProgressBar.Visibility = Visibility.Hidden;
            Slider.ValueChanged +=new RoutedPropertyChangedEventHandler<double>(Slider_ValueChanged);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                COM_PORT_SCAN.Content = "WCIŚNIJ START";

            }), DispatcherPriority.Background);
            for (int i = 1; i <= 12; i++)
            {
                Button on = (Button)FindName("ON" + Convert.ToString(i));
                Button off = (Button)FindName("OFF" + Convert.ToString(i));
                Button pomiar = (Button)FindName("POMIAR" + Convert.ToString(i));
                Button zero = (Button)FindName("ZERUJ" + Convert.ToString(i));
                Label element = (Label)FindName("ZERO" + Convert.ToString(i));

                on.Visibility = Visibility.Hidden;
                off.Visibility = Visibility.Hidden;
                pomiar.Visibility = Visibility.Hidden;
                zero.Visibility = Visibility.Hidden;
                element.Visibility = Visibility.Hidden;
            }
        }

        private void START_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += COM_PORT_TEST;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }
        private void COM_PORT_TEST(object sender, DoWorkEventArgs e)
        {
            lasery.Clear();
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
                COM_PORT_SCAN.Content = "SKANOWANIE...";

            }), DispatcherPriority.Background);
            string[] ports = SerialPort.GetPortNames();
            int i = 0;
            foreach (string p in ports)
            {
                SerialPort serialPort = new SerialPort(p, 19200);
                try
                {
                    serialPort.Open();
                    serialPort.Write("D");
                    System.Threading.Thread.Sleep(1500);
                    if(serialPort.ReadExisting() != "") 
                    {
                        lasery.Add(p);
                    }
                    serialPort.Close();
                }
                catch { }
                i++;
                int progressPercentage = Convert.ToInt32(((double)i / ports.Length) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage);
            }

            e.Result = "Znaleziono " + lasery.Count + " portów szeregowych";
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                COM_PORT_SCAN.Content = e.Result;

                for (int i = 0; i < lasery.Count; i++)
                {
                    Button on = (Button)FindName("ON" + Convert.ToString(i + 1));
                    Button off = (Button)FindName("OFF" + Convert.ToString(i + 1));
                    Button pomiar = (Button)FindName("POMIAR" + Convert.ToString(i + 1));
                    Button zero = (Button)FindName("ZERUJ" + Convert.ToString(i + 1));
                    Label element = (Label)FindName("ZERO" + Convert.ToString(i + 1));

                    on.Visibility = Visibility.Visible;
                    off.Visibility = Visibility.Visible;
                    pomiar.Visibility = Visibility.Visible;
                    zero.Visibility = Visibility.Visible;
                    element.Visibility = Visibility.Visible;
                }
                POMIAR.Visibility = Visibility.Visible;
                ZERUJ.Visibility = Visibility.Visible;
                POMIAR_CIAGLY.Visibility = Visibility.Visible;
                ON.Visibility = Visibility.Visible;
                OFF.Visibility = Visibility.Visible;
                System.Threading.Thread.Sleep(500);
                ProgressBar.Visibility = Visibility.Hidden;
            }), DispatcherPriority.Background);

        }

        private void onoff_cont(object sender, DoWorkEventArgs e)
        {
            SerialPort serialPort = new SerialPort();
            List<object> argumentList = e.Argument as List<object>;
            bool state = (bool)argumentList[0];
            int start = (int)argumentList[1];
            int stop = (int)argumentList[2];
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                if (state)
                {
                    COM_PORT_SCAN.Content = "WŁĄCZANIE LASERÓW...";
                    ProgressBar.Value = 0;
                }
                else
                {
                    COM_PORT_SCAN.Content = "WYŁĄCZANIE LASERÓW...";
                    ProgressBar.Value = 0;
                }

            }), DispatcherPriority.Background);
            int iter = 0;
            for (int i = start; i < stop; i++)
            {
                serialPort.PortName = lasery[i];
                serialPort.BaudRate = 19200;
                try
                {
                    serialPort.Open();
                    if (state)
                    {
                        serialPort.Write("O");
                    }
                    else
                    {
                        serialPort.Write("C");
                    }
                    serialPort.Close();
                }
                catch
                {

                }
                iter++;
                int progressPercentage = Convert.ToInt32(((double)iter / (stop - start)) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage);
            }
            if (state)
            {
                e.Result = "LASERY WŁĄCZONE";
            }
            else
            {
                e.Result = "LASERY WYŁĄCZONE";
            }
        }

        private void zeruj_cont(object sender, DoWorkEventArgs e)
        {
            List<object> argumentList = e.Argument as List<object>;
            int start = (int)argumentList[0];
            int stop = (int)argumentList[1];
            Dispatcher.BeginInvoke((Action)(() =>
            {
                COM_PORT_SCAN.Content = "ZEROWANIE...";
                COM_PORT_SCAN.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
                ProgressBar.Visibility = Visibility.Visible;

            }), DispatcherPriority.Normal);
            SerialPort serialPort = new SerialPort();
            Dictionary<int, string> results = new Dictionary<int, string>();
            string wynik;
            string raw;
            decimal num;
            int iter = 0;
            for (int i = start; i < stop; i++)
            {
                serialPort.PortName = lasery[i];
                serialPort.BaudRate = 19200;
                try
                {
                    serialPort.Open();
                    serialPort.Write("D");
                    System.Threading.Thread.Sleep(1500);
                    raw = serialPort.ReadExisting();
                    serialPort.Close();
                    wynik = raw.Substring(3, 6).Replace(".",",")+raw.Substring(11,3);
                    if (Decimal.TryParse(wynik, out num))
                    {
                        results.Add(i, (num*1000.0m).ToString("0.000"));
                    }
                }
                catch
                {
                    results.Add(i, "Err");
                }
                iter++;
                int progressPercentage = Convert.ToInt32(((double)iter / (stop - start)) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage);
            }
            e.Result = results;
        }
        void zeruj_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dictionary<int, string> resultList = e.Result as Dictionary<int, string>;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                COM_PORT_SCAN.Content = "WYZEROWANO";
                foreach (KeyValuePair<int, string> res in resultList)
                {
                    Label element = (Label)FindName("ZERO" + Convert.ToString(res.Key + 1));
                    decimal num;
                    element.Content = res.Value;
                    if (Decimal.TryParse(res.Value, out num))
                    {
                        element.Foreground = Brushes.Black;
                    }
                    else
                    {
                        element.Foreground = Brushes.Red;
                    }
                }
                System.Threading.Thread.Sleep(500);
                ProgressBar.Visibility = Visibility.Hidden;
            }), DispatcherPriority.Background);

        }

        private void pomiar_cont(object sender, DoWorkEventArgs e)
        {
            List<object> argumentList = e.Argument as List<object>;
            int start = (int)argumentList[0];
            int stop = (int)argumentList[1];
            Dispatcher.BeginInvoke((Action)(() =>
            {

                COM_PORT_SCAN.Content = "POMIAR...";
                COM_PORT_SCAN.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
                ProgressBar.Visibility = Visibility.Visible;

            }), DispatcherPriority.Normal);
            SerialPort serialPort = new SerialPort();
            Dictionary<int, string> results = new Dictionary<int, string>();
            string wynik;
            string raw;
            decimal num;
            int iter = 0;
            for (int i = start; i < stop; i++)
            {
                serialPort.PortName = lasery[i];
                serialPort.BaudRate = 19200;
                try
                {
                    serialPort.Open();
                    serialPort.Write("D");
                    System.Threading.Thread.Sleep(1500);
                    raw = serialPort.ReadExisting();
                    serialPort.Close();
                    wynik = raw.Substring(3, 6).Replace(".", ",") + raw.Substring(11, 3);
                    if (Decimal.TryParse(wynik, out num))
                    {
                        results.Add(i, (num * 1000.0m).ToString("0.000"));

                    }
                }
                catch
                {
                    results.Add(i, "Err");
                }
                iter++;
                int progressPercentage = Convert.ToInt32(((double)iter / (stop - start)) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage);
            }
            e.Result = results;
        }

        void pomiar_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dictionary<int, string> resultList = e.Result as Dictionary<int, string>;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                COM_PORT_SCAN.Content = "POMIAR ZAKOŃCZONO";
                foreach (KeyValuePair<int, string> res in resultList)
                {
                    Label element = (Label)FindName("Laser" + Convert.ToString(res.Key + 1));
                    Label zero = (Label)FindName("ZERO" + Convert.ToString(res.Key + 1));
                    decimal num;
                    if (Decimal.TryParse(res.Value, out num))
                    {
                        element.Content = (num - Convert.ToDecimal(zero.Content)).ToString();
                        element.Foreground = Brushes.Black;
                    }
                    else
                    {
                        element.Content = res.Value;
                        element.Foreground = Brushes.Red;
                    }
                }

                if (Path.Content.ToString() != "")
                {
                    string val = DateTime.Now.ToString("yyyy.MM.dd")+";";
                    val += DateTime.Now.ToString("HH:mm:ss") + ";";
                    for (int i = 0; i < 12; i++)
                    {
                        if (resultList.ContainsKey(i))
                        {
                            Label laser = (Label)FindName("Laser" + Convert.ToString(i + 1));
                            val += laser.Content+ ";";
                        }
                        else
                        {
                            val += "0" + ";";
                        }
                    }
                    if (!File.Exists(Path.Content.ToString()))
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Content.ToString(), true))
                        {
                            file.WriteLine("Date;Time;Laser1;Laser2;Laser3;Laser4;Laser5;Laser6;Laser7;Laser8;Laser9;Laser10;Laser11;Laser12");
                            file.WriteLine(val);
                        }
                    }
                    else
                    {
                        using (System.IO.StreamWriter file =new System.IO.StreamWriter(Path.Content.ToString(), true))
                        {
                            file.WriteLine(val);
                        }
                    }
                }
                System.Threading.Thread.Sleep(500);
                ProgressBar.Visibility = Visibility.Hidden;

                if (state)
                {
                    POMIAR_CIAGLY.Content = "STOP";
                    dispatcherTimer.Start();
                }

            }), DispatcherPriority.Background);

        }

        void pomiar_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }

        private void POMIAR1_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(0);
            arguments.Add(1);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void POMIAR2_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(1);
            arguments.Add(2);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void POMIAR3_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(2);
            arguments.Add(3);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void POMIAR4_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(3);
            arguments.Add(4);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void POMIAR5_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(4);
            arguments.Add(5);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void POMIAR6_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(5);
            arguments.Add(6);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void POMIAR7_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(6);
            arguments.Add(7);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void POMIAR8_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(7);
            arguments.Add(8);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void POMIAR9_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(8);
            arguments.Add(9);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void POMIAR10_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(9);
            arguments.Add(10);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void POMIAR11_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(10);
            arguments.Add(11);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void POMIAR12_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(11);
            arguments.Add(12);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ON_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(0);
            arguments.Add(lasery.Count);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ1_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(0);
            arguments.Add(1);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ZERUJ2_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(1);
            arguments.Add(2);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ZERUJ3_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(2);
            arguments.Add(3);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ4_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(3);
            arguments.Add(4);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ZERUJ5_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(4);
            arguments.Add(5);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ6_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(5);
            arguments.Add(6);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ7_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(6);
            arguments.Add(7);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ8_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(7);
            arguments.Add(8);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ9_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(8);
            arguments.Add(9);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ10_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(9);
            arguments.Add(10);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ZERUJ11_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(10);
            arguments.Add(11);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ZERUJ12_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(11);
            arguments.Add(12);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void OFF_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(0);
            arguments.Add(lasery.Count);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void POMIAR_Click(object sender, RoutedEventArgs e)
        {

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += pomiar_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(0);
            arguments.Add(lasery.Count);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void POMIAR_CIAGLY_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                state = !state;
                if (state)
                {
                    count = 0;
                    POMIAR_CIAGLY.Content = "STOP";
                    COM_PORT_SCAN.Visibility = Visibility.Visible;
                    dispatcherTimer.Tick += dispatcherTimer_Tick;
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                    dispatcherTimer.Start();
                }
                else
                {
                    count = 0;
                    POMIAR_CIAGLY.Content = "POMIAR CIĄGŁY";
                    COM_PORT_SCAN.Content = "";
                    COM_PORT_SCAN.Visibility = Visibility.Hidden;
                    dispatcherTimer.Tick -= dispatcherTimer_Tick;
                    dispatcherTimer.Stop();

                }
            }), DispatcherPriority.Background);
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {   
            count++;
            COM_PORT_SCAN.Content = "POZOSTAŁO " + (Slider.Value - count).ToString("0") + " SEKUND...";
            if (count >= Slider.Value) 
            {
                count = 0;
                dispatcherTimer.Stop();
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += pomiar_cont;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += pomiar_RunWorkerCompleted;
                List<object> arguments = new List<object>();
                arguments.Add(0);
                arguments.Add(lasery.Count);
                worker.RunWorkerAsync(argument: arguments);
            }
        }
        private void ZERUJ_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += zeruj_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += zeruj_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(0);
            arguments.Add(lasery.Count);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ON1_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(0);
            arguments.Add(1);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void OFF1_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(0);
            arguments.Add(1);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ON2_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(1);
            arguments.Add(2);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void OFF2_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(1);
            arguments.Add(2);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ON3_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(2);
            arguments.Add(3);
            worker.RunWorkerAsync(argument: arguments);


        }

        private void OFF3_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(2);
            arguments.Add(3);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ON4_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(3);
            arguments.Add(4);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void OFF4_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(3);
            arguments.Add(4);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ON5_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(4);
            arguments.Add(5);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void OFF5_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(4);
            arguments.Add(5);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ON6_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(5);
            arguments.Add(6);
            worker.RunWorkerAsync(argument: arguments);


        }

        private void OFF6_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(5);
            arguments.Add(6);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ON7_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(6);
            arguments.Add(7);
            worker.RunWorkerAsync(argument: arguments);


        }

        private void OFF7_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(6);
            arguments.Add(7);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ON8_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(7);
            arguments.Add(8);
            worker.RunWorkerAsync(argument: arguments);


        }

        private void OFF8_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(7);
            arguments.Add(8);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ON9_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(8);
            arguments.Add(9);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void OFF9_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(8);
            arguments.Add(9);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void ON10_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(9);
            arguments.Add(10);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void OFF10_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(9);
            arguments.Add(10);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ON11_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(10);
            arguments.Add(11);
            worker.RunWorkerAsync(argument: arguments);

        }

        private void OFF11_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(10);
            arguments.Add(11);
            worker.RunWorkerAsync(argument: arguments);
        }

        private void ON12_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(true);
            arguments.Add(11);
            arguments.Add(12);
            worker.RunWorkerAsync(argument: arguments);


        }

        private void OFF12_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += onoff_cont;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            List<object> arguments = new List<object>();
            arguments.Add(false);
            arguments.Add(11);
            arguments.Add(12);
            worker.RunWorkerAsync(argument: arguments);

        }

    

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case (Key.F1):
                    this.START.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case (Key.F2):
                    this.ON.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case (Key.F3):
                    this.OFF.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case (Key.F4):
                    this.POMIAR.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case (Key.F5):
                    this.POMIAR_CIAGLY.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case (Key.F6):
                    this.ZERUJ.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
            }

        }

        private void Path1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                Path.Content = filename;
            }

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string a = Slider.Value.ToString("0");
            Czas.Content ="Interwał pomiaru ciągłego: "+ a +" sekund";
        }
    }
}
