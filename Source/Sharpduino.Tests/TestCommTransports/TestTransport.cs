using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpduino.Tests.TestCommTransports
{
	public class TestTransport : Sharpduino.CommTransports.ICommTransport
	{
		private bool _isOpen = false;

		StringBuilder _writeBuffer;

		public TestTransport ()
		{
			_writeBuffer = new StringBuilder ();
			_isOpen = AutoStart;
            _memoryStream = new System.IO.MemoryStream();
		}

		#region ICommTransport implementation

		public void Open ()
		{
			_isOpen = true;
		}

		public void Close ()
		{
			_isOpen = false;
		}

		public void Write (byte[] buffer, int offset, int count)
		{
			foreach (byte data in buffer) {
				_writeBuffer.AppendFormat ("{0:X} ", data);
			}
		}

		public int ReadByte ()
		{;
            var dataByte =  _memoryStream.ReadByte();
            if (_memoryStream.Position == _memoryStream.Length) {
                _memoryStream.Flush();
            }
            return dataByte;
        }

		public bool IsOpen {
			get {
				return _isOpen;
			}
		}


		public int DataBits {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

        volatile System.IO.MemoryStream _memoryStream;
		public int BytesToRead {
			get {
				return (int)_memoryStream.Length;
			}
		}

		public bool AutoStart { get; set; } = true;


		public int Parity { get; set; }

		public int StopBits { get; set; }

		public string GetWritedData(){
			var buffer = _writeBuffer.ToString ();
			_writeBuffer.Clear ();
			return buffer;
		}
        #endregion

        public void SimulateReception(string message)
        {
            var strData = message.Split(' ').ToList();
            byte[] data = strData.Select(it => (byte)Convert.ToInt32(it,16)).ToArray();
            _memoryStream.Write(data, 0, data.Length);
            _memoryStream.Position = 0;
        }
    }
}

