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

                protocol.ServerVersion += Protocol_ServerVersion;
                if (serial.IsOpen)
                {
                    System.Console.WriteLine("Connected!");
                } else
                {
                    System.Console.WriteLine("Not Connected!");
                }

                protocol.RequestVersionReport();
                protocol.SysEx.QueryFirmwareNameAndVersion();
                protocol.SysEx.CapabilityQuery();
                //protocol.RequestVersionReport();
                protocol.SetPinMode(13, Sharpduino.Core.PinMode.Output);
                protocol.ToggleDigitalPortReporting(1, true);
                protocol.DigitalWrite (13, Sharpduino.Core.DigitalValue.High);
                protocol.SetPinMode(14, Core.PinMode.Serial);
                protocol.ToggleDigitalPortReporting(1, true);
                protocol.ToggleAnalogInReporting(1, true);
                protocol.ToggleAnalogInReporting(0, true);
                //
                //				System.Threading.Thread.Sleep (3000);
                
                protocol.RequestVersionReport();
                //
                //protocol.SysEx.CapabilityQuery();
                protocol.DigitalWrite(13, Sharpduino.Core.DigitalValue.Low);
                System.Console.WriteLine("Press key to exit.");
                System.Console.ReadKey();
			}
            System.Console.WriteLine("End");
        }

        private static void Protocol_ServerVersion(int mayorVersion, int minorVersion, string name)
        {
            System.Console.WriteLine("{0}.{1} {2}", mayorVersion, minorVersion, name);
        }
    }
}
