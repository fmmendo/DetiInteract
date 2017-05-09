using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DetiInteract.Guide.Controls.TeacherControl;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using mshtml;

namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interaction logic for TeacherView.xaml
	/// </summary>
	public sealed partial class TeacherView : UserControl, IDetiInteractControl
	{
		#region Fields
		/// <summary>
		/// The view model for this view
		/// </summary>
		private TeacherViewModel _viewmodel;

		/// <summary>
		/// Reference to the scroll viewer, to allow scrolling.
		/// </summary>
		private ScrollViewer _scrollviewer;

		/// <summary>
		/// Mouse position, used to allow scrolling on the touch device.
		/// </summary>
		private Point _mousePosition = new Point(-1, -1);

		/// <summary>
		/// Timer to calculate the fling decceleration.
		/// </summary>
		private Timer _flingTimer;

		/// <summary>
		/// Time used in the calculation of the fling decceleration.
		/// </summary>
		private float _time = 0;

		/// <summary>
		/// Fling speed used to calculate the fling decceleration.
		/// </summary>
		private float _flingSpeed;

		/// <summary>
		/// Amount to scroll in each iteration of a fling.
		/// </summary>
		private float _scrollValue;

		/// <summary>
		/// Scrollviewer offset, used for animation.
		/// </summary>
		private double _offset = 0;

		/// <summary>
		/// The currently selected line.
		/// </summary>
		private int _selectedLine = 0;

		/// <summary>
		/// The currently selected column.
		/// </summary>
		private int _selectedColumn = 0;

		/// <summary>
		/// Indicates if the user has entered selection mode.
		/// </summary>
		private bool _selecting = false;

		/// <summary>
		/// The number of columns in the Listbox.
		/// </summary>
		private int _columnCount;

		/// <summary>
		/// Number of lines in the listbox.
		/// </summary>
		private int _lineCount;

		/// <summary>
		/// Indicates if the users has entered browsing mode.
		/// </summary>
		private bool _browsing = false;

		/// <summary>
		/// Indicates if the interaction with links in the browser is locked
		/// </summary>
		private bool _browserLock = false;

		/// <summary>
		/// Holds the currently open html page, used to allow scrolling.
		/// </summary>
		private mshtml.HTMLDocument _htmlDoc;

		/// <summary>
		/// Triggered once animation has finished.
		/// </summary>
		public event System.EventHandler AnimationEnd;

		/// <summary>
		/// Timer used for animation purposes.
		/// </summary>
		private Timer _animationTimer;

		/// <summary>
		/// DLL import to get the IWebBrowser2 interface
		/// </summary>
		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
		internal interface IServiceProvider
		{
			[return: MarshalAs(UnmanagedType.IUnknown)]
			object QueryService(ref Guid guidService, ref Guid riid);
		}
		static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");

		#endregion

		#region Initialization
		/// <summary>
		/// Constructor.
		/// Interaction with the view is done via the viewmodel so the DataContext
		/// is set to it.
		/// </summary>
		public TeacherView()
		{
			InitializeComponent();

			// set up the view model and switch the data context to it.
			_viewmodel = new TeacherViewModel();
			DataContext = _viewmodel;

			_Teachers.ApplyTemplate();
			_scrollviewer = FindVisualChild<ScrollViewer>(_Teachers);

			// Disable navigating when clicking on a link
			_browser.Navigating += (o, e) =>
				{
					if (_browserLock)
					{
						e.Cancel = true;
					}
				};

			// Disable opening a new window.
			IServiceProvider _serviceProvider = null;

			_browser.Navigated += (o, e) =>
				{
					if (_browser.Document != null)
					{
						_browserLock = true;

						_serviceProvider = (IServiceProvider)_browser.Document; //will throw exception if Document is null

						Guid _serviceGuid = SID_SWebBrowserApp;
						Guid _iid = typeof(SHDocVw.IWebBrowser2).GUID;

						// Get the native browser
						SHDocVw.IWebBrowser2 _webBrowser = (SHDocVw.IWebBrowser2)_serviceProvider.QueryService(ref _serviceGuid, ref _iid);

						HTMLDocument doc = _webBrowser.Document as HTMLDocument;
						mshtml.HTMLDocumentEvents2_Event ev = doc as mshtml.HTMLDocumentEvents2_Event;

						// Handle a NewWindow Event
						SHDocVw.DWebBrowserEvents_Event wbEvents = (SHDocVw.DWebBrowserEvents_Event)_webBrowser;
						wbEvents.NewWindow -= NewWindowHandler;
						wbEvents.NewWindow += NewWindowHandler;
					}
				};
		}

		private void NewWindowHandler(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed)
		{
			Processed = true;
		}

		/// <summary>
		/// Finds child elements within a given item. 
		/// </summary>
		/// <typeparam name="childItem">Type of item to find</typeparam>
		/// <param name="obj">Parent item</param>
		/// <returns>Child item of given type</returns>
		private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);

				if (child != null && child is childItem)
				{
					return (childItem)child;
				}
				else
				{
					childItem childOfChild = FindVisualChild<childItem>(child);
					if (childOfChild != null)
					{
						return childOfChild;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Sets control dimensions
		/// </summary>
		/// <param name="width"></param>
		/// <param name="heigth"></param>
		public void SetDimensions(int width, int height)
		{
			//set dimensions
			this.Width = width;
			this.Height = height;

			// count the number of lines and columns of items given the contorls dimensions
			_columnCount = (int)this.Width / 340;
			_lineCount = (int)this.Height / 170;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ControlLoaded()
		{
			_scrollviewer.ScrollToHome();
		}


		#endregion

		#region Input Handling
		/// <summary>
		/// Handles a mouse down event. Gets its position on the screen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
		{
			_mousePosition = e.GetPosition(this);

			_browsing = false;
			_browser.Visibility = System.Windows.Visibility.Hidden;
		}

		/// <summary>
		/// Handles a Mouse Up event. Resets the saved mouse position.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBox_MouseUp(object sender, MouseButtonEventArgs e)
		{
			_mousePosition.X = -1;
			_mousePosition.Y = -1;
		}

		/// <summary>
		/// Handles a mouse move event. Allows scrolling the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (_mousePosition.Y > 0)
			{
				// Get mouse position
				double dMouseY = e.GetPosition(this._Teachers).Y;

				// Calculate offset
				double dDeltaY = dMouseY - _mousePosition.Y;
				double dCurOffset = _scrollviewer.VerticalOffset;
				double dNew = dCurOffset - dDeltaY;

				// Scroll into position
				_scrollviewer.ScrollToVerticalOffset(dNew);

				// Update mouse value
				_mousePosition.Y = dMouseY;
			}
		}

		/// <summary>
		/// Handles a double-click on a teacher. Opens up his web page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Listbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//fetch the selected teacher item.
			TeacherItemView t = _Teachers.SelectedItem as TeacherItemView;
			if (t != null)
			{
				//ensure the browser is visible
				_browser.Visibility = System.Windows.Visibility.Visible;
				_browser.Focus();

				//display the webpage.
				string url = t.GetWebpage().Substring(5, t.GetWebpage().Length - 5);

				if (!String.IsNullOrEmpty(url))
				{
					_browserLock = false;
					_browser.Navigate(new Uri(url));
					_browsing = true;
					_htmlDoc = _browser.Document as mshtml.HTMLDocument;
				}
			}
		}
		#endregion

		#region Animation
		/// <summary>
		/// Triggers the beggining of the animation.
		/// </summary>
		public void StartAnimation()
		{
			_offset = 0.0;
			_scrollviewer.ScrollToHome();
			_animationTimer = new Timer(new TimerCallback(_animationTimer_Tick), null, 500, 33);
		}

		/// <summary>
		/// Aborts a running animation.
		/// </summary>
		public void StopAnimation()
		{
			if (_animationTimer != null)
			{
				_animationTimer.Dispose();
			}
		}

		/// <summary>
		/// Handles the control's animation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _animationTimer_Tick(object state)
		{
			if (_scrollviewer.ExtentHeight > _offset)
			{
				//while not at end, scroll down
				_offset += 1.50;
				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
				{
					_scrollviewer.ScrollToVerticalOffset(_offset);
				}));

			}
			else
			{
				//once finished, stop timer, reset and trigger AnimationEnd
				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
				{
					if (AnimationEnd != null) AnimationEnd(this, null);
					_animationTimer.Dispose();

					_scrollviewer.ScrollToHome();
				}));

			}
		}
		#endregion

		#region Interaction
		/// <summary>
		/// Handles a tap from the mobile device.
		/// When user is in selection mode, opens the browser with the teachers
		/// web page loaded.
		/// </summary>
		public void Tap()
		{
			if (_selecting)
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "TAP on Teachers. Loading browser");

				//fetch the selected teacher item.
				TeacherItemView t = _Teachers.SelectedItem as TeacherItemView;
				if (t != null)
				{
					//ensure the browser is visible
					_browser.Visibility = System.Windows.Visibility.Visible;
					_browser.Focus();

					//display the webpage.
					string url = t.GetWebpage().Substring(5, t.GetWebpage().Length - 5);

					if (!String.IsNullOrEmpty(url))
					{
						_browserLock = false;
						_browser.Navigate(new Uri(url));
						_browsing = true;
						_htmlDoc = _browser.Document as mshtml.HTMLDocument;
					}
				}
			}
			else 
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "TAP on Teachers, but not in Selection Mode.");
			}
		}

		/// <summary>
		/// Handles a long press gesture from the mobile device.
		/// Toggles selection mode.
		/// </summary>
		public void LongPress()
		{
			if (!_selecting)
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "LONGPRESS on Teachers, Selection Mode on.");

				//select the item closest to the middle of the screen
				_selectedLine = (_lineCount / 2) + ((int)_scrollviewer.VerticalOffset / 170);
				_selectedColumn = _columnCount / 2;

				//set focus on the listbox.
				_Teachers.Focus();

				// highlight the selected item
				_Teachers.SelectedIndex = _selectedLine * _columnCount + _selectedColumn;

				_selecting = true;
				return;
			}
			else
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "LONGPRESS on Teachers, Selection Mode/Browsing off.");
				_selecting = false;
				_browsing = false;

				// Hide the browser
				_browser.Focus();
				_browser.Visibility = System.Windows.Visibility.Hidden;
			}
		}

		/// <summary>
		/// Handles a scrolling gesture from the mobile device.
		/// Scrolls the teacher list when not in selection mode, or the web 
		/// page, when visible.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Scroll(float x, float y)
		{
			if (!_selecting)
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "SCROLL on Teachers. Scrolling list.");

				_scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset + y * 2);
			}

			if (_browsing)
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "SCROLL on Teachers. Scrolling browser.");

				if (_htmlDoc != null) _htmlDoc.parentWindow.scrollBy((int)x, (int)y);
			}
		}

		/// <summary>
		/// Handles a fling gesture in the mobile device.
		/// Accelerates scrolling in the teacher list or web page, and switches
		/// the current selection.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Fling(float x, float y)
		{
			// if in selection mode
			if (_selecting)
			{
				//if browsing fling the web page.
				if (_browsing)
				{
					if (Math.Abs(y) > Math.Abs(x))
					{
						_time = 0;
						_flingSpeed = y;
						_scrollValue = y;

						_flingTimer = new Timer(new TimerCallback(_flingTimer_Tick), null, 0, 25);

						DetiInteract.Logger.Log.Instance.Write(this, "USER", "FLING on Teachers. Scrolling browser.");

						return;
					}
				}

				// focus on the control so the user can see the highlight
				_Teachers.Focus();

				DetiInteract.Logger.Log.Instance.Write(this, "USER", "FLING on Teachers in Selection mode. Changing selection.");

				// if vertical scrolling
				if (Math.Abs(y) > Math.Abs(x))
				{
					if (y < 0)
					{
						_selectedLine -= 1;
						if (_selectedLine < 0)
						{
							_selectedLine = 0;
						}
					}
					else
					{
						_selectedLine += 1;
						if (_selectedLine >= (_Teachers.Items.Count / _columnCount))
						{
							_selectedLine = _Teachers.Items.Count / _columnCount;
						}
					}
				}
				// if horizontal scrolling
				else
				{
					if (x > 0)
					{
						_selectedColumn += 1;
						if (_selectedColumn == _columnCount)
						{
							_selectedColumn = 0;
						}
					}
					else
					{
						_selectedColumn -= 1;
						if (_selectedColumn < 0)
						{
							_selectedColumn = _columnCount - 1;
						}
					}
				}

				// Highlight the selected item.
				_Teachers.SelectedIndex = _selectedLine * _columnCount + _selectedColumn;
				_Teachers.ScrollIntoView(_Teachers.Items[_Teachers.SelectedIndex]);
			}
			else
			{
				// A vertical fling scrolls the teacher list with acceleration.
				if (Math.Abs(y) > Math.Abs(x))
				{
					DetiInteract.Logger.Log.Instance.Write(this, "USER", "FLING on Teachers. Scrolling list.");

					_selecting = false;

					_time = 0;
					_flingSpeed = y;
					_scrollValue = y;

					_flingTimer = new Timer(new TimerCallback(_flingTimer_Tick), null, 0, 25);

					return;
				}
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public void Rotation(float x, float y, float z)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scale"></param>
		public void Zoom(float scale)
		{
		}
		/// <summary>
		/// Handles the fling timer callback.
		/// Deccelerates scrolling of the teacher list or web page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _flingTimer_Tick(object state)
		{
			// keep going while the speed is fast enough.
			if (_scrollValue > 0.5)
			{
				_scrollValue = (_flingSpeed - (5 * _time)) / 100;
			}
			else if (_scrollValue < -0.5)
			{
				_scrollValue = (_flingSpeed + (5 * _time)) / 100;
			}
			//otherwise, stop.
			else
			{
				_flingSpeed = 0;
				_scrollValue = 0;

				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
				{
					_flingTimer.Dispose();
				}));
			}

			_time += 10f;

			// scroll the browser or the listbox.
			Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
			{
				if (_browsing)
				{
					if (_htmlDoc != null) _htmlDoc.parentWindow.scrollBy(0, (int)-_scrollValue);
				}
				else
				{
					Scroll(0, -_scrollValue);
				}
			}));
		}

		#endregion

		public bool CanSwitchPage()
		{
			return true;
		}
	}
}
