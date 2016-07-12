using System;
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
		{
			throw new NotImplementedException ();
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

		public int BytesToRead {
			get {
				return 0;
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
	}
}

