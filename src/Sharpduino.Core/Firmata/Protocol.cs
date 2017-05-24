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
        private SynchronizationContext _syncCtx;

        #region Events
        public delegate void OnServerVersion(int mayorVersion, int minorVersion, string name);
        public event OnServerVersion ServerVersion;

        public delegate void OnDataRead(int data);
        public event OnDataRead DataRead;

        public delegate void OnDigitalWrite(string data);
        public event OnDigitalWrite DigitalWriteData;

        public delegate void OnAnalogRead(string data);
        public event OnAnalogRead AnalogRead;
        #endregion

        #region Locals
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

        // These are ports array.
		volatile int[] _digitalOutputData = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		volatile int[] _digitalInputData  = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		volatile int[] _analogInputData   = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		private const int _majorVersion = 2;
		private const int _minorVersion = 5;

		public int ClientMayorVersion { get; set; } = 0;
		public int ClientMinorVersion { get; set; } = 0;

		Task _readThread = null;
		object _locker = new object();

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
        #endregion

        #region Constructors

        public Protocol(ICommTransport commTransport)
		{
			_commTransport = commTransport;
			_sysEx = new SysExCommands (_commTransport);
            _syncCtx = SynchronizationContext.Current;
            _sysEx.OnQueryFirmwareNameAndVersionResponse += _sysEx_OnQueryFirmwareNameAndVersionResponse;
			if (_commTransport.AutoStart) {
				Open ();
			}	
		}

        private void _sysEx_OnQueryFirmwareNameAndVersionResponse(int majorVersion, int minorVersion, string name)
        {
            var serverVersion = majorVersion + (minorVersion * 0.1);
            var clientVersion = this.ClientMayorVersion + (this.ClientMinorVersion * 0.1);
            if (serverVersion < clientVersion) {
                throw new Exception(String.Format("Please update Firmata version on device to {0}.{1}", ClientMayorVersion, ClientMinorVersion));
            }
            ClientMayorVersion = majorVersion;
            ClientMinorVersion = minorVersion;
            ServerVersion?.Invoke(majorVersion, minorVersion, name);
        }

        #endregion

        /// <summary>
        /// Opens the serial port connection, should it be required. By default the port is
        /// opened when the object is first created.
        /// </summary>
        public void Open()
		{			
			Task.Delay(_delay);
            _commTransport.Open();
            //TODO: Enable for production
            //for (int pin = 0; pin < 6; pin++)
            //{
            //    ToggleAnalogInReporting(pin, true);
            //}

            //for (int port = 0; port < 2; port++)
            //{
            //    ToggleDigitalPortReporting(port, true);
            //}

            if (_readThread == null)
			{
				_readThread = new Task(ProcessInput);
				_readThread.Start();
			}
		}

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        public void Close()
        {
            if (_commTransport != null)
            {
                _commTransport.Close();
                _readThread = null;
            }
		}

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
				_digitalOutputData [port] &= ~(1 << (pin & 0x07));			
			} else {
				_digitalOutputData [port] |= (1 << (pin & 0x07));
			}
            _commTransport.Write(new byte[3] {
                (byte)((int)MessageTypesEnum.DIGITAL_MESSAGE | port),
                (byte)(_digitalOutputData [port] & (byte)SysExCommandsEnum.REALTIME),
                (byte)(_digitalOutputData [port] >> 7)
            }, 0, 3);
            string data = string.Empty;
            foreach (int n in _digitalOutputData) {
                data = data + string.Format("{0:X}, ", n);
            }
            if (_syncCtx != null)
            {
                _syncCtx.Send((x) => DigitalWriteData?.Invoke(data), null);
            }
            else {
                DigitalWriteData?.Invoke(data);
            }
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
				(byte)MessageTypesEnum.REPORT_VERSION,
				(byte)_majorVersion,
				(byte)_minorVersion
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
				(byte)(MessageTypesEnum.SET_DIGITAL_PIN_VALUE),
				(byte)(pin),
				(byte)(value)
			} , 0, 3);
		}

        /// <summary>
        /// Toogles the analog in reporting.
        /// </summary>
        /// <param name="pin">Arduino pin to report.</param>
        /// <param name="enable">If set to <c>true</c> report pin.</param>
        public void ToggleAnalogInReporting(int pin, bool enable){
			_commTransport.Write (new byte[2] { 
				(byte)((int)MessageTypesEnum.REPORT_ANALOG | pin),
				(byte)((enable) ? 0x01 : 0x00)
			}, 0, 2);
		}

        /// <summary>
        /// Toogles the digital in reporting.
        /// </summary>
        /// <param name="port">Arduino port to report.</param>
        /// <param name="enable">If set to <c>true</c> report pin.</param>
        public void ToggleDigitalPortReporting(int port, bool enable)
        {
			_commTransport.Write (new byte[2] { 
				(byte)((int)MessageTypesEnum.REPORT_DIGITAL | port),
				(byte)((enable) ? 0x01 : 0x00)
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
		private int digitalRead(int pin)
		{
			return (_digitalInputData[pin >> 3] >> (pin & 0x07)) & 0x01;
		}

		/// <summary>
		/// Returns the last known state of the analog pin.
		/// </summary>
		/// <param name="pin">The arduino analog input pin.</param>
		/// <returns>A value representing the analog value between 0 (0V) and 1023 (5V).</returns>
		private int analogRead(int pin)
		{
			return _analogInputData[pin];
		}

		private void setDigitalInputs(int portNumber, int portData)
		{
			_digitalInputData[portNumber] = portData;

            string data = string.Empty;
            foreach (int n in _digitalInputData)
            {
                data = data + string.Format("{0:X}, ", n);
            }

            if (DigitalWriteData != null)
            {
                if (_syncCtx != null)
                {
                    _syncCtx.Send((x) => DigitalWriteData?.Invoke(data), null);
                }
                else
                {
                    DigitalWriteData?.Invoke(data);
                }
            }
        }

		private void setAnalogInput(int pin, int value)
		{
			_analogInputData[pin] = value;

            string data = string.Empty;
            foreach (int n in _analogInputData)
            {
                data = data + string.Format("{0:X}, ", n);
            }
            
            if (AnalogRead != null)
            {
                if (_syncCtx != null)
                {
                    _syncCtx.Send((x) => AnalogRead?.Invoke(data), null);
                }
                else {
                    AnalogRead?.Invoke(data);
                }
            }
		}

		private void SetServerVersion(int majorVersion, int minorVersion)
		{
			Debug.WriteLine ("Firmata Server Version: {0}.{1}", majorVersion, minorVersion);
            ClientMayorVersion = majorVersion;
            ClientMinorVersion = minorVersion;
            if (ServerVersion != null)
            {
                if (_syncCtx != null)
                {
                    _syncCtx.Send((x) => ServerVersion?.Invoke(majorVersion, minorVersion, ""), null);
                }
                else
                {
                    ServerVersion?.Invoke(majorVersion, minorVersion, "");
                }
            }
		}
        #endregion

		private void ProcessInput()
		{
			while (_commTransport.IsOpen)
			{
				if (_commTransport.BytesToRead > 0)
				{
					lock (this)
					{
						int inputData = _commTransport.ReadByte();
						int command;
						Debug.WriteLine ("{0:X}", inputData);
                        if (DataRead != null)
                        {
                            _syncCtx.Send((x) => DataRead.Invoke(inputData), null);
                        }
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
								switch (_executeMultiByteCommand) {
								    case (int)MessageTypesEnum.DIGITAL_MESSAGE:
									    setDigitalInputs (_multiByteChannel, (_storedInputData [0] << 7) + _storedInputData [1]);
									    break;
								    case (int)MessageTypesEnum.ANALOG_MESSAGE:
									    setAnalogInput (_multiByteChannel, (_storedInputData [0] << 7) + _storedInputData [1]);
									    break;
								    case (int)MessageTypesEnum.REPORT_VERSION:
									    SetServerVersion (_storedInputData [1], _storedInputData [0]);
									    break;
								}
							}
						}
						else
						{
							command = inputData;
                            if ((command & 0xF0) == (int)MessageTypesEnum.DIGITAL_MESSAGE) {
                                _multiByteChannel = command & 0x0F;
                                _waitForData = 2;
                                _executeMultiByteCommand = (int)MessageTypesEnum.DIGITAL_MESSAGE;
                            }
                            if ((command & 0xE0) == (int)MessageTypesEnum.ANALOG_MESSAGE)
                            {
                                _multiByteChannel = command & 0x0F;
                                _waitForData = 2;
                                _executeMultiByteCommand = (int)MessageTypesEnum.ANALOG_MESSAGE;
                            }
                            switch (command) {
							    case (int)MessageTypesEnum.START_SYSEX:
								    _sysexBytesRead = 0;
								    _storedInputData [_sysexBytesRead] = (byte)MessageTypesEnum.START_SYSEX;
								    _sysexBytesRead++;
								    _parsingSysex = true;
								    break;
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

		private void ProcessSysexMessage() {
			_sysEx.ParseSysEx (_storedInputData);
			_sysexBytesRead = 0;
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			if (IsOpen) {
				Close ();
			}
            _sysEx.OnQueryFirmwareNameAndVersionResponse -= _sysEx_OnQueryFirmwareNameAndVersionResponse;
        }
		#endregion
	}
}
