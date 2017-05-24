using System;
using Sharpduino.CommTransports;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

namespace Sharpduino.Firmata
{
	public class SysExCommands
	{
		private readonly ICommTransport _comm;
		private List<SysExCommandsEnum> _pendingResponses;

        System.Threading.SynchronizationContext _syncCtx;
        #region Events
        public delegate void ReceiveFirmwareResponse(int mayorVersion, int minorVersion, string name);
		public event ReceiveFirmwareResponse OnQueryFirmwareNameAndVersionResponse;
		#endregion

		public SysExCommands (ICommTransport comm)
		{
			_comm = comm;
			_pendingResponses = new List<SysExCommandsEnum> ();
            _syncCtx = System.Threading.SynchronizationContext.Current;
            
		}

		public void ParseSysEx(byte [] message){
			switch (message [1]) {
			    case (byte)SysExCommandsEnum.REPORT_FIRMWARE:
				    ReceiveFirmware (message);
				    break;
			    case (byte)SysExCommandsEnum.CAPABILITY_RESPONSE:
				    CapabilityResponse (message);
				    break;
                case (byte)SysExCommandsEnum.EXTENDED_ANALOG:
                    ExtendedAnalogMessage(message);
                    break;
                case (byte)SysExCommandsEnum.ANALOG_MAPPING_RESPONSE:
                    AnalogMappingResponse(message);
                    break;
                case (byte)SysExCommandsEnum.PIN_STATE_RESPONSE:
                    PinStateResponse(message);
                    break;
			}
		}

		#region Query Firmware Name and Version

		public void QueryFirmwareNameAndVersion(){			
			_comm.Write (new byte[] {
				(byte)SysExCommandsEnum.START_SYSEX,
				(byte)SysExCommandsEnum.REPORT_FIRMWARE,
				(byte)SysExCommandsEnum.END_SYSEX
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
            if (OnQueryFirmwareNameAndVersionResponse != null)
            {
                if (_syncCtx != null)
                {
                    _syncCtx.Send((x) => OnQueryFirmwareNameAndVersionResponse?.Invoke(mayorVersion, minorVersion, serverName.ToString()), null);
                }
                else {
                    OnQueryFirmwareNameAndVersionResponse?.Invoke(mayorVersion, minorVersion, serverName.ToString());
                }
            }
        }
		#endregion

		// Extended Analog
		public void ExtendedAnalogMessage(byte[] message) {
            throw new NotImplementedException();
		}

        #region Capability Query (WIP)
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
            bool isMode = true;
            bool pinPrinted = false;
            for (int count = 2 ; count < 1024 ; count ++) {
                if (message[count] == (byte)SysExCommandsEnum.END_SYSEX) {
					break;
				}
                if (!pinPrinted) {
                    Debug.WriteLine("Pin {0}", pinId);
                    pinPrinted = true;
                }
                if (message[count] == 0x7F)
                {
                    pinId++;
                    pinPrinted = false;
                    isMode = true;
                    continue;
                }
                else {
                    
                    if (isMode)
                    {
                        Debug.WriteLine("     Pin Mode {0}", Enum.Parse(typeof(Core.CapabilityPinMode), message[count].ToString()).ToString());
                        isMode = false;
                    }
                    else
                    {
                        Debug.WriteLine("     Pin Resolution {0}", message[count + 1]);
                        isMode = true;
                    }
                }
			}
		}

        #endregion

        #region Analog Mapping Query
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

        public int[] AnalogMappingResponse(byte[] message) {
            int[] res = new int[message.Length - 3];
            int count = 2;
            while (message[count] != 0xF7 || count < 1024)
            //for (int count = 2; count < 1024; count += 1)
            {
                if (message[count] == (byte)SysExCommandsEnum.END_SYSEX)
                {
                    break;
                }
                else {
                    res[count - 2] = message[count];
                    if (message[count] == 0x7F)
                    {
                        Debug.WriteLine("Pin {0} not able to do Analog operations", count - 2);
                    }
                    else {
                        Debug.WriteLine("Pin {0} able to do Analog operations at channel {1}", count - 2, message[count]);
                    }
                }
                count++;
            }
            return res;
        }

        #endregion

        #region Pin State Query (WIP)
        // Pin State Query
        public void PinStateQuery(int pin){			
			_comm.Write (new byte[] {
				(byte)((int)SysExCommandsEnum.START_SYSEX),
				(byte)((int)SysExCommandsEnum.PIN_STATE_QUERY),
                (byte)pin,
                (byte)((int)SysExCommandsEnum.END_SYSEX)
			}, 0, 4);
			_pendingResponses.Add (SysExCommandsEnum.PIN_STATE_RESPONSE);
			// Implement response
		}

        public void PinStateResponse(byte[] message) {
            for (int count = 2; count < 1024; count++)
            {
                if (message[count] == (int)SysExCommandsEnum.END_SYSEX) {
                    break;
                }
                switch (count) {
                    case 2:
                        Debug.WriteLine("Requested pin is {0}", message[count]);
                        break;
                    case 3:
                        Debug.WriteLine("Pin Mode Is {0}", Enum.Parse(typeof(Core.CapabilityPinMode),message[count].ToString()).ToString());
                        break;
                    default:
                        Debug.WriteLine("Pin state {0}", message[count]);
                        break;
                }
            }
        }
        #endregion

        public void SamplingInterval(int milliseconds){
			_comm.Write (new byte[] {
				(byte)SysExCommandsEnum.START_SYSEX,
				(byte)SysExCommandsEnum.SAMPLING_INTERVAL,
				(byte)(milliseconds & 0x7F),
				(byte)(milliseconds >> 7),
				(byte)SysExCommandsEnum.END_SYSEX
			}, 0, 5);
		}
	}
}

