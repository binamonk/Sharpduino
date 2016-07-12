
namespace Sharpduino.CommTransports
{
	public interface ICommTransport
	{
		bool IsOpen { get; }
		int DataBits { get; set; }
		int BytesToRead { get; }
		bool AutoStart { get; set; }
		int Parity { get; set; }
		int StopBits { get; set; }

		void Open();
		void Close();
		void Write( byte[] buffer, int offset, int count);
		int ReadByte();
	}
}

