using System;
using NUnit.Framework;

namespace Sharpduino.Tests.Firmata
{
	[TestFixture,Category("Protocol_Write")]
	public class SysEx_Tests
	{
		[Test]
		public void QueryFirmwareNameAndVersion_Valid_Success(){			

			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {

				Assert.IsTrue (protocol.IsOpen);
				protocol.SysEx.QueryFirmwareNameAndVersion ();
				var data = port.GetWritedData ();
				Assert.AreEqual ("F0 79 F7 ", data);
			}
		}

		[Test]
		public void CapabilityQuery_Valid_Success(){			

			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {

				Assert.IsTrue (protocol.IsOpen);
				protocol.SysEx.CapabilityQuery ();
				var data = port.GetWritedData ();
				Assert.AreEqual ("F0 6B F7 ", data);
			}
		}

		[Test]
		public void AnalogMappingQuery_Valid_Success(){

			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {

				Assert.IsTrue (protocol.IsOpen);
				protocol.SysEx.AnalogMappingQuery ();
				var data = port.GetWritedData ();
				Assert.AreEqual ("F0 69 F7 ", data);
			}
		}

		[Test]
		public void PinStateQuery_Valid_Success(){			

			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {

				Assert.IsTrue (protocol.IsOpen);
				protocol.SysEx.PinStateQuery (1);
				var data = port.GetWritedData ();
				Assert.AreEqual ("F0 6D 1 F7 ", data);
			}
		}

		[Test]
		public void SamplingInterval_Valid_Success(){			

			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {

				Assert.IsTrue (protocol.IsOpen);
				protocol.SysEx.SamplingInterval (1000);
				var data = port.GetWritedData ();
				Assert.AreEqual ("F0 7A 68 7 F7 ", data);
			}
		}
	}
}

