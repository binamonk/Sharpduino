using System;

namespace Sharpduino.Core
{
	public enum PinMode
	{
		Input , 
		Output,
		Analog,
		Pwm,
		Servo,
		I2C = 6,
		OneWire,
		Stepper,
		Encoder
	}
}

