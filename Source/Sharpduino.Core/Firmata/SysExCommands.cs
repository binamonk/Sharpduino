using System;
using Sharpduino.CommTransports;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sharpduino.Firmata
{
	public class SysExCommands
	{
		readonly ICommTransport _comm;
		List<SysExCommandsEnum> _pendingResponses;

		#region Events
		public delegate void ReceiveFirmwareResponse(int mayorVersion, int minorVersion, string name);
		public event ReceiveFirmwareResponse OnQueryFirmwareNameAndVersionResponse;
		#endregion

		public SysExCommands (ICommTransport comm)
		{
			_comm = comm;
			_pendingResponses = new List<SysExCommandsEnum> ();
		}

		public void ParseSysEx(byte [] message){
			switch (message [1]) {
			case (byte)SysExCommandsEnum.REPORT_FIRMWARE:
				ReceiveFirmware (message);
				break;
			case (byte)SysExCommandsEnum.CAPABILITY_RESPONSE:
				CapabilityResponse (message);
				break;
			}
		}

		#region Query Firmware Name and Version

		public void QueryFirmwareNameAndVersion(){			
			_comm.Write (new byte[] {
				(byte)((int)SysExCommandsEnum.START_SYSEX),
				(byte)((int)SysExCommandsEnum.REPORT_FIRMWARE),
				(byte)((int)SysExCommandsEnum.END_SYSEX)
			}, 0, 3);
			_pendingResponses.Add (SysExCommandsEnum.REPORT_FIRMWARE);
		}

		public void ReceiveFirmware (byte [] message){
			int mayorVersion = message [2];
			int minorVersion = message [3];
			var serverName = new System.Text.StringBuilder ();
			for (int pos = 4; pos <= 1024; pos += 2) {										
				if (message [pos] == (byte)SysExCommandsEnum.END_SYSEX) {					
					break;
				} else {
					serverName.Append (Convert.ToChar ((message[pos + 1] << 7) + message [pos]));
				}
			}
			string name = serverName.ToString ();
			if (OnQueryFirmwareNameAndVersionResponse != null) {
				OnQueryFirmwareNameAndVersionResponse (mayorVersion, minorVersion, name);
			}
		}
		#endregion

		// Extended Analog
		public void ExtendedAnalogMessage() {
			throw new NotImplementedException ();
		}

		// Capability Query
		public void CapabilityQuery(){			
			_comm.Write (new byte[] {
				(byte)((int)SysExCommandsEnum.START_SYSEX),
				(byte)((int)SysExCommandsEnum.CAPABILITY_QUERY),
				(byte)((int)SysExCommandsEnum.END_SYSEX)
			}, 0, 3);
			_pendingResponses.Add (SysExCommandsEnum.CAPABILITY_RESPONSE);
			// Implement response
		}

		public void CapabilityResponse (byte [] message){
			int pinId = 0;
			Debug.WriteLine ("Pin {0}", pinId);
			for (int count = 2 ; count < 1024 ; count += 2) {
				if (message[count] == (byte)SysExCommandsEnum.END_SYSEX) {
					break;
				}
				if (message [count] == 0x7F) {
					pinId++;
					count--;
					Debug.WriteLine ("Pin {0}", pinId);
				} else {
					Debug.WriteLine ("     Pin Mode {0}", message [count]);
					Debug.WriteLine ("     Pin Resolution {0}", message [count + 1]);
				}
			}
				
//			var x = 0;
//			int mayorVersion = message [2];
//			int minorVersion = message [3];
//			var serverName = new System.Text.StringBuilder ();
//			for (int pos = 4; pos <= 1024; pos += 2) {										
//				if (message [pos] == (byte)SysExCommandsEnum.END_SYSEX) {					
//					break;
//				} else {
//					serverName.Append (Convert.ToChar ((message[pos + 1] << 7) + message [pos]));
//				}
//			}
//			string name = serverName.ToString ();
//			if (OnQueryFirmwareNameAndVersionResponse != null) {
//				OnQueryFirmwareNameAndVersionResponse (mayorVersion, minorVersion, name);
//			}
		}

		// Analog Mapping Query
		public void AnalogMappingQuery(){			
			_comm.Write (new byte[] {
				(byte)((int)SysExCommandsEnum.START_SYSEX),
				(byte)((int)SysExCommandsEnum.ANALOG_MAPPING_QUERY),
				(byte)((int)SysExCommandsEnum.END_SYSEX)
			}, 0, 3);
			_pendingResponses.Add (SysExCommandsEnum.ANALOG_MAPPING_RESPONSE);
			// Implement response
		}

		// Pin State Query
		public void PinStateQuery(){			
			_comm.Write (new byte[] {
				(byte)((int)SysExCommandsEnum.START_SYSEX),
				(byte)((int)SysExCommandsEnum.PIN_STATE_QUERY),
				(byte)((int)SysExCommandsEnum.END_SYSEX)
			}, 0, 3);
			_pendingResponses.Add (SysExCommandsEnum.PIN_STATE_RESPONSE);
			// Implement response
		}

		public void SamplingInterval(int milliseconds){			
			_comm.Write (new byte[] {
				(byte)((int)SysExCommandsEnum.START_SYSEX),
				(byte)((int)SysExCommandsEnum.SAMPLING_INTERVAL),
				(byte)(milliseconds & 0x7F),
				(byte)(milliseconds >> 7),
				(byte)((int)SysExCommandsEnum.END_SYSEX)
			}, 0, 5);
		}
	}
}

