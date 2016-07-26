using System;
using System.IO.Ports;
using System.Diagnostics;
using Sharpduino.Components;

namespace Sharpduino.Console
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            //			System.Console.WriteLine ("Start");
            //			using (var board = new Boards.ArduinoUno ()) {	
            //				var led = new Led (13).Strobe (250);
            //				board.AddComponent (led);
            //				System.Console.ReadKey();
            //				led.Stop ();
            //				System.Console.WriteLine ("Stop");
            //				System.Console.ReadKey();
            //				led.Off ();
            //				System.Console.WriteLine ("Exit");
            //			}          

            var serial = new CommTransports.Serial.SerialPortTransport();

            using (var protocol = new Sharpduino.Firmata.Protocol (serial)) {

                if (serial.IsOpen)
                {
                    System.Console.WriteLine("Connected!");
                } else
                {
                    System.Console.WriteLine("Not Connected!");
                }

                //protocol.RequestVersionReport();
                protocol.SetPinMode(13, Sharpduino.Core.PinMode.Output);
                protocol.DigitalWrite (13, Sharpduino.Core.DigitalValue.High);
                protocol.ToggleDigitalPortReporting(0, true);
                //
                //				System.Threading.Thread.Sleep (3000);
                System.Console.ReadKey();
                protocol.RequestVersionReport();
                //
                //protocol.SysEx.CapabilityQuery();
                System.Console.ReadKey();

				protocol.DigitalWrite (13, Sharpduino.Core.DigitalValue.Low);
			}
		}
	}
}
