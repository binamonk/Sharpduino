using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Sharpduino.CommTransports;
using System.Threading.Tasks;


namespace Sharpduino.Firmata
{
	/// <summary>
	/// This class implements the protocol for communications. https://github.com/firmata/protocol/blob/master/protocol.md
	/// </summary>
	public class Protocol : IDisposable
	{
		const int MAX_DATA_BYTES   = 1024;

		const int MAX_AVAILABLE_PIN = 127;

		readonly ICommTransport _commTransport;
		const int _delay = 3000;

		int _waitForData = 0;
		int _executeMultiByteCommand = 0;
		int _multiByteChannel = 0;
		readonly byte[] _storedInputData = new byte[MAX_DATA_BYTES];
		bool _parsingSysex;
		int _sysexBytesRead;

		volatile int[] digitalOutputData = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		volatile int[] digitalInputData  = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		volatile int[] analogInputData   = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		const int _majorVersion = 2;
		const int _minorVersion = 4;

		public int ClientMayorVersion { get; set; } = 0;
		public int ClientMinorVersion { get; set; } = 0;

		Task readThread = null;
		object locker = new object();

		SysExCommands _sysEx;
		public SysExCommands SysEx {
			get {
				return _sysEx;
			}
		}

		public bool IsOpen { 
			get { 
				return (_commTransport != null) && _commTransport.IsOpen; 
			} 
		}

		#region Constructors

		public Protocol()
		{
		}

		public Protocol(ICommTransport commTransport)
		{
			_commTransport = commTransport;
			_sysEx = new SysExCommands (_commTransport);
			if (_commTransport.AutoStart) {
				Open ();
			}	
		}
			
		#endregion

		/// <summary>
		/// Opens the serial port connection, should it be required. By default the port is
		/// opened when the object is first created.
		/// </summary>
		public void Open()
		{			

			Task.Delay(_delay);
			//TODO: Watch for inputs
//			byte[] command = new byte[2];
//
//			for (int i = 0; i < 6; i++)
//			{
//				command[0] = (byte)((int)MessageType.REPORT_ANALOG | i);
//				command[1] = (byte)1;
//				_commTransport.Write(command, 0, 2);
//			}
//
//			for (int i = 0; i < 2; i++)
//			{
//				command[0] = (byte)((int)MessageType.REPORT_DIGITAL | i);
//				command[1] = (byte)1;
//				_commTransport.Write(command, 0, 2);
//			}
//			command = null;

			if (readThread == null)
			{
				readThread = new Task(ProcessInput);
				readThread.Start();
			}
		}

		/// <summary>
		/// Closes the serial port.
		/// </summary>
		public void Close()
		{
//			readThread.Join(500);
//			readThread = null;
			_commTransport.Close();
		}

		/// <summary>
		/// Lists all available serial ports on current system.
		/// </summary>
		/// <returns>An array of strings containing all available serial ports.</returns>
//		public static string[] list()
//		{
//			return SerialPort.GetPortNames();
//		}

		#region Data Message Expanssion

		/// <summary>
		/// Writes to digital output.
		/// </summary>
		/// <param name="pin">Pin.</param>
		/// <param name="value">Value.</param>
		public void DigitalWrite(int pin, Sharpduino.Core.DigitalValue value){
			if (pin > MAX_AVAILABLE_PIN) {
				throw new NotImplementedException ();
			}
			int port = (pin >> 3) & 0x0F;
			if (value == 0) {
				digitalOutputData [port] &= ~(1 << (pin & 0x07));			
			} else {
				digitalOutputData [port] |= (1 << (pin & 0x07));
			}
			_commTransport.Write (new byte[3] {
				(byte)((int)MessageTypesEnum.DIGITAL_MESSAGE | port),
				(byte)(digitalOutputData [port] & 0x7F),
				(byte)(digitalOutputData [port] >> 7)
			}, 0, 3);			
		}

		/// <summary>
		/// Write to an analog pin using Pulse-width modulation (PWM).
		/// </summary>
		/// <param name="pin">Analog output pin.</param>
		/// <param name="value">PWM frequency from 0 (always off) to 255 (always on).</param>
		public void AnalogWrite(int pin, int value)
		{			
			_commTransport.Write (new byte[3] {
				(byte)((int)MessageTypesEnum.ANALOG_MESSAGE | (pin & 0x0F)),
				(byte)(value & 0x7F),
				(byte)(value >> 7)
			}, 0, 3);
		}

		/// <summary>
		/// Reports the version.
		/// </summary>
		public void ReportVersion(){
			_commTransport.Write (new byte[3] {
				(byte)((int)MessageTypesEnum.REPORT_VERSION),
				(byte)(_majorVersion),
				(byte)(_minorVersion)
			}, 0, 3);
		}
		#endregion

		#region Control Messages Expansion
		/// <summary>
		/// Sets the mode of the specified pin.
		/// </summary>
		/// <param name="pin">Arduino pin number.</param>
		/// <param name="mode">Pin Mode.</param>
		public void SetPinMode(int pin, Sharpduino.Core.PinMode mode)
		{
			_commTransport.Write(new byte[3] { 
				(byte)(MessageTypesEnum.SET_PIN_MODE),
				(byte)(pin),
				(byte)(mode)
			} , 0, 3);
		}

		/// <summary>
		/// Sets the digital pin value.
		/// </summary>
		/// <param name="pin">Pin.</param>
		/// <param name="value">Value.</param>
		public void SetDigitalPinValue(int pin, Sharpduino.Core.DigitalValue value)
		{
			_commTransport.Write(new byte[3] { 
				(byte)(MessageTypesEnum.SetDigitalPinValue),
				(byte)(pin),
				(byte)(value)
			} , 0, 3);
		}			

		/// <summary>
		/// Toogles the analog in reporting.
		/// </summary>
		/// <param name="pin">Arduino pin to report.</param>
		/// <param name="reportPin">If set to <c>true</c> report pin.</param>
		public void ToggleAnalogInReporting(int pin, bool reportPin){
			_commTransport.Write (new byte[2] { 
				(byte)((int)MessageTypesEnum.REPORT_ANALOG | pin),
				(byte)((reportPin) ? 0x01 : 0x00)
			}, 0, 2);
		}

		/// <summary>
		/// Toogles the digital in reporting.
		/// </summary>
		/// <param name="port">Arduino pin to report.</param>
		/// <param name="reportPin">If set to <c>true</c> report pin.</param>
		public void ToggleDigitalPortReporting(int port, bool reportPin){
			_commTransport.Write (new byte[2] { 
				(byte)((int)MessageTypesEnum.REPORT_DIGITAL | port),
				(byte)((reportPin) ? 0x01 : 0x00)
			}, 0, 2);
		}

		/// <summary>
		/// Requests version report.
		/// </summary>
		public void RequestVersionReport(){
			_commTransport.Write (new byte[1] { (byte)((int)MessageTypesEnum.REPORT_VERSION) }, 0, 1);
		}
		#endregion

		#region Read Commands
		/// <summary>
		/// Returns the last known state of the digital pin.
		/// </summary>
		/// <param name="pin">The arduino digital input pin.</param>
		/// <returns>Arduino.HIGH or Arduino.LOW</returns>
		public int digitalRead(int pin)
		{
			return (digitalInputData[pin >> 3] >> (pin & 0x07)) & 0x01;
		}

		/// <summary>
		/// Returns the last known state of the analog pin.
		/// </summary>
		/// <param name="pin">The arduino analog input pin.</param>
		/// <returns>A value representing the analog value between 0 (0V) and 1023 (5V).</returns>
		public int analogRead(int pin)
		{
			return analogInputData[pin];
		}

		private void setDigitalInputs(int portNumber, int portData)
		{
			digitalInputData[portNumber] = portData;
		}

		private void setAnalogInput(int pin, int value)
		{
			analogInputData[pin] = value;
		}

		private void SetClientVersion(int majorVersion, int minorVersion)
		{
			ClientMayorVersion = majorVersion;
			ClientMinorVersion = minorVersion;
			Debug.WriteLine ("Firmata Server Version: {0}.{1}", ClientMayorVersion, ClientMinorVersion);
		}
		#endregion
		private int available()
		{
			return _commTransport.BytesToRead;
		} 

		public void ProcessInput()
		{
			while (_commTransport.IsOpen)
			{
				if (_commTransport.BytesToRead > 0)
				{
					lock (this)
					{
						int inputData = _commTransport.ReadByte();
						int command;
						//System.Diagnostics.Debug.WriteLine ("{0:X}", inputData);
						if (_parsingSysex)
						{
							if (inputData == (int)MessageTypesEnum.END_SYSEX)
							{
								_parsingSysex = false;
								_storedInputData[_sysexBytesRead] = (byte)MessageTypesEnum.END_SYSEX;
								ProcessSysexMessage();
							}
							else
							{
								_storedInputData[_sysexBytesRead] = (byte)inputData;
								_sysexBytesRead++;
							}
						}
						else if (_waitForData > 0 && inputData < 128)
						{
							_waitForData--;
							_storedInputData[_waitForData] = (byte)inputData;

							if (_executeMultiByteCommand != 0 && _waitForData == 0)
							{
								//we got everything
								switch (_executeMultiByteCommand) {
								case (int)MessageTypesEnum.DIGITAL_MESSAGE:
									setDigitalInputs (_multiByteChannel, (_storedInputData [0] << 7) + _storedInputData [1]);
									break;
								case (int)MessageTypesEnum.ANALOG_MESSAGE:
									setAnalogInput (_multiByteChannel, (_storedInputData [0] << 7) + _storedInputData [1]);
									break;
								case (int)MessageTypesEnum.REPORT_VERSION:
									SetClientVersion (_storedInputData [1], _storedInputData [0]);
									break;
								}
							}
						}
						else
						{
							if (inputData < 0xF0)
							{
								command = inputData & 0xF0;
								_multiByteChannel = inputData & 0x0F;
							}
							else
							{
								command = inputData;
								// commands in the 0xF* range don't use channel data
							}
							switch (command) {
							case (int)MessageTypesEnum.START_SYSEX:
								_sysexBytesRead = 0;
								_storedInputData [_sysexBytesRead] = (byte)MessageTypesEnum.START_SYSEX;
								_sysexBytesRead++;
								_parsingSysex = true;
								break;
							case (int)MessageTypesEnum.DIGITAL_MESSAGE:
							case (int)MessageTypesEnum.ANALOG_MESSAGE:
							case (int)MessageTypesEnum.REPORT_VERSION:
								_waitForData = 2;
								_executeMultiByteCommand = command;
								break;
							}
						}
					}
				} 
			}
		}

		public void ProcessSysexMessage() {
			_sysEx.ParseSysEx (_storedInputData);
			//System.Diagnostics.Debug.WriteLine ("{0:X}", _storedInputData[0]);
			_sysexBytesRead = 0;
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			if (this.IsOpen) {
				this.Close ();
			}
		}
		#endregion
	} // End Arduino class

} // End namespace
