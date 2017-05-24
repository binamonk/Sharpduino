using System;
using System.Linq;
using System.IO.Ports;
using Sharpduino.CommTransports;
using System.Management;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sharpduino.CommTransports.Serial
{
    public class SerialPortTransport : SerialPort, ICommTransport
    {

        public bool AutoStart { get; set; } = true;

        public SerialPortTransport()
        {
            var ports = GetPortNames();
            PortName = ports.Last();
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
                return (int)Parity;
            }
            set {
                Parity = (Parity)value;
            }
        }

        int ICommTransport.StopBits {
            get {
                return (int)StopBits;
            }
            set {
                StopBits = (StopBits)value;
            }
        }
    }
}

