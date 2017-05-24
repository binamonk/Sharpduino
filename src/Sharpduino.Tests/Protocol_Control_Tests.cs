using NUnit.Framework;
using System;

namespace Sharpduino.Tests.Firmata
{
	[TestFixture ()]
	public class Protocol_Control_Tests
	{

		#region Control Messages Expansion
		[Test]
		public void SetPinMode_AllCombinations_Success (){
			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {
				Assert.IsTrue (protocol.IsOpen);
				for (int pin = 0; pin <= 127; pin++) {
					for (int mode = 0 ; mode <= 2 ; mode++) {
						if (mode == 5) {
							continue;
						}
						var state = (Sharpduino.Core.DigitalValue)mode;
						protocol.SetDigitalPinValue (pin, state);
						var data = port.GetWritedData ();
						Assert.AreEqual (string.Format ("F5 {0:X} {1:X} ", pin, mode), data);
					}
				}
			}
		}

		[Test]
		public void SetDigitalPinValue_AllCombinations_Success (){
			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {
				Assert.IsTrue (protocol.IsOpen);
				for (int pin = 0; pin <= 127; pin++) {
					for (int mode = 0 ; mode <= 9 ; mode++) {
						if (mode == 5) {
							continue;
						}
						var state = (Sharpduino.Core.PinMode)mode;
						protocol.SetPinMode (pin, state);
						var data = port.GetWritedData ();
						Assert.AreEqual (string.Format ("F4 {0:X} {1:X} ", pin, mode), data);
					}
				}
			}
		}

		[Test]
		public void ToggleAnalogInReporting_AllCombinations_Success (){
			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {
				Assert.IsTrue (protocol.IsOpen);
				for (int pin = 0; pin < 16; pin++) {
					
					protocol.ToggleAnalogInReporting (pin, true);
					var data = port.GetWritedData ();
					Assert.AreEqual (string.Format ("{0:X} {1:X} ", 0xC0 + pin, 1), data);

					protocol.ToggleAnalogInReporting (pin, false);
					data = port.GetWritedData ();
					Assert.AreEqual (string.Format ("{0:X} {1:X} ", 0xC0 + pin, 0), data);
				}
			}
		}

		[Test]
		public void ToggleDigitalPortReporting_AllCombinations_Success (){
			var serial = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (serial)) {
				Assert.IsTrue (protocol.IsOpen);
				for (int port = 0; port < 16; port++) {

					protocol.ToggleDigitalPortReporting (port, true);
					var data = serial.GetWritedData ();
					Assert.AreEqual (string.Format ("{0:X} {1:X} ", 0xD0 + port, 1), data);

					protocol.ToggleDigitalPortReporting (port, false);
					data = serial.GetWritedData ();
					Assert.AreEqual (string.Format ("{0:X} {1:X} ", 0xD0 + port, 0), data);
				}
			}
		}

		[Test]
		public void RequestVersionReport_Write_Success (){
			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {
				Assert.IsTrue (protocol.IsOpen);
				protocol.RequestVersionReport ();
				var data = port.GetWritedData ();
				Assert.AreEqual ("F9 ", data);
			}
		}


		#endregion

	}
}

