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

namespace Sharpduino.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Firmata.Protocol _protocol;

        public MainWindow()
        {
            InitializeComponent();

            List<string> pins = new List<string>();
            List<string> analogPins = new List<string>();
            for (var x = 0; x <= 127; x++) {
                pins.Add(x.ToString());
                if (x < 16) {
                    analogPins.Add(x.ToString());
                }
            }
            cmbMode.ItemsSource = Enum.GetNames(typeof(Core.PinMode)).ToList();
            cmbPin.ItemsSource = pins;
            cmbAnalogPin.ItemsSource = analogPins;
            cmbDigitalPort.ItemsSource = analogPins;
            cmbPin.SelectedValue = "0";
            cmbMode.SelectedValue = "Input";
            cmbAnalogPin.SelectedValue = "0";
            cmbDigitalPort.SelectedValue = "0";
            
            var serial = new CommTransports.Serial.SerialPortTransport();

            _protocol = new Firmata.Protocol(serial);

            this.Closed += MainWindow_Closed;

            _protocol.ServerVersion += _protocol_ServerVersion;
            _protocol.DataRead += _protocol_DataRead;
            _protocol.DigitalWriteData += _protocol_DigitalWriteData;
            _protocol.AnalogRead += _protocol_AnalogRead;
            
        }

        private void _protocol_AnalogRead(string data)
        {
            lblAnalogInput.Content = data;
        }

        private void _protocol_DigitalWriteData(string data)
        {
            lblDigitalOutput.Content = data;
        }

        private void _protocol_DataRead(int data)
        {
            richTextBox.AppendText(String.Format("{0:X} ",data));
        }

        private void _protocol_ServerVersion(int mayorVersion, int minorVersion, string name)
        {
            lblVersion.Content = ((int)mayorVersion + ((int)minorVersion * .1)).ToString() + ":" + name;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (_protocol.IsOpen)
            {
                _protocol.Close();
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_protocol.IsOpen)
            {
                _protocol.RequestVersionReport();
                btnConnect.Content = "Disconnect";
            }
            else {
                btnConnect.Content = "Connect";
                _protocol.Close();
            }
            
        }

        private void btnWrite_Click(object sender, RoutedEventArgs e)
        {
            _protocol.SetPinMode(13, Sharpduino.Core.PinMode.Output);
            _protocol.DigitalWrite(13, Sharpduino.Core.DigitalValue.High);
            _protocol.ToggleDigitalPortReporting(1, true);
        }

        private void btnRequestFirmware_Click(object sender, RoutedEventArgs e)
        {
            _protocol.SysEx.QueryFirmwareNameAndVersion();
        }

        private void btnRequestCapability_Click(object sender, RoutedEventArgs e)
        {
            _protocol.SysEx.CapabilityQuery();
        }

        private void btnRequestAnalog_Click(object sender, RoutedEventArgs e)
        {
            _protocol.SysEx.AnalogMappingQuery();
        }

        private void btnPinStateQuery_Click(object sender, RoutedEventArgs e)
        {
            _protocol.SysEx.PinStateQuery(13);
        }

        private void btnSetDigitalPinMode_Click(object sender, RoutedEventArgs e)
        {
            _protocol.SetPinMode(
                int.Parse(cmbPin.SelectedValue.ToString()), 
                (Core.PinMode)Enum.Parse(typeof(Core.PinMode), cmbMode.SelectedValue.ToString()));
        }

        private void btnSetDigitalPinValue_Click(object sender, RoutedEventArgs e)
        {
            _protocol.SetDigitalPinValue(
                int.Parse(cmbPin.SelectedValue.ToString()),
                (chkDigitalValue.IsChecked.Value) ? Core.DigitalValue.High : Core.DigitalValue.Low);
        }

        private void btnToogleAnalogReport_Click(object sender, RoutedEventArgs e)
        {
            _protocol.ToggleAnalogInReporting(
                int.Parse(cmbAnalogPin.SelectedValue.ToString()),
                chkAnalogEnabled.IsChecked.Value);
        }

        private void btnToogleDigitalReport_Click(object sender, RoutedEventArgs e)
        {
            _protocol.ToggleDigitalPortReporting(
                int.Parse(cmbDigitalPort.SelectedValue.ToString()),
                chkDigitalValue.IsChecked.Value);
        }
    }
}
