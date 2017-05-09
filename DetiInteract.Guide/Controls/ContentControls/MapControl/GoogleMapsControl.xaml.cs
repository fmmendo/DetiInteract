using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interaction logic for GoogleMapsControl.xaml
	/// </summary>
	public partial class GoogleMapsControl : UserControl, IDetiInteractControl
	{
		#region Fields
		/// <summary>
		/// Timer used for animation
		/// </summary>
		private Timer _timer;

		/// <summary>
		/// Counts the elapsed time.
		/// </summary>
		private int _time = 0;

		/// <summary>
		/// Tells the control to begin animation.
		/// </summary>
		private bool _animate = false;

		/// <summary>
		/// Informs the contorl if animation is ongoing.
		/// </summary>
		private bool _ongoingAnimation = false;

		/// <summary>
		/// Notifies MainView of end of animation.
		/// </summary>
		public event EventHandler AnimationEnd;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public GoogleMapsControl()
		{
			InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			Uri uri = new Uri(@"pack://application:,,,/Controls/ContentControls/MapControl/earth.html", UriKind.Absolute);
			Stream stream = Application.GetResourceStream(uri).Stream;
			using (StreamReader reader = new StreamReader(stream))
			{
				string html = reader.ReadToEnd();
				mapBrowser.NavigateToString(html);
			}
		}

		#region Javascript Calls
		/// <summary>
		/// Invokes the Javascript Pan method.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void Pan(double x, double y)
		{
			try
			{
				this.mapBrowser.InvokeScript("Pan", -y, x);
			}
			catch (Exception e) { }

		}

		/// <summary>
		/// Invokes the Javascript Pan method.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void MapZoom(double val)
		{
			try
			{
				this.mapBrowser.InvokeScript("Zoom", val);
			}
			catch (Exception e) { }

		}

		/// <summary>
		/// Invokes the Javascript Tilt method.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void Tilt(double val)
		{
			try
			{
				this.mapBrowser.InvokeScript("Tilt", Math.Abs(val));
			}
			catch (Exception e) { }
		}

		/// <summary>
		/// Invokes the Javascript Reset method.
		/// </summary>
		private void Reset()
		{
			try
			{
				this.mapBrowser.InvokeScript("Reset");
			}
			catch (Exception e) { }
		}

		/// <summary>
		/// Invokes the Javascript Animate method.
		/// </summary>
		private void Animate()
		{
			try
			{
				this.mapBrowser.InvokeScript("Animate");
			}
			catch (Exception e)
			{
				if (_animate)
				{
					if (AnimationEnd != null) AnimationEnd(this, new EventArgs());
				}
			}
		}

		/// <summary>
		/// Invokes the Javascript Zoom method.
		/// </summary>
		private void Zoom(double val)
		{
			try
			{
				this.mapBrowser.InvokeScript("Zoom", Math.Abs(val));
			}
			catch (Exception e)
			{

			}
		}
		#endregion

		#region Animation
		/// <summary>
		/// Begins the animation for this control
		/// </summary>
		public void StartAnimation()
		{
			_animate = true;
			_time = 0;
			_timer = new Timer(new TimerCallback(_timer_Tick), null, 500, 1000);
		}

		/// <summary>
		/// Aborts animation.
		/// </summary>
		public void StopAnimation()
		{
			if (_timer != null)
			{
				_timer.Dispose();
			}
			_time = 0;
			_ongoingAnimation = false;
		}

		/// <summary>
		/// Timer callback. Begins animation or resets the view, depending on 
		/// the nature of the call.
		/// </summary>
		/// <param name="state"></param>
		private void _timer_Tick(object state)
		{
			// If ordered to animate, call the Animate() method.
			if (_animate)
			{
				_time += 1;

				if (!_ongoingAnimation)
				{
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						Animate();
					}));
					_ongoingAnimation = true;
				}

				// If enought time hase elapsed, signal end of animation.
				if (_time == 45)
				{
					_ongoingAnimation = false;
					_animate = false;

					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						if (AnimationEnd != null) AnimationEnd(this, new EventArgs());

						_timer.Dispose();
					}));
				}
				return;
			}
			// else, reset the view.
			else
			{
				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
				{
					Reset();
					_timer.Dispose();
				}));
			}
		}
		#endregion

		#region Interaction
		/// <summary>
		/// 
		/// </summary>
		public void Tap()
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "TAP on GoogleMap.");
		}

		/// <summary>
		/// 
		/// </summary>
		public void LongPress()
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "LONGPRESS on GoogleMap.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Scroll(float x, float y)
		{
			Pan(x, y);
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "SCROLL on GoogleMap.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Fling(float x, float y)
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "FLING on GoogleMap.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>\
		/// <param name="z"></param>
		public void Rotation(float x, float y, float z)
		{
		}

		public void Zoom(float scale)
		{
			MapZoom(1 / scale);
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public void ControlLoaded()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void SetDimensions(int width, int height)
		{
			this.Width = width;
			this.Height = height;
			this.MinWidth = width;
			this.MinHeight = height;
			this.MaxWidth = width;
			this.MaxHeight = height;

			mapBrowser.Width = width - 100;
			mapBrowser.Height = height - 150;
			mapBrowser.MinWidth = width - 100;
			mapBrowser.MinHeight = height - 150;
			mapBrowser.MaxWidth = width - 100;
			mapBrowser.MaxHeight = height - 150;
		}


		public bool CanSwitchPage()
		{
			return true;
		}
	}
}
