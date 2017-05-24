using System;
using Sharpduino.Firmata;
using System.Collections.Generic;
using Sharpduino.Components;
using System.Linq;

namespace Sharpduino.Boards
{
	public class ArduinoUno : IDisposable
	{
		readonly Protocol _protocol;
		const int DIGITAL_PINS = 13;

		List<IComponent> _components;

		public ArduinoUno (CommTransports.ICommTransport comm)
		{
			_protocol = new Protocol (comm);
			_components = new List<IComponent> ();
		}


		public ArduinoUno ()
		{
			_protocol = new Protocol ();
			_components = new List<IComponent> ();
		}

		public List<T> GetComponents<T>()
		{
			List<T> res = new List<T> ();
			var items =_components.Where (comp => comp.GetType () == typeof(T)).ToList();

			foreach (var item in items) {
				res.Add ((T)item);
			}
			return res;
		}

		public void AddComponent(IComponent newComponent){
			_components.Add (newComponent);
			var pins = newComponent.GetPins ();
			foreach (var pin in pins) {
				SetPinMode (pin);
				SetPinValue (pin);
			}
			newComponent.PinValueChanged += PinValueChanged;
		}

		public void SetPinMode(BoardPin pin){
			_protocol.SetPinMode (pin.Id, pin.Mode);
		}

		public void SetPinValue(BoardPin pin){
			_protocol.DigitalWrite (pin.Id, (Core.DigitalValue)pin.Value);
		}

		public void PinValueChanged(IComponent component){
			var pins = component.GetPins ();
			foreach (var pin in pins) {
				if (pin.Mode == Sharpduino.Core.PinMode.Output) {
					SetPinValue (pin);
				}
			}
		}

//		public void Led(int pinId){
//			if (pinId > 0 && pinId <= DIGITAL_PINS) {
//				_protocol.SetPinMode (pinId, Sharpduino.Core.PinMode.Output);
//			}
//		}
//
//		public void Led(int[] pinIds){
//			foreach (int pinId in pinIds) {
//				if (pinId > 0 && pinId <= DIGITAL_PINS) {
//					_protocol.SetPinMode (pinId, Sharpduino.Core.PinMode.Output);
//				}
//			}
//		}

		#region IDisposable implementation

		public void Dispose ()
		{
			if (_protocol.IsOpen) {
				_protocol.Close ();
			}
		}

		#endregion
	}
}

