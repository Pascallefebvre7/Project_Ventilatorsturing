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

        public MainWindow()
        {
            InitializeComponent();

            cbxComPort.Items.Add("None");
            foreach (string s in SerialPort.GetPortNames())
                cbxComPort.Items.Add(s);

            _serialPort = new SerialPort();
            _serialPort.BaudRate = 9600;

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
        private void On_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialPort.WriteLine("FanON");

                FanCanvas.Background = Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending command: {ex.Message}");
            }
        }

        private void Off_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialPort.WriteLine("FanOFF");

                FanCanvas.Background = Brushes.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending command: {ex.Message}");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                _serialPort.WriteLine("getData");
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
                string[] values = data.Split(',');

                if (values.Length == 2 && values[0] == "temperature")
                {
                    double temperature = Convert.ToDouble(values[1]);
                    UpdateUI(temperature);
                }
                else if (e.EventType == SerialData.Chars)
                {
                    string data1 = _serialPort.ReadLine();
                    SensorData gegevens = JsonConvert.DeserializeObject<SensorData>(data1);
                    UpdateUI(gegevens);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing data: {ex.Message}");
            }
        }
        private void UpdateUI(double temperature)
        {
            Dispatcher.BeginInvoke(() =>
            {
                lblTemp.Content = $"Temperature: {temperature}°C";

                if (temperature > 20)
                {
                    Fanlbl.Content = "Ventilator: ON";
                    FanCanvas.Background = Brushes.LightGreen;
                }
                else
                {
                    Fanlbl.Content = "Ventilator: OFF";
                    FanCanvas.Background = Brushes.Orange;
                }
            });
        }
        private void UpdateUI(SensorData gegevens)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (gegevens != null)
                {
                    lblTemp.Content = $"{gegevens.Temperature}°C";
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
