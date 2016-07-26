using System;
using System.Linq;
using System.IO.Ports;
using Sharpduino.CommTransports;
using System.Management;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sharpduino.CommTransports.Serial
{
    public class SerialPortTransport : System.IO.Ports.SerialPort, ICommTransport
    {

        public bool AutoStart { get; set; } = true;

        public SerialPortTransport()
        {
            var portNames = ListFriendlyCOMPort().Where(pt => pt.ToUpper().Contains("ARDUINO")).FirstOrDefault();
            var port = Regex.Match(portNames, "\\(([A-Z0-9])*\\)").Value;
            PortName = port.Substring(1, port.Length - 2);
            BaudRate = 57600;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            AutoStart = true;
        }

        public SerialPortTransport(string portName) : base(portName, 57600) { }

        public SerialPortTransport(string portName, int baudRate) : base(portName, baudRate) {
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            AutoStart = true;
        }

        int ICommTransport.Parity {
            get {
                return (int)base.Parity;
            }
            set {
                base.Parity = (Parity)value;
            }
        }

        int ICommTransport.StopBits {
            get {
                return (int)base.StopBits;
            }
            set {
                base.StopBits = (StopBits)value;
            }
        }

        public static List<string> ListFriendlyCOMPort() {
            List<string> oList = new List<string>();
            try {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'")) {
                    foreach (var queryObj in searcher.Get()) {
                        oList.Add(queryObj["Caption"].ToString());
                    }
                }
            } catch (ManagementException err) {
                throw new Exception("An error occurred while querying for WMI data: " + err.Message);
            }
            return oList;
        }


    }
}

