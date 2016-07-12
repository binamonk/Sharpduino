using System;

namespace Sharpduino.Boards
{
	public class BoardPin
	{
		public int Id { get; set; }
		public Core.PinMode Mode { get; set; }
		public int Value { get; set; }

	}
}

