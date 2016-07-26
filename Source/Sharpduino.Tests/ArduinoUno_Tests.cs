using System;
using NUnit.Framework;

namespace Sharpduino.Tests
{
	[TestFixture]
	public class ArduinoUno_Tests
	{
		[Test]
		public void ArduinoUno_Basic_Tests ()
		{
			var comm = new TestCommTransports.TestTransport ();
			using (var board = new Boards.ArduinoUno (comm)) {
				var led = new Components.Led (13);
				board.AddComponent (led);
				led.On();
			}
			var x = comm.GetWritedData ();
		}
	}
}

