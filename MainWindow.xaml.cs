using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO.Ports;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Project_Ventilatorsturing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SerialPort _serialPort;
        private bool isVentilatorOn;
        private const int SERIAL_PORT_BAUDRATE = 9600;
        private const string FAN_ON_COMMAND = "FanON";
        private const string FAN_OFF_COMMAND = "FanOFF";
        private const string GET_DATA_COMMAND = "getData";

        private List<string> availableComPorts = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            availableComPorts.Add("None");
            availableComPorts.AddRange(SerialPort.GetPortNames());

            foreach (string port in availableComPorts)
                cbxComPort.Items.Add(port);

            _serialPort = new SerialPort();
            _serialPort.BaudRate = SERIAL_PORT_BAUDRATE;

            _serialPort.DataReceived += SerialPort_DataReceived;
        }


        private void cbxComPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                if (cbxComPort.SelectedItem.ToString() != "None")
                {
                    _serialPort.PortName = cbxComPort.SelectedItem.ToString();
                    _serialPort.Open();
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                _serialPort.WriteLine(GET_DATA_COMMAND);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending command: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadLine().Trim();
                SensorData gegevens = JsonConvert.DeserializeObject<SensorData>(data);
                UpdateUI(gegevens);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing data: {ex.Message}");
            }
        }
        private bool isVentilatorRecentlyTurnedOn = false;
        private void On_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialPort.WriteLine(FAN_ON_COMMAND);

                Fanlbl.Background = Brushes.Green;
                isVentilatorOn = true;

                isVentilatorRecentlyTurnedOn = true;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5); // Kies hier de gewenste tijd dat u de ventilator aan wilt laten draaien
                timer.Tick += (s, args) =>
                {
                    Fanlbl.Background = Brushes.Red;
                    isVentilatorRecentlyTurnedOn = false;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending command: {ex.Message}");
            }
        }

        private bool isVentilatorRecentlyTurnedOff = false;
        private void Off_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialPort.WriteLine(FAN_OFF_COMMAND);

                Fanlbl.Background = Brushes.Red;
                isVentilatorOn = false;

                isVentilatorRecentlyTurnedOff = true;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5); // // Kies hier de gewenste tijd dat u de ventilator uit wilt laten
                timer.Tick += (s, args) =>
                {
                    Fanlbl.Background = Brushes.Green;
                    isVentilatorRecentlyTurnedOff = false; 
                    timer.Stop(); 
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending command: {ex.Message}");
            }
        }
        private void UpdateUI(SensorData gegevens)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (gegevens != null)
                {
                    lblTemp.Content = $"{gegevens.Temperature}°C";

                    if (isVentilatorRecentlyTurnedOn)
                    {
                        Fanlbl.Background = Brushes.Green;
                    }
                    else if (isVentilatorRecentlyTurnedOff)
                    {
                        Fanlbl.Background = Brushes.Red;
                    }
                    else if (gegevens.Temperature > 25) // Kies hier de gewenste temperatuur
                    {
                        Fanlbl.Background = Brushes.Green;
                    }
                    else
                    {
                        Fanlbl.Background = Brushes.Red;
                    }
                }
            });
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Dispose();
            base.OnClosing(e);
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}
