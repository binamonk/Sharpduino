using System;
using System.Linq;
using System.IO.Ports;
using Sharpduino.CommTransports;

namespace Sharpduino.CommTransports.Serial
{
	public class SerialPortTransport : System.IO.Ports.SerialPort, ICommTransport
	{		

		public bool AutoStart { get; set; } = true;

		public SerialPortTransport() : base(GetPortNames ().First(port => port.Contains("usbmodem")), 57600) 
		{ 			
			DataBits = 8;
			Parity = Parity.None;
			StopBits = StopBits.One;
			Open();
		}

		public SerialPortTransport(string portName) : base(portName, 57600) { 
			DataBits = 8;
			Parity = Parity.None;
			StopBits = StopBits.One;
			Open();
		}

		public SerialPortTransport(string portName, int baudRate) : base(portName, baudRate){
			DataBits = 8;
			Parity = Parity.None;
			StopBits = StopBits.One;
			Open();
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
	}
}

