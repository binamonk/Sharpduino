using NUnit.Framework;
using System;

namespace Sharpduino.Tests.Firmata
{
	[TestFixture ()]
	public class Protocol_ControlRead_Tests
	{

		#region Data Message Expansion
		[Test]
		public void AnalogPin_Read_Success (){
			var port = new TestCommTransports.TestTransport ();

			using (var protocol = new Sharpduino.Firmata.Protocol (port)) {
				Assert.IsTrue (protocol.IsOpen);
                bool dataReceived = false;
                protocol.AnalogRead += (string data) => {
                    dataReceived = true;
                    Assert.AreEqual(data, "1E1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ");
                };
                port.SimulateReception("E0 61 3");
                while (!dataReceived) ;
			}
		}

        [Test]
        public void DigitalDataPin_Read_Success()
        {
            var port = new TestCommTransports.TestTransport();

            using (var protocol = new Sharpduino.Firmata.Protocol(port))
            {
                Assert.IsTrue(protocol.IsOpen);
                bool dataReceived = false;
                protocol.DigitalWriteData += (string data) => {
                    dataReceived = true;
                    Assert.AreEqual(data, "0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ");
                };
                port.SimulateReception("90 0 0");
                while (!dataReceived);
            }
        }

        [Test]
        public void FirmwareMessage_Read_Success()
        {
            var port = new TestCommTransports.TestTransport();

            using (var protocol = new Sharpduino.Firmata.Protocol(port))
            {
                Assert.IsTrue(protocol.IsOpen);
                bool dataReceived = false;
                protocol.ServerVersion += (int mayorVersion, int minorVersion, string name) =>
                {
                    dataReceived = true;
                    Assert.AreEqual(mayorVersion, 2);
                    Assert.AreEqual(minorVersion, 5);
                    Assert.AreEqual(name, "");
                };
                port.SimulateReception("F9 2 5");
                while (!dataReceived);
            }
        }

        //[Test]
        //public void FirmwareMessage_Read_Success()
        //{
        //    var port = new TestCommTransports.TestTransport();

        //    using (var protocol = new Sharpduino.Firmata.Protocol(port))
        //    {
        //        Assert.IsTrue(protocol.IsOpen);
        //        bool dataReceived = false;
        //        protocol.ServerVersion += (int mayorVersion, int minorVersion, string name) =>
        //        {
        //            dataReceived = true;
        //            Assert.AreEqual(mayorVersion, 2);
        //            Assert.AreEqual(minorVersion, 5);
        //            Assert.AreEqual(name, "StandardFirmata.ino");
        //        };
        //        port.SimulateReception("F0 79 2 5 53 0 74 0 61 0 6E 0 64 0 61 0 72 0 64 0 46 0 69 0 72 0 6D 0 61 0 74 0 61 0 2E 0 69 0 6E 0 6F 0 F7");
        //        while (!dataReceived) ;
        //    }
        //}
        
        #endregion

    }
}

