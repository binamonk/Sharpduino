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
		Encoder,
        Serial,
        PullUp
	}

    public enum CapabilityPinMode {
        DIGITAL_INPUT = 0x00,
DIGITAL_OUTPUT     =0x01,
ANALOG_INPUT       =0x02,
PWM                =0x03,
SERVO              =0x04,
SHIFT              =0x05,
I2C                =0x06,
ONEWIRE            =0x07,
STEPPER            =0x08,
ENCODER            =0x09,
SERIAL             =0x0A,
INPUT_PULLUP       =0x0B
    }
}

