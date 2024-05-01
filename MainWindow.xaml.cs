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

namespace Project_Ventilatorsturing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort _serialPort;
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            _serialPort.WriteLine("getData");
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadLine().Trim();
            string[] values = data.Split(',');

            if (values.Length == 2 && values[0] == "temperature")
            {
                double temperature = Convert.ToDouble(values[1]);

                // Update de GUI met de ontvangen temperatuur
                Dispatcher.Invoke(() =>
                {
                    lblTemp.Content = $"Temperature: {temperature}%";

                    // Controleer of de ventilator moet worden ingeschakeld of uitgeschakeld
                    if (temperature > 20 && !isVentilatorOn)
                    {
                        isVentilatorOn = true;
                        lblTemp.Content = "Ventilator: AAN";
                    }
                    else if (temperature < 20 && isVentilatorOn)
                    {
                        isVentilatorOn = false;
                        lblTemp.Content = "Ventilator: UIT";
                    }
                });
            }
        }

        
    }
}
