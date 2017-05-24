using System;
using System.Threading.Tasks;
using System.Threading;

namespace Sharpduino.Components
{
	public class Led : IComponent
	{
		bool _stop = false;

		bool _state;
		bool State { 
			get { return _state; }
			set { 
				_state = value;
				if (PinValueChanged != null) {
					PinValueChanged (this);
				}
			}
		} 

		readonly int _pinId;
		public int Pin {
			get { return _pinId; }
		}

		public Led (int pinId)
		{
			_pinId = pinId;
		}

		/// <summary>
		/// Turn the led on.
		/// </summary>
		public Led On() {
			State = true;
			return this;
		}

		/// <summary>
		/// Turn the led off. If a led is strobing, it will not stop. Use Led.Stop().Off() to turn off a led while strobing.
		/// </summary>
		public Led Off() {
			State = false;
			return this;
		}

		/// <summary>
		/// Toggle the current state, if on then turn off, if off then turn on.
		/// </summary>
		public Led Toggle() {
			State = !_state;
			return this;
		}			

		public Led Strobe (int milliseconds = 500) {
			Task.Run (() => {
				while (!_stop){
					this.Toggle ();
					Task.Delay (milliseconds);
				}
				_stop = false;
			});
			return this;
		}
		/*

		strobe(ms, callback) Strobe/Blink the Led on/off in phases over ms with an optional callback. This is an interval operation and can be stopped by calling led.stop(), however that will not necessarily turn it "off". The callback will be invoked every time the Led turns on or off. Defaults to 100ms.

		var led = new five.Led(13);

		// Strobe on-off in 500ms phases
		led.strobe(500);
		blink(ms, callback) alias to strobe.

		var led = new five.Led(13);

		// Strobe on-off in 500ms phases
		led.blink(500);
		brightness(0-255) Set the brightness of led. This operation will only work with Leds attached to PWM pins.

		var led = new five.Led(11);

		// This will set the brightness to about half 
		led.brightness(128);
		fade(brightness, ms, callback) Fade from current brightness to brightness over ms with an optional callback. This is an interval operation and can be stopped by calling pin.stop(), however that will not necessarily turn it "off". This operation will only work with Leds attached to PWM pins.

		var led = new five.Led(11);

		// Fade to half brightness over 2 seconds
		led.fade(128, 2000);
		fade(animation options) Control the fading of an LED with Animation options.

		var led = new five.Led(11);

		led.fade({
			easing: "linear",
			duration: 1000,
			cuePoints: [0, 0.2, 0.4, 0.6, 0.8, 1],
			keyFrames: [0, 250, 25, 150, 100, 125],
			onstop: function() {
				console.log("Animation stopped");
			}
		});

		fadeIn(ms, callback) Fade in from current brightness over ms with an optional callback. This is an interval operation and can be stopped by calling pin.stop(), however that will not necessarily turn it "off". This operation will only work with Leds attached to PWM pins.

		var led = new five.Led(11);

		// Fade in over 500ms.
		led.fadeIn(500);
		fadeOut(ms, callback) Fade out from current brightness over ms with an optional callback. This is an interval operation and can be stopped by calling pin.stop(), however that will not necessarily turn it "off". This operation will only work with Leds attached to PWM pins.

		var led = new five.Led(11);

		// Fade out over 500ms.
		led.fadeOut(500);
		pulse(ms, callback) Pulse the Led in phases from on to off over ms time, with an optional callback. This is an interval operation and can be stopped by calling pin.stop(), however that will not necessarily turn it "off". The callback will be invoked every time the Led is fully on or off. This operation will only work with Leds attached to PWM pins.

		var led = new five.Led(11);

		// Pulse from on to off in 500ms phases
		led.pulse(500);
		pulse(animation options) Control the pulse of an LED with Animation options.

		var led = new five.Led(11);

		led.pulse({
			easing: "linear",
			duration: 3000,
			cuePoints: [0, 0.2, 0.4, 0.6, 0.8, 1],
			keyFrames: [0, 10, 0, 50, 0, 255],
			onstop: function() {
				console.log("Animation stopped");
			}
		});

		*/
		/// <summary>
		///  For interval operations, call stop to stop the interval. stop does not necessarily turn "off" the Led, in order to fully shut down an Led, a program must call stop().off(). This operation will only work with Leds attached to PWM pins.
		/// </summary>
		public Led Stop(){
			_stop = true;
			return this;
		}
			
		#region IComponent implementation

		public event ValueChange PinValueChanged;

		public Sharpduino.Boards.BoardPin[] GetPins ()
		{
			return new [] { 
				new Sharpduino.Boards.BoardPin{ 
					Id = _pinId, 
					Mode = Sharpduino.Core.PinMode.Output, 
					Value = _state ? 1 : 0 } };
		}

		#endregion

	}
}

