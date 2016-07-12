using System;

namespace Sharpduino.Firmata
{
	public enum MessageTypesEnum
	{
		ANALOG_MESSAGE        			= 0xE0, // send data for an analog pin (or PWM)
		DIGITAL_MESSAGE       			= 0x90, // send data for a digital port
		REPORT_ANALOG         			= 0xC0, // enable analog input by pin #
		REPORT_DIGITAL        			= 0xD0, // enable digital input by port

		START_SYSEX           			= 0xF0, // start a MIDI SysEx message
		SET_PIN_MODE          			= 0xF4, // set a pin to INPUT/OUTPUT/PWM/etc
		SetDigitalPinValue    			= 0xF5, // set a digital pin to INPUT/OUTPUT
		END_SYSEX             			= 0xF7, // end a MIDI SysEx message
		REPORT_VERSION        			= 0xF9, // report firmware version

		EXTENDED_STRING       			= 0x71, // String
		EXTENDED_VERSION     			= 0x79, // Firmware name/version
	}
}

