using System;
using Sharpduino.Boards;

namespace Sharpduino.Components
{
	public delegate void ValueChange(IComponent component);

	public interface IComponent
	{
		BoardPin[] GetPins ();

		event ValueChange PinValueChanged;
	}
}

